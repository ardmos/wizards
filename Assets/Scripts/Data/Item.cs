using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
/// <summary>
/// 게임에서 사용되는 아이템의 상세 정보를 갖고있는 스크립트 
/// </summary>
public class Item
{
    public enum ItemType
    {
        Armor_1,
        Armor_2,
        Armor_3,
        Armor_4,
        Armor_5,
        Armor_6,
        Armor_7,
        Armor_8,
        Armor_9,
        Armor_10,
        Armor_11,
        Armor_12,
        Armor_13,
        Armor_14,
        Armor_15,
        Armor_16,
        Armor_17,
        Armor_18,
        Armor_19,
        Armor_20,
        Hat_1,
        Hat_2,
        Hat_3,
        Hat_4,
        Hat_5,
        Hat_6,
        Hat_7,
        Hat_8,
        Hat_9,
        Hat_10,
        Hat_11,
        Hat_12,
        Hat_13,
        Hat_14,
        BackPack_1,
        BackPack_2,
        BackPack_3,
        Wand_1,
        Wand_2,
        Wand_3,
        Wand_4,
        Wand_5,
        Wand_6,
        Wand_7,
        Scroll_1,
        Scroll_2,
        Scroll_3,
        Max
    }

    public static int GetCost(ItemType itemType)
    {
        switch (itemType)
        {
            case ItemType.Armor_1:
            case ItemType.Armor_2:
            case ItemType.Armor_3:
            case ItemType.Armor_4:
            case ItemType.Armor_5:
            case ItemType.Armor_6:
            case ItemType.Armor_7:
            case ItemType.Armor_8:
            case ItemType.Armor_9:
            case ItemType.Armor_10:
            case ItemType.Armor_11:
            case ItemType.Armor_12:
            case ItemType.Armor_13:
            case ItemType.Armor_14:
            case ItemType.Armor_15:
            case ItemType.Armor_16:
            case ItemType.Armor_17:
            case ItemType.Armor_18:
            case ItemType.Armor_19:
            case ItemType.Armor_20:
                return 100;
            case ItemType.Hat_1:
            case ItemType.Hat_2:
            case ItemType.Hat_3:
            case ItemType.Hat_4:
            case ItemType.Hat_5:
            case ItemType.Hat_6:
            case ItemType.Hat_7:
            case ItemType.Hat_8:
            case ItemType.Hat_9:
            case ItemType.Hat_10:
            case ItemType.Hat_11:
            case ItemType.Hat_12:
            case ItemType.Hat_13:
            case ItemType.Hat_14:
                return 50;
            case ItemType.BackPack_1:
            case ItemType.BackPack_2:
            case ItemType.BackPack_3:
                return 80;
            case ItemType.Wand_1:
            case ItemType.Wand_2:
            case ItemType.Wand_3:
            case ItemType.Wand_4:
            case ItemType.Wand_5:
            case ItemType.Wand_6:
            case ItemType.Wand_7:
                return 60;
            case ItemType.Scroll_1:
            case ItemType.Scroll_2:
            case ItemType.Scroll_3:
                return 30;
            default:
                { Debug.LogError("GetCost error"); return 0; }
        }
    }

    public static Mesh GetMesh(ItemType itemType)
    {
        switch (itemType)
        {
            case ItemType.Armor_1: return GameAssets.instantiate.m_Body_1;
            case ItemType.Armor_2: return GameAssets.instantiate.m_Body_2;
            case ItemType.Armor_3: return GameAssets.instantiate.m_Body_3;
            case ItemType.Armor_4: return GameAssets.instantiate.m_Body_4;
            case ItemType.Armor_5: return GameAssets.instantiate.m_Body_5;
            case ItemType.Armor_6: return GameAssets.instantiate.m_Body_6;
            case ItemType.Armor_7: return GameAssets.instantiate.m_Body_7;
            case ItemType.Armor_8: return GameAssets.instantiate.m_Body_8;
            case ItemType.Armor_9: return GameAssets.instantiate.m_Body_9;
            case ItemType.Armor_10: return GameAssets.instantiate.m_Body_10;
            case ItemType.Armor_11: return GameAssets.instantiate.m_Body_11;
            case ItemType.Armor_12: return GameAssets.instantiate.m_Body_12;
            case ItemType.Armor_13: return GameAssets.instantiate.m_Body_13;
            case ItemType.Armor_14: return GameAssets.instantiate.m_Body_14;
            case ItemType.Armor_15: return GameAssets.instantiate.m_Body_15;
            case ItemType.Armor_16: return GameAssets.instantiate.m_Body_16;
            case ItemType.Armor_17: return GameAssets.instantiate.m_Body_17;
            case ItemType.Armor_18: return GameAssets.instantiate.m_Body_18;
            case ItemType.Armor_19: return GameAssets.instantiate.m_Body_19;
            case ItemType.Armor_20: return GameAssets.instantiate.m_Body_20;
            case ItemType.Hat_1: return GameAssets.instantiate.m_Hat_1;
            case ItemType.Hat_2: return GameAssets.instantiate.m_Hat_2;
            case ItemType.Hat_3: return GameAssets.instantiate.m_Hat_3;
            case ItemType.Hat_4: return GameAssets.instantiate.m_Hat_4;
            case ItemType.Hat_5: return GameAssets.instantiate.m_Hat_5;
            case ItemType.Hat_6: return GameAssets.instantiate.m_Hat_6;
            case ItemType.Hat_7: return GameAssets.instantiate.m_Hat_7;
            case ItemType.Hat_8: return GameAssets.instantiate.m_Hat_8;
            case ItemType.Hat_9: return GameAssets.instantiate.m_Hat_9;
            case ItemType.Hat_10: return GameAssets.instantiate.m_Hat_10;
            case ItemType.Hat_11: return GameAssets.instantiate.m_Hat_11;
            case ItemType.Hat_12: return GameAssets.instantiate.m_Hat_12;
            case ItemType.Hat_13: return GameAssets.instantiate.m_Hat_13;
            case ItemType.Hat_14: return GameAssets.instantiate.m_Hat_14;
            case ItemType.BackPack_1: return GameAssets.instantiate.m_BackPack_1;
            case ItemType.BackPack_2: return GameAssets.instantiate.m_BackPack_2;
            case ItemType.BackPack_3: return GameAssets.instantiate.m_BackPack_3;
            case ItemType.Wand_1: return GameAssets.instantiate.m_Wand_1;
            case ItemType.Wand_2: return GameAssets.instantiate.m_Wand_2;
            case ItemType.Wand_3: return GameAssets.instantiate.m_Wand_3;
            case ItemType.Wand_4: return GameAssets.instantiate.m_Wand_4;
            case ItemType.Wand_5: return GameAssets.instantiate.m_Wand_5;
            case ItemType.Wand_6: return GameAssets.instantiate.m_Wand_6;
            case ItemType.Wand_7: return GameAssets.instantiate.m_Wand_7;
            case ItemType.Scroll_1: return GameAssets.instantiate.m_Scroll_1;
            default: { Debug.LogError("GetMesh error"); return null; }
        }
    }
}
