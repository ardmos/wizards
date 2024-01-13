using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 스크롤 획득시 획득한 스크롤을 어느 스펠에 적용할지 
/// 선택하는 팝업을 관리하는 스크립트 입니다.
/// 
/// 1. 어떤 스킬 관련해서 이 팝업이 열리게된건지 스크롤 아이템 정보 저장
/// </summary>
public class PopupSelectSpellUI : MonoBehaviour
{
    public SpellSlot spellSlot0;
    public SpellSlot spellSlot1;
    public SpellSlot spellSlot2;

    List<SpellInfo> spellInfos;

    private void Start()
    {
        Hide();
    }

    public void InitPopup(Scroll scroll)
    {
        // 슬롯 UI 정보 초기화
        spellInfos = Player.LocalInstance.GetSpellController().GetSpellInfoArray();
        spellSlot0.InitUI(scroll, spellInfos[0].spellName);
        spellSlot1.InitUI(scroll, spellInfos[1].spellName);
        spellSlot2.InitUI(scroll, spellInfos[2].spellName);
    }

    public void Show(Scroll scroll)
    {
        gameObject.SetActive(true);
        InitPopup(scroll);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}