using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class AudioSourceObject : NetworkBehaviour
{
    public AudioSource audioSource;


    [ClientRpc]
    public void SetupClientRPC(SkillName spellName, SFX_Type sFX_Type, Vector3 position)
    {
        AudioClip audioClip = GameAssetsManager.Instance.GetSkillSFXSound(spellName, sFX_Type);

        if (audioClip == null) return;
        if (audioSource == null) return;

        transform.position = position;

        audioSource.volume = SoundManager.Instance.GetVolumeSFX();
        audioSource.clip = audioClip;
        audioSource.Play();

        //float distance = Vector3.Distance(transform.position, PlayerClient.Instance.transform.position);
        //Debug.Log($"Distance: {distance}");
    }

    [ClientRpc]
    public void SetupClientRPC(ItemName itemName, Vector3 position)
    {
        AudioClip audioClip = GameAssetsManager.Instance.GetItemSFXSound(itemName);

        if (audioClip == null) return;
        if (audioSource == null) return;

        transform.position = position;

        audioSource.volume = SoundManager.Instance.GetVolumeSFX();
        audioSource.clip = audioClip;
        audioSource.Play();

        float distance = Vector3.Distance(transform.position, PlayerClient.Instance.transform.position);
        Debug.Log($"Distance: {distance}");
    }
}
