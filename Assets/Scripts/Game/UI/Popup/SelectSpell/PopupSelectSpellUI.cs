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
        // ���� UI ���� �ʱ�ȭ
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