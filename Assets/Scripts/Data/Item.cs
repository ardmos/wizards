using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// ���ӿ��� ���Ǵ� �������� �� ������ �����ִ� ��ũ��Ʈ 
/// </summary>
public class Item
{
    public enum ItemName
    {
        None,
        ShopItemStart,
        RemoveAds,
        ShopItemEnd,
        ItemStart,
        Item_Wizard,
        Item_Knight,
        Item_Gold,
        Item_BonusGold,
        Item_Exp,
        ItemEnd,
        ScrollStart,
        Scroll_LevelUp,
        Scroll_FireRateUp,
        Scroll_FlySpeedUp,
        Scroll_Attach,
        Scroll_Guide,
        ScrollEnd,
        Max
    }


    public static float GetCost(ItemName itemType)
    {
        switch (itemType)
        {
            case ItemName.None: return 0;
            case ItemName.RemoveAds: 
                return 3.99f;            
            default:
                Debug.LogError("GetCost error"); 
                return 0; 
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
            case ItemName.Scroll_LevelUp:
                return "LevelUp";
            case ItemName.Scroll_FireRateUp:
                return "FireRateUp";
            case ItemName.Scroll_FlySpeedUp:
                return "FlySpeedUp";
            case ItemName.Scroll_Attach:
                return "Attach";
            case ItemName.Scroll_Guide:
                return "Guide";
            default:
                return "";
        }
    } 

    // ������ũ�� ȿ�� ���

}
