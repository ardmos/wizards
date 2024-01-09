using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 게임에서 사용되는 아이템의 상세 정보를 갖고있는 스크립트 
/// </summary>
public class Item
{
    public enum ItemType
    {
        None,
        ShopItemStart,
        RemoveAds,
        ShopItemEnd,
        ArmorStart,
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
        ArmorEnd,
        HatStart,
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
        HatEnd,
        BackPackStart,
        BackPack_1,
        BackPack_2,
        BackPack_3,
        BackPackEnd,
        WandStart,
        Wand_1,
        Wand_2,
        Wand_3,
        Wand_4,
        Wand_5,
        Wand_6,
        Wand_7,
        WandEnd,
        SpellStart,
        FireBall_1,
        WaterBall_1,
        IceBall_1,
        SpellEnd,
        ItemStart,
        Item_Wizard,
        Item_Knight,
        Item_Gold,
        Item_BonusGold,
        Item_Exp,
        ItemEnd,
        Max
    }


    public static float GetCost(ItemType itemType)
    {
        switch (itemType)
        {
            case ItemType.None: return 0;
            case ItemType.RemoveAds: 
                return 3.99f;
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
                return 100f;
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
                return 50f;
            case ItemType.BackPack_1:
            case ItemType.BackPack_2:
            case ItemType.BackPack_3:
                return 80f;
            case ItemType.Wand_1:
            case ItemType.Wand_2:
            case ItemType.Wand_3:
            case ItemType.Wand_4:
            case ItemType.Wand_5:
            case ItemType.Wand_6:
            case ItemType.Wand_7:
                return 60f;
            case ItemType.FireBall_1:
            case ItemType.WaterBall_1:
            case ItemType.IceBall_1:
                return 30f;
            default:
                Debug.LogError("GetCost error"); 
                return 0; 
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
            case ItemType.FireBall_1: return GameAssets.instantiate.m_Scroll_1;
            case ItemType.WaterBall_1: return GameAssets.instantiate.m_Scroll_1;
            case ItemType.IceBall_1: return GameAssets.instantiate.m_Scroll_1;
            default: { Debug.LogError("GetMesh error"); return null; }
        }
    }

    public static Sprite GetIcon(ItemType itemType)
    {
        switch (itemType)
        {
            case ItemType.RemoveAds: return GameAssets.instantiate.s_RemoveAds;
            case ItemType.Item_Wizard: return GameAssets.instantiate.IconWizardClass;
            case ItemType.Item_Knight: return GameAssets.instantiate.IconKnightClass;
            case ItemType.Item_Gold: return GameAssets.instantiate.IconGold;
            case ItemType.Item_BonusGold: return GameAssets.instantiate.IconBonusGold;
            case ItemType.Item_Exp: return GameAssets.instantiate.IconExp;
            default:
                Debug.Log("No Image");
                return null;               
        }
    }

    public static string GetName(ItemType itemType)
    {
        switch (itemType)
        {
            case ItemType.RemoveAds:
                return "Remove Ads";
            default:
                return "";
        }
    } 
}
