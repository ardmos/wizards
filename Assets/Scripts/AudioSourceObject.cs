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

    // �������! ���������� ���� ����ȿ���� ����� ����Ұ�. �ڵ��ı� �޼ҵ带 ����� ����!
/*    private IEnumerator (float clipLength)
    {

    }*/
}
