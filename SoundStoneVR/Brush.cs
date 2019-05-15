using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Valve.Newtonsoft.Json.Utilities;
using Valve.VR;
using Valve.VR.InteractionSystem;
using SoundStone;

namespace SoundStone.SoundSystem
{
    [RequireComponent(typeof(PlayerToolkit))]
    [RequireComponent(typeof(VRController_InputHandler))]
    public class Brush : MonoBehaviour
    {
        [Header("Prefabs")]
        [Tooltip("Prefab object for geometric soundpoint")]
        public GameObject prefabGeometric;

        [Tooltip("Prefab object for sinewave soundpoint")]
        public GameObject prefabSineStone;

        [Header("sinwave Properties")]
        [Tooltip("Sinwave frequency multiplier")]
        public float sinFrequencyMultiplier = 0.5f;

        [Header("LineRenderer Properties")]
        public float lineRendererWidth;

        public float lineRendererSmoother;

        public Material LineRendererMaterial;

        public Color[] LineRendererColor; 

        //        [Tooltip("Prefab object for vector sound points in line tool")]
        //        public GameObject prefabVectorPoint;

        //        [Tooltip("Prefab parent object for prefabVectorPoint")]
        //        public GameObject prefabSoundLine;

        //        [Tooltip("The rate at which sound points are spawned along the horizontal axis")]
        //        public float sampleRate;

        protected VRController_InputHandler VRInputHandler;
        protected Player player = null;
        protected Recorder recorder = null;
        protected SinGenerator sinGenerator;
        protected AudioClip recordedAudio = null;
        protected float sinFrequencyinitYPos;
        protected int lineRendererUpdatePosition;

        protected GameObject tmpGameObject;

//        protected GameObject tempParentGameObject;

        private float startTime = 0;
        private float stopTime = 0;
        private float trackedFrequency;

        // Start is called before the first frame update

        void Start()
        {
            player = Player.instance;
            if (player == null)
            {
                Debug.LogError("<b>[SteamVR Interaction]</b> No Player instance found in map.");
                Destroy(this.gameObject);
                return;
            }

            recorder = Recorder.instance;
            if (recorder == null)
            {
                Debug.LogWarning("<b>[SoundStone SoundSystem]</b> Couldn't find Recorder instance in map");
            }

            VRInputHandler = VRController_InputHandler.instance;
            if (VRInputHandler == null)
            {
                Debug.LogError("<b>[SoundStone ControlTrigger]</b> Couldn't find Input Handler instance in map.");
                Destroy(this.gameObject);
                return;
            }

            sinGenerator = SinGenerator.instance;
            if (sinGenerator == null)
            {
                Debug.LogError("<b>[SoundStone AudioSystem]</b> Couldn't find SinGenerator instance");
                return;
            }
        }


        protected GameObject InstantiateAudioSourcePrefab(GameObject prefabAudioSource, AudioClip audioClip,
            Transform initTransform, float duration = 0)
        {
            GameObject vectorSoundPoint = Instantiate(prefabAudioSource, initTransform.position,
                initTransform.localRotation);
            if (audioClip != null)
            {
                var trimmedClip = recorder.MakeSubclip(audioClip, duration);

                var audioSource = vectorSoundPoint.GetComponent<AudioSource>();
                audioSource.clip = trimmedClip;
            }

            return vectorSoundPoint;
        }

        public void DrawGeometricPrefab(Hand hand, SteamVR_Action_Boolean triggerAction)
        {
            if (prefabGeometric == null)
            {
                Debug.LogError("<b>[SoundStone SoundSystem]</b> prefabGeometric returns null");
                return;
            }

            if (VRInputHandler.WasButtonPressed(hand, triggerAction))
            {
                lineRendererUpdatePosition = 1;
                recordedAudio = recorder.RecordAudio();
                startTime = Time.time;
                tmpGameObject = InstantiateAudioSourcePrefab(prefabGeometric, null, hand.transform);
                var linerenderer = tmpGameObject.AddComponent<LineRenderer>();
                linerenderer.positionCount = 1;
                linerenderer.SetPosition(0, hand.transform.position);
                linerenderer.material = LineRendererMaterial;
                linerenderer.widthMultiplier = lineRendererWidth;
            }

            if (VRInputHandler.IsButtonPressed(hand, triggerAction))
            {
                LineRendererUpdate(hand, tmpGameObject.GetComponent<LineRenderer>(), lineRendererUpdatePosition);
            }

            else if (VRInputHandler.WasButtonReleased(hand, triggerAction))
            {
                if (recorder.IsRecording())
                {
                    stopTime = Time.time;
                    var duration = stopTime - startTime;
                    recorder.EndRecording();
                    tmpGameObject.GetComponent<AudioSource>().clip = recorder.MakeSubclip(recordedAudio, duration);
                    tmpGameObject.GetComponent<Soundblob>().duration = duration;
                    var tmpPositionArray = new Vector3[lineRendererUpdatePosition];
                    tmpGameObject.GetComponent<LineRenderer>().GetPositions(tmpPositionArray);
                    tmpGameObject.GetComponent<Soundblob>().positionList = tmpPositionArray.ToList();
                }
            }
        }

