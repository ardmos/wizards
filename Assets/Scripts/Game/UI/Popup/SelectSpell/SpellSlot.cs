using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 1. 버튼 클릭시 -> 선택된 슬롯의 스킬에 스크롤 아이템 정보 전달.
/// </summary>
public class SpellSlot : MonoBehaviour
{
    public sbyte slotNumber;
    public Button btnApply;

    public TextMeshProUGUI txtScrollName;
    public TextMeshProUGUI txtSpellName;
    public Image imgSpellIcon;

    /// <summary>
    /// 슬롯의 slotNumber와 이 팝업의 scroll정보에 맞춰 슬롯 UI의 정보를 초기화 해주는 메소드 입니다.
    /// </summary>
    public void InitUI(Scroll scroll, SpellName spellName)
    {
        // scroll 이름
        txtScrollName.text = $"{Item.GetName(scroll.scrollName)}!!";
        // spell 이름
        txtSpellName.text = spellName.ToString();
        // spell 아이콘 이미지
        imgSpellIcon.sprite = GameAssets.instantiate.GetSpellIconImage(spellName);

        btnApply.onClick.AddListener(() => {
            Player.LocalInstance.ApplyScrollEffectToSpell(scroll, slotNumber);
            GetComponentInParent<PopupSelectSpellUI>().Hide();
        });
    }
}
