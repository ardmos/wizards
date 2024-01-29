using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// �����۵� ����
/// ���� ������ ����Ʈ
/// 1. Remove Ads(���� ���� ����� ������ �߰� �� �߰�)
/// </summary>
public class DailyGroupUI : MonoBehaviour
{
    [SerializeField] private GameObject itemTemplatePrefab;

    // ������ ����
    public void GenerateItems()
    {
        // ������ ����� �������� ����(Shop �����۸�)
        for (int i = (int)Item.ItemName.ShopItemStart+1; i < (int)Item.ItemName.ShopItemEnd; i++)
        {
            GameObject item = Instantiate(itemTemplatePrefab);
            item.transform.SetParent(transform, true);
            item.transform.localScale = Vector3.one;
            item.GetComponent<ItemTemplateUI>()?.InitItem((Item.ItemName)i);
        }       
    }

}