        protected void LineRendererUpdate(Hand hand, LineRenderer lineRenderer,
            int lineRendererPosition)
        {
            if (lineRenderer.GetPosition(lineRendererPosition - 1) == null || lineRenderer == null)
            {
                Debug.LogError("<b>[SoundStone SoundSystem]</b> lineRenderer has not been initialized properly");
                return;
            }

            // Only track position if distance change is greater than 1
            var distancePrevious = Vector3.Distance(lineRenderer.GetPosition(lineRendererPosition - 1),
                hand.transform.position);
            if (distancePrevious > lineRendererSmoother)
            {
                // Debug.Log(lineRendererPosition);
                lineRenderer.positionCount++;
                lineRenderer.SetPosition(lineRendererPosition, hand.transform.position);
                lineRendererUpdatePosition++;
            }
        }

        public void DrawSinwave(Hand hand, SteamVR_Action_Boolean triggerAction, float amp = 0.05f)
        {
            if (prefabSineStone == null)
            {
                Debug.LogWarning("<b>[SoundStone SoundSystem]</b> No prefab object specified");
                return;
            }

            if (VRInputHandler.WasButtonPressed(hand, triggerAction))
            {
                sinFrequencyinitYPos = hand.transform.position.y;
                Debug.Log(sinFrequencyinitYPos);
                tmpGameObject = InstantiateAudioSourcePrefab(prefabSineStone, null, hand.transform);
                tmpGameObject.GetComponent<Soundblob>().initSinFrequency = 1100;
                tmpGameObject.GetComponent<Soundblob>().oldHandyPosition = hand.transform.position.y;
                tmpGameObject.GetComponent<Soundblob>().sinAmplitude = amp;
                tmpGameObject.GetComponent<AudioSource>().clip =
                    sinGenerator.GenerateSinusoidalClip(tmpGameObject.GetComponent<Soundblob>().ModifyDataStream, 1);
                tmpGameObject.GetComponent<AudioSource>().Play();
            }

            if (VRInputHandler.IsButtonPressed(hand, triggerAction))
            {
                tmpGameObject.transform.position = hand.transform.position;
                var frequencyScaler =  hand.transform.position.y;
                var handyTransform = tmpGameObject.transform.GetComponent<Soundblob>().oldHandyPosition - hand.transform.position.y;
                if (handyTransform > 0.05 || handyTransform < -0.05)
                {
                    tmpGameObject.GetComponent<Soundblob>().sinFrequency = tmpGameObject.GetComponent<Soundblob>().initSinFrequency * frequencyScaler * sinFrequencyMultiplier;

                    if (tmpGameObject.GetComponent<Soundblob>().sinFrequency < 300)
                        tmpGameObject.GetComponent<Soundblob>().sinFrequency = 300;
                    else if (tmpGameObject.GetComponent<Soundblob>().sinFrequency > 2200)
                        tmpGameObject.GetComponent<Soundblob>().sinFrequency = 2200;
                    Debug.Log(tmpGameObject.GetComponent<Soundblob>().sinFrequency);
                    tmpGameObject.GetComponent<Soundblob>().oldHandyPosition = hand.transform.position.y;
                }
                if (!tmpGameObject.GetComponent<AudioSource>().isPlaying)
                    tmpGameObject.GetComponent<AudioSource>().Play();
            }
        }

        public void EraseSoundPoint(Hand hand, SteamVR_Action_Boolean triggerAction)
        {
            if (VRInputHandler.IsButtonPressed(hand, triggerAction))
            {
                if (hand.hoveringInteractable.gameObject != null)
                {
                    var hoveringGameObject = hand.hoveringInteractable.gameObject;
                    var hoveringObjectAudioSource = hoveringGameObject.GetComponent<AudioSource>();
                    if (hoveringObjectAudioSource.isPlaying)
                    {
                        hoveringObjectAudioSource.Stop();
                    }

                    // Deactivate GameObject. For purposes of restoring functionality in future
                    hoveringGameObject.SetActive(false);
                }
                else return;
            }
        }

        // Dependent IEnumerator method for DrawLinearPrefab

        /*protected IEnumerator InstantiateVectorPoint(GameObject prefabGameObject, Transform parentTransform, Hand hand)
        {
            while (VRInputHandler.IsButtonPressed(hand, brushAction))
            {
                InstantiateAudioSourcePrefab(prefabGameObject, null, hand.transform, parentTransform: parentTransform);
                yield return new WaitForSeconds(sampleRate);
            }
        }*/

        // Feature may be incorporated in future builds

        /*private void DrawLinearPrefab(Hand hand, GameObject prefabVectorPoint, float sampleRate)
        {
            if (prefabVectorPoint == null)
            {
                Debug.LogError("<b>[SoundStone]</b>Couldn't find prefabVectorPoint");
                return;
            }

            if (VRInputHandler.WasButtonPressed(hand, brushAction))
            {
                tempParentGameObject = Instantiate(prefabSoundLine, hand.transform.position, Quaternion.identity);
                recordedAudio = recorder.RecordAudio();
                startTime = Time.time;
                StartCoroutine(InstantiateVectorPoint(prefabVectorPoint, tempParentGameObject.transform, hand));
            }
            else if (VRInputHandler.WasButtonReleased(hand, brushAction))
            {
                if (recorder.IsRecording())
                {
                    stopTime = Time.time;
                    recorder.EndRecording();
                    var audioSourceList = tempParentGameObject.GetComponentsInChildren<AudioSource>();
                    var timeBuffer = 0f;
                    foreach (var audioSource in audioSourceList)
                    {
                        audioSource.clip = recorder.MakeSubclip(recordedAudio, timeBuffer, timeBuffer + sampleRate);
                    }
                }
            }
        }*/
    }
}