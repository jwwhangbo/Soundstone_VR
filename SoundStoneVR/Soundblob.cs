using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.XR.WSA;
using Valve.VR;
using Valve.VR.InteractionSystem;
using LineRenderer = UnityEngine.LineRenderer;
using Quaternion = UnityEngine.Quaternion;
using Random = System.Random;
using Vector3 = UnityEngine.Vector3;

namespace SoundStone.SoundSystem
{
    [RequireComponent(typeof(Interactable))]
    public class Soundblob : MonoBehaviour
    {
        [Tooltip("rotation angle per second")] public int degreesPerSecond;
        [Tooltip("Rotation speed multiplier")] public float rotationMultiplier;

        public Light pointLight;

        private List<Vector3> _positionList;
        private AudioSource audioSource;
        private Interactable interactable;
        private float _sinAmplitude = 1;
        private float _sinFrequency = 1;
        private float _duration;

        public List<Vector3> positionList
        {
            get => _positionList;
            set => _positionList = value;
        }

        public float duration
        {
            get => _duration;
            set => _duration = value;
        }

        public float sinAmplitude
        {
            get => _sinAmplitude;
            set => _sinAmplitude = value;
        }

        private float _initSinFrequency;

        public float initSinFrequency { get => _initSinFrequency; set => _initSinFrequency = value; }

        private float _oldHandyPosition;

        public float oldHandyPosition { get => _oldHandyPosition ; set => _oldHandyPosition = value; }

        public float sinFrequency
        {
            get => _sinFrequency;
            set => _sinFrequency = value;
        }

        void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            interactable = GetComponent<Interactable>();
            pointLight = GetComponentInChildren<Light>();
        }

        void Update()
        {
            // Debug.Log(sinFrequency);
            if (GetComponent<LineRenderer>() != null && audioSource.isPlaying)
                SetPosition();
            RotationModifier(degreesPerSecond);
        }

        protected void Play(Hand hand)
        {
            if (hand.handType == SteamVR_Input_Sources.RightHand)
            {
                if (audioSource.isPlaying)
                {
                    return;
                }

                audioSource.Play();
                SetLightActive(true);
            }
            else if (hand.handType == SteamVR_Input_Sources.LeftHand)
            { 
                if (audioSource.isPlaying)
                {
                    audioSource.Pause();
                    SetLightActive(false);
                }
            }
        }

        private void SetLightActive(bool triggerBool)
        {
            pointLight.gameObject.active = triggerBool;
        }

        private void HandHoverUpdate(Hand hand)
        {
            Play(hand);
        }

        private void SetPosition()
        {
            var playPositionFraction = GetComponent<AudioSource>().time / duration;
            var playPositionIndex = (int) (playPositionFraction * GetComponent<LineRenderer>().positionCount);
            Debug.Log(playPositionIndex);
            gameObject.transform.position = positionList[playPositionIndex];
        }

        public void ModifyDataStream(float[] data)
        {
            var length = data.Length;
            for (var i = 0; i < length; i++)
            {
                data[i] = sinAmplitude * Mathf.Sin(i * sinFrequency);
            }
        }

        private void RotationModifier(int rotationRate)
        {
            Random rand = new Random();
            transform.Rotate(rotationMultiplier * Time.deltaTime * (float) rand.Next(rotationRate),
                rotationMultiplier * Time.deltaTime * (float) rand.Next(rotationRate),
                rotationMultiplier * Time.deltaTime * (float) rand.Next(rotationRate));
        }

        private void UpdateLoopingIndicator(Image indicatorImage, TextMesh indicatorText = null)
        {
            if (indicatorImage != null)
            {
                if (audioSource.loop)
                    indicatorImage.IsActive();

            }
            else if (indicatorText != null)
            {

            }
            else return;
        }
    }
}