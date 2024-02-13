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
public class PopupSelectScrollEffectUIController : MonoBehaviour
{
    /*    public SpellSlot spellSlot0;
        public SpellSlot spellSlot1;
        public SpellSlot spellSlot2;

        List<SpellInfo> spellInfos;*/

    public ScrollEffectSlot scrollEffectSlot0;
    public ScrollEffectSlot scrollEffectSlot1;
    public ScrollEffectSlot scrollEffectSlot2;

    public byte spellIndexToApply;

    private void Start()
    {
        Hide();
    }

    /*    public void InitPopup(Scroll scroll)
        {
            // 슬롯 UI 정보 초기화
            spellInfos = Player.LocalInstance.GetSpellController().GetSpellInfoArray();
            spellSlot0.InitUI(scroll, spellInfos[0].spellName);
            spellSlot1.InitUI(scroll, spellInfos[1].spellName);
            spellSlot2.InitUI(scroll, spellInfos[2].spellName);
        }*/

    /*    public void Show(Scroll scroll)
        {
            gameObject.SetActive(true);
            InitPopup(scroll);
        }*/

    public void InitPopup(ItemName[] scrollNames)
    {
        // 슬롯 UI 정보 초기화. 효과가 적용될 Spell Slot은 서버로부터 공유받은 큐에 저장된 첫번째 값.
        spellIndexToApply = SpellManager.Instance.PeekPlayerScrollSpellSlotQueueOnClient();
        // 슬롯 UI 정보 초기화. 플레이어에게 랜덤의 스펠 기능 세 가지를 선택지로 제시.

        scrollEffectSlot0.InitUI(scrollNames[0], spellIndexToApply);
        scrollEffectSlot1.InitUI(scrollNames[1], spellIndexToApply);
        scrollEffectSlot2.InitUI(scrollNames[2], spellIndexToApply);
    }

    public void Show()
    {
        gameObject.SetActive(true);
        Player.LocalInstance.RequestUniqueRandomScrollsToServer();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}