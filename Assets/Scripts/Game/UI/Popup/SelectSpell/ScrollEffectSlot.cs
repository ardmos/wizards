using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 1. 버튼 클릭시 -> 선택된 기능을 지정된 스펠 슬롯에 적용
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
        // scroll 이름
        txtScrollName.text = $"{Item.GetName(scrollNames)}!!";
        // spell 이름
        SpellName spellName = Player.LocalInstance.GetSpellController().GetSpellInfoList()[spellIndexToApply].spellName;
        txtSpellName.text = spellName.ToString();
        // spell 아이콘 이미지
        imgSpellIcon.sprite = GameAssets.instantiate.GetSpellIconImage(spellName);

        btnApply.AddClickListener(() => {
            if (Player.LocalInstance == null) return;
            if(GetComponentInParent<PopupSelectScrollEffectUIController>() == null) return;

            Player.LocalInstance.RequestApplyScrollEffectToServer(scrollNames, spellIndexToApply);
            GetComponentInParent<PopupSelectScrollEffectUIController>().Hide();
        });
    }



    /// <summary>
    /// 슬롯의 slotNumber와 이 팝업의 scroll정보에 맞춰 슬롯 UI의 정보를 초기화 해주는 메소드 입니다.
    /// </summary>
/*    public void InitUI(Scroll scroll, SpellName spellName)
    {
        // scroll 이름
        txtScrollName.text = $"{Item.GetName(scroll.scrollName)}!!";
        // spell 이름
        txtSpellName.text = spellName.ToString();
        // spell 아이콘 이미지
        imgSpellIcon.sprite = GameAssets.instantiate.GetSpellIconImage(spellName);

        btnApply.AddClickListener(() => {
            Player.LocalInstance.ApplyScrollEffectToSpell(scroll, slotNumber);
            GetComponentInParent<PopupSelectSpellUIController>().Hide();
        });
    }*/
}
