using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSourceObject : MonoBehaviour
{
    public void Setup(AudioClip audioClip)
    {
        AudioSource audioSource = GetComponent<AudioSource>();
        audioSource.volume = SoundManager.instance.GetVolumeSFX();
        audioSource.clip = audioClip;
        audioSource.Play();

        
    }

    // 여기부터! 프리팹으로 만들어서 게임효과음 재생시 사용할것. 자동파괴 메소드를 만들고 시작!
/*    private IEnumerator (float clipLength)
    {

    }*/
}
