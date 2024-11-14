using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
/// <summary>
/// 매직스토어 UI의 아이템 템플릿 
/// !!!현재 기능
///     1. 아이템 이름, 가격 표시
///     2. 클릭시 구매 시도 소식을 MagicStore에게 알림
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
