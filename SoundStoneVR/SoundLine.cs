using System.Collections;
using System.Collections.Generic;
using SoundStone.SoundSystem;
using UnityEngine;
using Valve.VR.InteractionSystem;

[RequireComponent(typeof(Interactable))]
public class SoundLine : MonoBehaviour
{
    protected AudioSource[] audioSourceList;
    protected Interactable interactable;

    void Awake()
    {
        audioSourceList = GetComponentsInChildren<AudioSource>();
        interactable = GetComponent<Interactable>();
    }

    protected void PlaySequential()
    {
        foreach (AudioSource audioSource in audioSourceList)
        {
            audioSource.Play();
            StartCoroutine("WaitForClipToFinish", audioSource.clip.length);
        }
    }

    protected IEnumerator WaitForClipToFinish(float clipLength)
    {
        yield return new WaitForSeconds(clipLength);
    }
}
