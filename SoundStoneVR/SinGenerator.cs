using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.Collections;
using UnityEngine;

namespace SoundStone.SoundSystem
{
    public class SinGenerator : MonoBehaviour
    {
        private static SinGenerator _instance;

        public static SinGenerator instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType<SinGenerator>();

                return _instance;
            }
        }

        public AudioClip GenerateSinusoidalClip(AudioClip.PCMReaderCallback pcmReaderCallback, int duration, int clipFrequency = 44100)
        {
            return AudioClip.Create("sinwave", duration * clipFrequency , 1, clipFrequency, true, pcmReaderCallback);
        }

        
    }
}