using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 아이템들 생성
/// 현재 아이템 리스트
/// 1. Remove Ads
/// </summary>
public class DailyGroupUI : MonoBehaviour
{
    [SerializeField] private GameObject itemTemplatePrefab;

    // Start is called before the first frame update
    void Start()
    {
        // 아이템 생성   
        GenerateItems();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // 아이템 생성
    private void GenerateItems()
    {
        // 아이템 목록을 바탕으로 생성(Shop 아이템만)
        for (int i = (int)Item.ItemType.ShopItemStart+1; i < (int)Item.ItemType.ShopItemEnd; i++)
        {
            GameObject item = Instantiate(itemTemplatePrefab);
            item.transform.SetParent(transform, true);
            item.GetComponent<ItemTemplateUI>()?.InitItem((Item.ItemType)i);
        }       
    }

}
