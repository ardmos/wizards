using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 1. 버튼 클릭시 -> 선택된 기능을 지정된 스펠 슬롯에 적용
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
    ///  scroll정보에 맞춰 UI의 정보를 초기화 해주는 메소드 입니다.
    /// </summary>
    public void InitUI(ItemName scrollName, byte spellIndexToApply)
    {
        //Debug.Log($"InitUI. scrollName:{scrollName}, spellIndexToApply:{spellIndexToApply}");

        // scroll 이름
        txtScrollName.text = $"{Item.GetName(scrollName)}!!";
        // spell 이름
        SpellName spellName = Player.LocalInstance.GetSpellController().GetSpellInfoList()[spellIndexToApply].spellName;
        txtSpellName.text = spellName.ToString();
        // spell 아이콘 이미지
        imgSpellIcon.sprite = GameAssets.instantiate.GetSpellIconImage(spellName);
        // scroll effect 아이콘 이미지
        imgScrollEffectIcon.sprite = GameAssets.instantiate.GetScrollEffectIconImage(scrollName);

        // 버튼 기능 설정   
        btnApply.onClick.RemoveAllListeners();
        btnApply.AddClickListener(() => {
            if (Player.LocalInstance == null) return;
            if(GetComponentInParent<PopupSelectScrollEffectUIController>() == null) return;

            Player.LocalInstance.RequestApplyScrollEffectToServer(scrollName, spellIndexToApply);
            GetComponentInParent<PopupSelectScrollEffectUIController>().Hide();
        });
    }
}
