using System;
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
    public TextMeshProUGUI txtSpellDescription;
    public Image imgSpellIcon;
    //public Image imgScrollEffectIcon;

    /// <summary>
    ///  scroll������ ���� UI�� ������ �ʱ�ȭ ���ִ� �޼ҵ� �Դϴ�.
    /// </summary>
    public void InitUI(ISkillUpgradeOption skillUpgradeOption, PopupSelectScrollEffectUIController popupSelectScrollEffectUIController)
    {
        //Debug.Log($"InitUI. scrollName:{scrollName}, spellIndexToApply:{spellIndexToApply}");
        skillUpgradeOption.ToDTO();
        // upgradeOption �̸�
        txtScrollName.text = $"{skillUpgradeOption.GetName()}!!";
        // upgradeOption ����  
        txtSpellDescription.text = skillUpgradeOption.GetDescription();
        // upgradeOption ������ �̹���
        imgSpellIcon.sprite = skillUpgradeOption.GetIcon();

        // ��ư ��� ����   
        btnApply.onClick.RemoveAllListeners();
        btnApply.AddClickListener(() => {
            // ���޹��� ��ũ�� �̸��� �����ε����� ����ؼ� ȿ�� ������ �����Ѵ�.
            //ScrollManagerServer.Instance.UpdateScrollEffectServerRPC(scrollName, spellIndex);
            ScrollManagerServer.Instance.UpdateScrollEffectServerRPC(skillUpgradeOption);

            // SFX ���
            SoundManager.Instance.PlayItemSFXServerRPC(ItemName.ScrollUse, transform.position);

            // VFX ���
            PlayerClient.Instance.GetComponent<PlayerServer>().StartApplyScrollVFXServerRPC();

            popupSelectScrollEffectUIController.Hide();
        });
    }
}
