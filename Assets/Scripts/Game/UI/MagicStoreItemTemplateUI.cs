using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
/// <summary>
/// ��������� UI�� ������ ���ø� 
/// !!!���� ���
///     1. ������ �̸�, ���� ǥ��
///     2. Ŭ���� ���� �õ� �ҽ��� MagicStore���� �˸�
/// </summary>
public class MagicStoreItemTemplateUI : MonoBehaviour
{
    public ItemName itemType;
    public SelectSpellSlotUI selectSpellSlotPopup;
    public TextMeshProUGUI txtName, txtPrice;

    public void InitItemInfo(string itemName, string itemPrice)
    {
        txtName.text = itemName;
        txtPrice.text = itemPrice;
    }

    public void BtnItemOnClick()
    {
        selectSpellSlotPopup.Show(itemType);
    }
}
