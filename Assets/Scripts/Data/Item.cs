using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 게임에서 사용되는 아이템의 상세 정보를 갖고있는 스크립트 
/// </summary>
public class Item
{
    public enum ItemName
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


    public static float GetCost(ItemName itemType)
    {
        switch (itemType)
        {
            case ItemName.None: return 0;
            case ItemName.RemoveAds: 
                return 3.99f;
            case ItemName.Armor_1:
            case ItemName.Armor_2:
            case ItemName.Armor_3:
            case ItemName.Armor_4:
            case ItemName.Armor_5:
            case ItemName.Armor_6:
            case ItemName.Armor_7:
            case ItemName.Armor_8:
            case ItemName.Armor_9:
            case ItemName.Armor_10:
            case ItemName.Armor_11:
            case ItemName.Armor_12:
            case ItemName.Armor_13:
            case ItemName.Armor_14:
            case ItemName.Armor_15:
            case ItemName.Armor_16:
            case ItemName.Armor_17:
            case ItemName.Armor_18:
            case ItemName.Armor_19:
            case ItemName.Armor_20:
                return 100f;
            case ItemName.Hat_1:
            case ItemName.Hat_2:
            case ItemName.Hat_3:
            case ItemName.Hat_4:
            case ItemName.Hat_5:
            case ItemName.Hat_6:
            case ItemName.Hat_7:
            case ItemName.Hat_8:
            case ItemName.Hat_9:
            case ItemName.Hat_10:
            case ItemName.Hat_11:
            case ItemName.Hat_12:
            case ItemName.Hat_13:
            case ItemName.Hat_14:
                return 50f;
            case ItemName.BackPack_1:
            case ItemName.BackPack_2:
            case ItemName.BackPack_3:
                return 80f;
            case ItemName.Wand_1:
            case ItemName.Wand_2:
            case ItemName.Wand_3:
            case ItemName.Wand_4:
            case ItemName.Wand_5:
            case ItemName.Wand_6:
            case ItemName.Wand_7:
                return 60f;
            case ItemName.FireBall_1:
            case ItemName.WaterBall_1:
            case ItemName.IceBall_1:
                return 30f;
            default:
                Debug.LogError("GetCost error"); 
                return 0; 
        }
    }

    public static Mesh GetMesh(ItemName itemType)
    {
        switch (itemType)
        {
            case ItemName.Armor_1: return GameAssets.instantiate.m_Body_1;
            case ItemName.Armor_2: return GameAssets.instantiate.m_Body_2;
            case ItemName.Armor_3: return GameAssets.instantiate.m_Body_3;
            case ItemName.Armor_4: return GameAssets.instantiate.m_Body_4;
            case ItemName.Armor_5: return GameAssets.instantiate.m_Body_5;
            case ItemName.Armor_6: return GameAssets.instantiate.m_Body_6;
            case ItemName.Armor_7: return GameAssets.instantiate.m_Body_7;
            case ItemName.Armor_8: return GameAssets.instantiate.m_Body_8;
            case ItemName.Armor_9: return GameAssets.instantiate.m_Body_9;
            case ItemName.Armor_10: return GameAssets.instantiate.m_Body_10;
            case ItemName.Armor_11: return GameAssets.instantiate.m_Body_11;
            case ItemName.Armor_12: return GameAssets.instantiate.m_Body_12;
            case ItemName.Armor_13: return GameAssets.instantiate.m_Body_13;
            case ItemName.Armor_14: return GameAssets.instantiate.m_Body_14;
            case ItemName.Armor_15: return GameAssets.instantiate.m_Body_15;
            case ItemName.Armor_16: return GameAssets.instantiate.m_Body_16;
            case ItemName.Armor_17: return GameAssets.instantiate.m_Body_17;
            case ItemName.Armor_18: return GameAssets.instantiate.m_Body_18;
            case ItemName.Armor_19: return GameAssets.instantiate.m_Body_19;
            case ItemName.Armor_20: return GameAssets.instantiate.m_Body_20;
            case ItemName.Hat_1: return GameAssets.instantiate.m_Hat_1;
            case ItemName.Hat_2: return GameAssets.instantiate.m_Hat_2;
            case ItemName.Hat_3: return GameAssets.instantiate.m_Hat_3;
            case ItemName.Hat_4: return GameAssets.instantiate.m_Hat_4;
            case ItemName.Hat_5: return GameAssets.instantiate.m_Hat_5;
            case ItemName.Hat_6: return GameAssets.instantiate.m_Hat_6;
            case ItemName.Hat_7: return GameAssets.instantiate.m_Hat_7;
            case ItemName.Hat_8: return GameAssets.instantiate.m_Hat_8;
            case ItemName.Hat_9: return GameAssets.instantiate.m_Hat_9;
            case ItemName.Hat_10: return GameAssets.instantiate.m_Hat_10;
            case ItemName.Hat_11: return GameAssets.instantiate.m_Hat_11;
            case ItemName.Hat_12: return GameAssets.instantiate.m_Hat_12;
            case ItemName.Hat_13: return GameAssets.instantiate.m_Hat_13;
            case ItemName.Hat_14: return GameAssets.instantiate.m_Hat_14;
            case ItemName.BackPack_1: return GameAssets.instantiate.m_BackPack_1;
            case ItemName.BackPack_2: return GameAssets.instantiate.m_BackPack_2;
            case ItemName.BackPack_3: return GameAssets.instantiate.m_BackPack_3;
            case ItemName.Wand_1: return GameAssets.instantiate.m_Wand_1;
            case ItemName.Wand_2: return GameAssets.instantiate.m_Wand_2;
            case ItemName.Wand_3: return GameAssets.instantiate.m_Wand_3;
            case ItemName.Wand_4: return GameAssets.instantiate.m_Wand_4;
            case ItemName.Wand_5: return GameAssets.instantiate.m_Wand_5;
            case ItemName.Wand_6: return GameAssets.instantiate.m_Wand_6;
            case ItemName.Wand_7: return GameAssets.instantiate.m_Wand_7;
            case ItemName.FireBall_1: return GameAssets.instantiate.m_Scroll_1;
            case ItemName.WaterBall_1: return GameAssets.instantiate.m_Scroll_1;
            case ItemName.IceBall_1: return GameAssets.instantiate.m_Scroll_1;
            default: { Debug.LogError("GetMesh error"); return null; }
        }
    }

    public static Sprite GetIcon(ItemName itemType)
    {
        switch (itemType)
        {
            case ItemName.RemoveAds: return GameAssets.instantiate.s_RemoveAds;
            case ItemName.Item_Wizard: return GameAssets.instantiate.IconWizardClass;
            case ItemName.Item_Knight: return GameAssets.instantiate.IconKnightClass;
            case ItemName.Item_Gold: return GameAssets.instantiate.IconGold;
            case ItemName.Item_BonusGold: return GameAssets.instantiate.IconBonusGold;
            case ItemName.Item_Exp: return GameAssets.instantiate.IconExp;
            default:
                Debug.Log("No Image");
                return null;               
        }
    }

    public static string GetName(ItemName itemType)
    {
        switch (itemType)
        {
            case ItemName.RemoveAds:
                return "Remove Ads";
            default:
                return "";
        }
    } 
}
