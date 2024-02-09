using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSourceObject : MonoBehaviour
{
    public AudioSource audioSource;

    public void Setup(AudioClip audioClip)
    {
        if (audioClip == null) return;
        if (audioSource == null) return;

        audioSource.volume = SoundManager.instance.GetVolumeSFX();
        audioSource.clip = audioClip;
        audioSource.Play();

        StartCoroutine(StartDestroyCountdown(audioClip.length));
    }

    private IEnumerator StartDestroyCountdown(float clipLength)
    {
        yield return new WaitForSeconds(clipLength);
        Destroy(gameObject);
    }
}
