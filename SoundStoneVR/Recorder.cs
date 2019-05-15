using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;
using SoundStone;

namespace SoundStone.SoundSystem
{
    [RequireComponent(typeof(PlayerToolkit))]
    public class Recorder : MonoBehaviour
    {
        // Default audio device set to first device detected
        public int audioDeviceIndexer = 0;
        public bool debugAudioDevices;
        public static string[] audioDeviceList = null;

        [Tooltip("Don't enter this field unless you know the name of the device")]
        public string selectedDevice = null;

        private PlayerToolkit playerToolkit;

        void Awake()
        {
            playerToolkit = GetComponentInParent<PlayerToolkit>();
        }

        void Start()
        {
            audioDeviceList = Microphone.devices;
            if (!audioDeviceList.Any())
            {
                Debug.LogWarning("<b>[SoundStone]</b> No Usable Audio Source Found!");
            }
            else
            {
                selectedDevice = audioDeviceList[audioDeviceIndexer];
                if (debugAudioDevices)
                    DebugAudioDeviceList(audioDeviceList);
            }
        }


        // Recorder.instance should be a global variable
        private static Recorder _instance;

        public static Recorder instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<Recorder>();
                }

                return _instance;
            }
        }

        protected void DebugAudioDeviceList(string[] audioDeviceList)
        {
            foreach (var device in audioDeviceList)
            {
                Microphone.GetDeviceCaps(device, out var minFreq, out var maxFreq);
                Debug.Log("Device: " + device);
                if (minFreq == maxFreq)
                    Debug.Log("For any frequency range");
                else
                    Debug.Log(string.Format("For minFreq: {0} maxFreq: {1}", minFreq, maxFreq));
            }

            Debug.Log(string.Format("Currently using {0} as audio input device", selectedDevice));
        }

        public AudioClip RecordAudio(int length = 60, int recordFreq = 44100, bool loop = true)
        {
            if (selectedDevice != null)
                return Microphone.Start(selectedDevice, loop, length, recordFreq);
            Debug.LogError("<b>[SoundStone]</b> No Audio Source Found!");
            return null;
        }

        public bool IsRecording()
        {
            if (selectedDevice != null)
            {
                return Microphone.IsRecording(selectedDevice);
            }

            Debug.LogError("<b>[SoundStone]</b> No Audio Source Found!");
            return false;
        }

        public void EndRecording()
        {
            if (IsRecording())
                Microphone.End(selectedDevice);
        }

        public AudioClip MakeSubclip(AudioClip clip,  float stopTime, float offsetTime = 0)
        {
            if (stopTime == 0)
            {
                return null;
            }

            /* Create a new audio clip */
            int frequency = clip.frequency;
            float timeLength = stopTime - offsetTime;
            int samplesLength = (int) (frequency * timeLength);

            AudioClip newClip = AudioClip.Create(clip.name + "-sub", samplesLength, 1, frequency, false);

            /* Create a temporary buffer for the samples */
            float[] data = new float[samplesLength];

            /* Get the data from the original clip */
            clip.GetData(data, (int) offsetTime * frequency);

            /* Transfer the data to the new clip */
            newClip.SetData(data, 0);

            /* Return the sub clip */
            return newClip;
        }
    }
}
