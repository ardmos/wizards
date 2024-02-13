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

    /*    public void InitPopup(Scroll scroll)
        {
            // ���� UI ���� �ʱ�ȭ
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
        // ���� UI ���� �ʱ�ȭ. ȿ���� ����� Spell Slot�� �����κ��� �������� ť�� ����� ù��° ��.
        spellIndexToApply = SpellManager.Instance.PeekPlayerScrollSpellSlotQueueOnClient();
        // ���� UI ���� �ʱ�ȭ. �÷��̾�� ������ ���� ��� �� ������ �������� ����.

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