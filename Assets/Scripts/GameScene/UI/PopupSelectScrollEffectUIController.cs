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

    public ScrollEffectSlotUI scrollEffectSlot0;
    public ScrollEffectSlotUI scrollEffectSlot1;
    public ScrollEffectSlotUI scrollEffectSlot2;

    public byte spellIndexToApply;

    private void Start()
    {
        Hide();
    }

    public void InitPopup(ItemName[] scrollNames)
    {
        // 슬롯 UI 정보 초기화. Scroll 효과가 적용될 Spell Slot은 서버로부터 공유받은 큐에 저장된 첫번째 값.
        spellIndexToApply = SpellManager.Instance.PeekPlayerScrollSpellSlotQueueOnClient();
        //Debug.Log($"InitPopup. spellIndexToApply:{spellIndexToApply}, scrollNames[0]:{scrollNames[0]}, [1]:{scrollNames[1]}, [2]:{scrollNames[2]}");

        scrollEffectSlot0.InitUI(scrollNames[0], spellIndexToApply);
        scrollEffectSlot1.InitUI(scrollNames[1], spellIndexToApply);
        scrollEffectSlot2.InitUI(scrollNames[2], spellIndexToApply);
    }

    public void Show()
    {
        gameObject.SetActive(true);

        // 효과음 재생
        SoundManager.Instance.PlayOpenScrollSound();

        // 슬롯 UI 정보 초기화. 플레이어에게 랜덤의 스펠 기능 세 가지를 선택지로 제시.
        Player.LocalInstance.RequestUniqueRandomScrollsToServer();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}