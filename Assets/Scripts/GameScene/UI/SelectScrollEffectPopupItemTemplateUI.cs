using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 1. ��ư Ŭ���� -> ���õ� ����� ������ ���� ���Կ� ����
/// </summary>
public class SelectScrollEffectPopupItemTemplateUI : MonoBehaviour
{
    //public sbyte slotNumber;
    public CustomButton btnApply;

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
        SpellName spellName = Player.LocalInstance.GetSpellController().GetSpellInfoList()[spellIndexToApply].spellName;
        txtSpellName.text = spellName.ToString();
        // spell ������ �̹���
        imgSpellIcon.sprite = GameAssets.instantiate.GetSpellIconImage(spellName);
        // scroll effect ������ �̹���
        imgScrollEffectIcon.sprite = GameAssets.instantiate.GetScrollEffectIconImage(scrollName);

        // ��ư ��� ����   
        btnApply.onClick.RemoveAllListeners();
        btnApply.AddClickListener(() => {
            if (Player.LocalInstance == null) return;
            if(GetComponentInParent<PopupSelectScrollEffectUIController>() == null) return;

            Player.LocalInstance.RequestApplyScrollEffectToServer(scrollName, spellIndexToApply);
            GetComponentInParent<PopupSelectScrollEffectUIController>().Hide();
        });
    }
}
