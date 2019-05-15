using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using SoundStone;
using SoundStone.SoundSystem;
using UnityEngine;
using UnityEngine.SceneManagement;
using Valve.VR;
using Valve.VR.InteractionSystem;
using Vector3 = System.Numerics.Vector3;

namespace SoundStone
{
    public class VRController_InputHandler : MonoBehaviour
    {
        [Header("Controller Mapping")]
        public SteamVR_Action_Boolean grabPinch = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("GrabPinch");
        public SteamVR_Action_Boolean grabGrip = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("GrabGrip");
//        public SteamVR_Action_Boolean interactUI = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("InteractUI");
//        public SteamVR_Action_Boolean openUI = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("OpenUI");
//        public SteamVR_Action_Boolean trackpadUp = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("TrackpadUp");
//        public SteamVR_Action_Boolean trackpadDown = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("TrackpadDown");
//        public SteamVR_Action_Boolean trackpadLeft = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("TrackpadLeft");
//        public SteamVR_Action_Boolean trackpadRight = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("TrackpadRight");
        public SteamVR_Action_Boolean trackpadTouch = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("TrackpadTouch");
        public SteamVR_Action_Boolean trackpadPressed = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("TrackpadPressed");
        public SteamVR_Action_Vector2 trackpadPos = SteamVR_Input.GetAction<SteamVR_Action_Vector2>("TrackpadPos");
        public SteamVR_Action_Single squeeze = SteamVR_Input.GetAction<SteamVR_Action_Single>("Squeeze");

        [Header("Z-axis movement properties")]
        public float movementMultiplier = 1f;

        protected Player player;
        protected Rigidbody playerRigidbody;
        protected Brush brush;

        protected List<Transform> handTransformList;
        protected Transform initHandTransform;
        protected GameObject tmpSoundblob;

        private float startTime;
        private float duration;

        void Start()
        {
            player = Player.instance;
            if (player == null)
            {
                Debug.LogError("<b>[SteamVR Interaction]</b> No Player instance found in map.");
                Destroy(this.gameObject);
                return;
            }

            playerRigidbody = player.gameObject.GetComponentInChildren<Rigidbody>();
            brush = FindObjectOfType<Brush>();
        }

        void Update()
        {
            // Handle method calling depending on PlayerToolkit.selectedTool
            switch (PlayerToolkit.selectedTool)
            {
                case SoundStone_ToolTypes.Brush:
                    brush.DrawGeometricPrefab(player.rightHand, grabPinch);
                    break;
                case SoundStone_ToolTypes.SineTool:
                    brush.DrawSinwave(player.rightHand, grabPinch);
                    break;
                case SoundStone_ToolTypes.Eraser:
                    brush.EraseSoundPoint(player.rightHand, grabPinch);
                    break;
                case SoundStone_ToolTypes.Conductor:
                    TriggerLoop(player.rightHand, grabPinch);
                    break;
                case SoundStone_ToolTypes.Flying:
                    MoveZAxis(player.rightHand, grabPinch);
                    break;
            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            }
        }

        //Can only be one instance of Input Handler
        private static VRController_InputHandler _instance;
        private float initialHandyPosition;
        private float initialPlayeryPosition;
        private float oldHandyPosition;

        public static VRController_InputHandler instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<VRController_InputHandler>();
                }

                return _instance;
            }
        }

        public bool AssertHand(Hand hand)
        {
            if (hand == null)
            {
                return false;
            }

            if (!hand.gameObject.activeInHierarchy)
            {
                return false;
            }

            return true;
        }

        public bool WasButtonPressed(Hand hand, SteamVR_Action_Boolean booleanAction)
        {
            if (AssertHand(hand))
            {
                return booleanAction.GetStateDown(hand.handType);
            }

            return false;
        }

        public bool WasButtonReleased(Hand hand, SteamVR_Action_Boolean booleanAction)
        {
            if (AssertHand(hand))
            {
                return booleanAction.GetStateUp(hand.handType);
            }

            return false;
        }

        public bool IsButtonPressed(Hand hand, SteamVR_Action_Boolean booleanAction)
        {
            if (AssertHand(hand))
            {
                return booleanAction.GetState(hand.handType);
            }

            return false;
        }

        public float GetSqueezeVector(Hand hand, SteamVR_Action_Single squeezeAction)
        {
            if (AssertHand(hand))
            {
                return squeezeAction.GetAxis(hand.handType);
            }

            return 0.0f;
        }

        public UnityEngine.Vector2 GetTrackpadPos (Hand hand)
        {
            if (AssertHand(hand))
            {
                return trackpadPos.GetAxis(hand.handType);
            }

            return new UnityEngine.Vector2(0, 0);
        }

        protected void MoveZAxis(Hand hand, SteamVR_Action_Boolean triggerAction)
        {
            if (WasButtonPressed(hand, triggerAction))
            {
                initialHandyPosition = hand.transform.position.y;
                initialPlayeryPosition = player.transform.position.y;
                oldHandyPosition = initialHandyPosition;
            }
            if (IsButtonPressed(hand, triggerAction))
            {
                var handyTransform = initialHandyPosition - hand.transform.position.y;
                // UnityEngine.Vector3 rotationBasedVector3 = new UnityEngine.Vector3(0, -hand.transform.rotation.z, 0);
                if (hand.transform.position.y - oldHandyPosition > 0.01 || hand.transform.position.y - oldHandyPosition < -0.01)
                {
                    oldHandyPosition = hand.transform.position.y;
                    player.transform.position = new UnityEngine.Vector3(player.transform.position.x,
                    initialPlayeryPosition + handyTransform * movementMultiplier, player.transform.position.z);
                }
                
            }
        }

        protected void MeasureButtonPressTime(Hand hand, SteamVR_Action_Boolean buttonAction)
        {
            if (WasButtonPressed(hand, buttonAction))
                startTime = Time.time;

            if (WasButtonReleased(hand, buttonAction))
            {
                duration = Time.time - startTime;
//                Debug.Log("Buttonpress duration: " + duration);
//                Debug.Log("deltaTime: " + Time.deltaTime);
            }

        }

        protected void TriggerLoop(Hand hand, SteamVR_Action_Boolean triggerAction)
        {
            if (WasButtonPressed(hand, triggerAction))
            {
                if (hand.hoveringInteractable != null)
                {
                    tmpSoundblob = hand.hoveringInteractable.gameObject;
                    tmpSoundblob.GetComponent<AudioSource>().loop = !tmpSoundblob.GetComponent<AudioSource>().loop;
                }
                else
                {
                    return;
                }
            }
        }

        protected void RecordHandTransform(Hand hand, SteamVR_Action_Boolean buttonAction)
        {
            if (WasButtonPressed(hand, buttonAction))
            {
                initHandTransform = hand.transform; 
            }

            if (IsButtonPressed(hand, buttonAction))
            {
                handTransformList.Add(hand.transform);
            }
        }
    }
}