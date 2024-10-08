using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 아이템들 생성
/// 현재 아이템 리스트
/// 1. Remove Ads(실제 구매 기능은 광고기능 추가 후 추가)
/// </summary>
public class DailyGroupUI : MonoBehaviour
{
    [SerializeField] private GameObject itemTemplatePrefab;

    // 아이템 생성
    public void GenerateItems()
    {
        // 아이템 목록을 바탕으로 생성(Shop 아이템만)
        for (int i = (int)ItemName.ShopItemStart+1; i < (int)ItemName.ShopItemEnd; i++)
        {
            GameObject item = Instantiate(itemTemplatePrefab);
            item.transform.SetParent(transform, true);
            item.transform.localScale = Vector3.one;
            item.GetComponent<ItemTemplateUI>()?.InitItem((ItemName)i);
        }       
    }

}
