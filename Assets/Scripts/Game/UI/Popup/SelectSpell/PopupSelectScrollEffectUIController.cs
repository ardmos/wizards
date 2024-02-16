using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// ��ũ�� ȹ��� ȹ���� ��ũ���� ��� ���翡 �������� 
/// �����ϴ� �˾��� �����ϴ� ��ũ��Ʈ �Դϴ�.
/// 
/// 1. � ��ų �����ؼ� �� �˾��� �����ԵȰ��� ��ũ�� ������ ���� ����
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

    public void InitPopup(ItemName[] scrollNames)
    {
        // ���� UI ���� �ʱ�ȭ. Scroll ȿ���� ����� Spell Slot�� �����κ��� �������� ť�� ����� ù��° ��.
        spellIndexToApply = SpellManager.Instance.PeekPlayerScrollSpellSlotQueueOnClient();
        //Debug.Log($"InitPopup. spellIndexToApply:{spellIndexToApply}, scrollNames[0]:{scrollNames[0]}, [1]:{scrollNames[1]}, [2]:{scrollNames[2]}");

        scrollEffectSlot0.InitUI(scrollNames[0], spellIndexToApply);
        scrollEffectSlot1.InitUI(scrollNames[1], spellIndexToApply);
        scrollEffectSlot2.InitUI(scrollNames[2], spellIndexToApply);
    }

    public void Show()
    {
        gameObject.SetActive(true);

        // ���� UI ���� �ʱ�ȭ. �÷��̾�� ������ ���� ��� �� ������ �������� ����.
        Player.LocalInstance.RequestUniqueRandomScrollsToServer();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}