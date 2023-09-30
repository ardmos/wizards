using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
/// <summary>
/// ���ӿ��� ���Ǵ� �������� �� ������ �����ִ� ��ũ��Ʈ 
/// </summary>
public class Item
{
    public enum ItemType
    {
        Armor_1,
        Hat_1,
        BackPack_1,
        Wand_1,
        Scroll_1
    }

    public static int GetCost(ItemType itemType)
    {
        switch (itemType)   
        {
            case ItemType.Armor_1:  return 100;
            case ItemType.Hat_1:    return 50;
            case ItemType.BackPack_1: return 80;
            case ItemType.Wand_1:   return 60;
            case ItemType.Scroll_1:    return 30;
            default: { Debug.LogError("GetCost error"); return 0; }
        }
    }

    public static Mesh GetMesh(ItemType itemType)
    {
        switch (itemType)
        {
            case ItemType.Armor_1:  return GameAssets.instantiate.m_Body_1;
            case ItemType.Hat_1:    return GameAssets.instantiate.m_Hat_1;
            case ItemType.BackPack_1: return GameAssets.instantiate.m_BackPack_1;
            case ItemType.Wand_1:   return GameAssets.instantiate.m_Wand_1;
            case ItemType.Scroll_1: return GameAssets.instantiate.m_Scroll_1;
            default: { Debug.LogError("GetMesh error"); return null; }
        }
    }
}
