using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Shop에 노출시킬 아이템들의 주형
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

    // 구매시 프로세스 구현할 차례.
    // 1. 고객은? 현재 접속중인 UGS Auth 계정
    // 2. 해당 계정 구매 처리(ex. Remove Ads)
}
