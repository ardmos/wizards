using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 1. ��ư Ŭ���� -> ���õ� ������ ��ų�� ��ũ�� ������ ���� ����.
/// </summary>
public class SpellSlot : MonoBehaviour
{
    public sbyte slotNumber;
    public Button btnApply;

    public TextMeshProUGUI txtScrollName;
    public TextMeshProUGUI txtSpellName;
    public Image imgSpellIcon;

    /// <summary>
    /// ������ slotNumber�� �� �˾��� scroll������ ���� ���� UI�� ������ �ʱ�ȭ ���ִ� �޼ҵ� �Դϴ�.
    /// </summary>
    public void InitUI(Scroll scroll, SpellName spellName)
    {
        // scroll �̸�
        txtScrollName.text = $"{Item.GetName(scroll.scrollName)}!!";
        // spell �̸�
        txtSpellName.text = spellName.ToString();
        // spell ������ �̹���
        imgSpellIcon.sprite = GameAssets.instantiate.GetSpellIconImage(spellName);

        btnApply.onClick.AddListener(() => {
            Player.LocalInstance.ApplyScrollEffectToSpell(scroll, slotNumber);
            GetComponentInParent<PopupSelectSpellUI>().Hide();
        });
    }
}
