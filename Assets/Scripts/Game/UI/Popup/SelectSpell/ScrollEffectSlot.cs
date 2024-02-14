using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 1. ��ư Ŭ���� -> ���õ� ����� ������ ���� ���Կ� ����
/// 
/// 
/// </summary>
public class ScrollEffectSlot : MonoBehaviour
{
    //public sbyte slotNumber;
    public CustomButton btnApply;

    public TextMeshProUGUI txtScrollName;
    public TextMeshProUGUI txtSpellName;
    public Image imgSpellIcon;


    public void InitUI(ItemName scrollNames, byte spellIndexToApply)
    {
        // scroll �̸�
        txtScrollName.text = $"{Item.GetName(scrollNames)}!!";
        // spell �̸�
        SpellName spellName = Player.LocalInstance.GetSpellController().GetSpellInfoList()[spellIndexToApply].spellName;
        txtSpellName.text = spellName.ToString();
        // spell ������ �̹���
        imgSpellIcon.sprite = GameAssets.instantiate.GetSpellIconImage(spellName);

        btnApply.AddClickListener(() => {
            if (Player.LocalInstance == null) return;
            if(GetComponentInParent<PopupSelectScrollEffectUIController>() == null) return;

            Player.LocalInstance.RequestApplyScrollEffectToServer(scrollNames, spellIndexToApply);
            GetComponentInParent<PopupSelectScrollEffectUIController>().Hide();
        });
    }



    /// <summary>
    /// ������ slotNumber�� �� �˾��� scroll������ ���� ���� UI�� ������ �ʱ�ȭ ���ִ� �޼ҵ� �Դϴ�.
    /// </summary>
/*    public void InitUI(Scroll scroll, SpellName spellName)
    {
        // scroll �̸�
        txtScrollName.text = $"{Item.GetName(scroll.scrollName)}!!";
        // spell �̸�
        txtSpellName.text = spellName.ToString();
        // spell ������ �̹���
        imgSpellIcon.sprite = GameAssets.instantiate.GetSpellIconImage(spellName);

        btnApply.AddClickListener(() => {
            Player.LocalInstance.ApplyScrollEffectToSpell(scroll, slotNumber);
            GetComponentInParent<PopupSelectSpellUIController>().Hide();
        });
    }*/
}
