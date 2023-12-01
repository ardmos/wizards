using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Shop�� �����ų �����۵��� ����
/// </summary>

public class ItemTemplateUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI txtName;
    [SerializeField] private Image imgIcon;
    [SerializeField] private TextMeshProUGUI txtPrice;
    public void InitItem(Item.ItemType itemType)
    {
        txtName.text = Item.GetName(itemType);
        imgIcon.sprite = Item.GetSprite(itemType);
        txtPrice.text = $"USD $<size=120%><#21bf82>{Item.GetCost(itemType)}";
    }

    // ���Ž� ���μ��� ������ ����.
    // 1. ����? ���� �������� UGS Auth ����
    // 2. �ش� ���� ���� ó��(ex. Remove Ads)
}
