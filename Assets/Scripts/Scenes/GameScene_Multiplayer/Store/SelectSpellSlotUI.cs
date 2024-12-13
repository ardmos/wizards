using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 상점기능을 봉인중이어서 사용하지 않고있는 스크립트. 
/// </summary>
public class SelectSpellSlotUI : MonoBehaviour
{
    private ItemName itemType;

    public PopupMagicStoreUI magicStore;

    // Start is called before the first frame update
    void Start()
    {
        // UI 비활성화
        gameObject.SetActive(false);
    }

    public void Show(ItemName itemType)
    {
        this.itemType = itemType;
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void OnBtnSlot1Clicked()
    {
        magicStore.TryBuyItem(itemType, 1);
    }
    public void OnBtnSlot2Clicked()
    {
        magicStore.TryBuyItem(itemType, 2);
    }
    public void OnBtnSlot3Clicked()
    {
        magicStore.TryBuyItem(itemType, 3);
    }
}
