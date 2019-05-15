using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using SoundStone.SoundSystem;
using SoundStone;

namespace Valve.VR.InteractionSystem
{
    [RequireComponent(typeof(Interactable))]
    public class Moveable : MonoBehaviour
    {
        [EnumFlags] [Tooltip("The flags used to attach this object to the hand.")]
        public Hand.AttachmentFlags attachmentFlags = Hand.AttachmentFlags.ParentToHand |
                                                      Hand.AttachmentFlags.DetachFromOtherHand |
                                                      Hand.AttachmentFlags.TurnOnKinematic;

        [Tooltip("The local point which acts as a positional and rotational offset to use while held")]
        public Transform attachmentOffset;

        [Tooltip("When detaching the object, should it return to its original parent?")]
        public bool restoreOriginalParent = false;

        public bool enablePitchModify = true;

        public float defaultPitch = 1.0f;

        public float pitchModifier = 1.0f;

        protected bool attached = false;
        protected float attachTime;
        protected Vector3 attachPosition;
        protected Quaternion attachRotation;
        protected Transform attachEaseInTransform;
        protected PlayerToolkit playerToolkit;
        protected AudioSource audioSource;

        public UnityEvent onPickUp;
        public UnityEvent onDetachFromHand;
        public UnityEvent<Hand> onHeldUpdate;

        

        protected RigidbodyInterpolation hadInterpolation = RigidbodyInterpolation.None;

        [HideInInspector] public Interactable interactable;

        protected virtual void Awake()
        {
            interactable = GetComponent<Interactable>();

            playerToolkit = PlayerToolkit.instance;

            audioSource = GetComponentInParent<AudioSource>();

//            stationaryPosition = this.transform.localPosition;

            if (attachmentOffset != null)
            {
                // remove?
                //interactable.handFollowTransform = attachmentOffset;
            }
        }

        public float audioClipPitch
        {
            get { return audioSource.pitch; }
            set { audioSource.pitch = value; }
        }

        // -----------------------------------
        // Obsolete methods for altering pitch based on y-axis displacement.
        // will be removed in future updates
        // -----------------------------------

        /*protected float GetVerticalDisplaceScalar(Vector3 startingPos, Vector3 updatedPos)
        {
            var verticalDisplacement = updatedPos.y - startingPos.y / updatedPos.y;
            return verticalDisplacement;
        }

        protected void ModifyPitchBasedOnVerticalVectorDisplacement(float verticalDisplacement, bool enablePitchModify)
        {
            if (enablePitchModify)
            {
                audioSource.pitch = defaultPitch + verticalDisplacement * pitchModifier;
            }
        }

        protected void UpdateStationaryPosition(Vector3 updatePosition)
        {
            stationaryPosition = updatePosition;
        }*/

        protected virtual void OnHandHoverBegin(Hand hand)
        {
            GrabTypes bestGrabType = hand.GetBestGrabbingType();
        }


        //-------------------------------------------------
        protected virtual void OnHandHoverEnd(Hand hand)
        {
            hand.HideGrabHint();
        }


        //-------------------------------------------------
        protected virtual void HandHoverUpdate(Hand hand)
        {
            GrabTypes startingGrabType = GrabTypes.None;
            startingGrabType = hand.GetGrabStarting(GrabTypes.Grip);

            if (startingGrabType != GrabTypes.None && PlayerToolkit.selectedTool == SoundStone_ToolTypes.Handtool)
            {
                hand.AttachObject(gameObject, startingGrabType, attachmentFlags, attachmentOffset);
                hand.HideGrabHint();
            }
        }

        //-------------------------------------------------
        protected virtual void OnAttachedToHand(Hand hand)
        {
            //Debug.Log("<b>[SteamVR Interaction]</b> Pickup: " + hand.GetGrabStarting().ToString());

            attached = true;

            onPickUp.Invoke();

            hand.HoverLock(GetComponent<Interactable>());

            attachTime = Time.time;
            attachPosition = transform.position;
            attachRotation = transform.rotation;
        }


        //-------------------------------------------------
        protected virtual void OnDetachedFromHand(Hand hand)
        {
            attached = false;

            onDetachFromHand.Invoke();

            hand.HoverUnlock(GetComponent<Interactable>());

//            UpdateStationaryPosition(this.transform.localPosition);
        }

        protected virtual void HandAttachedUpdate(Hand hand)
        {
            if (hand.IsGrabEnding(this.gameObject))
            {
                hand.DetachObject(gameObject, restoreOriginalParent);

                // Uncomment to detach ourselves late in the frame.
                // This is so that any vehicles the player is attached to
                // have a chance to finish updating themselves.
                // If we detach now, our position could be behind what it
                // will be at the end of the frame, and the object may appear
                // to teleport behind the hand when the player releases it.
                //StartCoroutine( LateDetach( hand ) );
            }

            if (onHeldUpdate != null)
                onHeldUpdate.Invoke(hand);

//            var verticalDisplacement = GetVerticalDisplaceScalar(stationaryPosition, hand.transform.position);

//            Debug.Log(verticalDisplacement);

//            ModifyPitchBasedOnVerticalVectorDisplacement(verticalDisplacement, enablePitchModify);
        }


        //-------------------------------------------------
        protected virtual IEnumerator LateDetach(Hand hand)
        {
            yield return new WaitForEndOfFrame();

            hand.DetachObject(gameObject, restoreOriginalParent);
        }


        //-------------------------------------------------
        protected virtual void OnHandFocusAcquired(Hand hand)
        {
            gameObject.SetActive(true);
        }


        //-------------------------------------------------
        protected virtual void OnHandFocusLost(Hand hand)
        {
            gameObject.SetActive(false);
        }
    }

    public enum ReleaseStyle
    {
        NoChange,
        GetFromHand,
        ShortEstimation,
        AdvancedEstimation,
    }
}