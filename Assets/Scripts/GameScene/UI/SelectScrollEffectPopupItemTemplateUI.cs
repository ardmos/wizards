using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 1. ��ư Ŭ���� -> ���õ� ����� ������ ���� ���Կ� ����
/// </summary>
public class SelectScrollEffectPopupItemTemplateUI : MonoBehaviour
{
    //public sbyte slotNumber;
    public CustomClickSoundButton btnApply;

    public TextMeshProUGUI txtScrollName;
    public TextMeshProUGUI txtSpellName;
    public Image imgSpellIcon;
    public Image imgScrollEffectIcon;

    /// <summary>
    ///  scroll������ ���� UI�� ������ �ʱ�ȭ ���ִ� �޼ҵ� �Դϴ�.
    /// </summary>
    public void InitUI(ItemName scrollName, byte spellIndexToApply)
    {
        //Debug.Log($"InitUI. scrollName:{scrollName}, spellIndexToApply:{spellIndexToApply}");

        // scroll �̸�
        txtScrollName.text = $"{Item.GetName(scrollName)}!!";
        // spell �̸�
        SkillName spellName = PlayerClient.Instance.GetComponent<SkillSpellManagerClient>().GetSpellInfoList()[spellIndexToApply].spellName;
        txtSpellName.text = spellName.ToString();
        // spell ������ �̹���
        imgSpellIcon.sprite = GameAssets.instantiate.GetSpellIconImage(spellName);
        // scroll effect ������ �̹���
        imgScrollEffectIcon.sprite = GameAssets.instantiate.GetScrollEffectIconImage(scrollName);

        // ��ư ��� ����   
        btnApply.onClick.RemoveAllListeners();
        btnApply.AddClickListener(() => {
            if (PlayerClient.Instance == null) return;
            if(GetComponentInParent<PopupSelectScrollEffectUIController>() == null) return;

            RequestApplyScrollEffectToServer(scrollName, spellIndexToApply);
            GetComponentInParent<PopupSelectScrollEffectUIController>().Hide();
        });
    }

    // ���� ���ý� ����. Ŭ���̾�Ʈ���� ���ư��� �޼ҵ� �Դϴ�.
    public void RequestApplyScrollEffectToServer(ItemName scrollName, byte spellIndex)
    {
        //Debug.Log($"RequestApplyScrollEffectToServer. scrollNames:{scrollName}, spellIndexToApply:{spellIndex}");

        // ���޹��� ��ũ�� �̸��� �����ε����� ����ؼ� ȿ�� ������ �����Ѵ�.
        ScrollManagerServer.Instance.UpdateScrollEffectServerRPC(scrollName, spellIndex);

        // SFX ���
        SoundManager.Instance.PlayItemSFX(scrollName);
        // VFX ���
        PlayerClient.Instance.GetComponent<PlayerServer>().StartApplyScrollVFXServerRPC();
    }
}
