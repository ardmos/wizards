using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// ���ӿ��� ���Ǵ� �������� �� ������ �����ִ� ��ũ��Ʈ 
/// </summary>
public class Item
{
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
            case ItemName.Item_Wizard: return GameAssets.instantiate.iconWizardClass;
            case ItemName.Item_Knight: return GameAssets.instantiate.iconKnightClass;
            case ItemName.Item_Gold: return GameAssets.instantiate.iconGold;
            case ItemName.Item_BonusGold: return GameAssets.instantiate.iconBonusGold;
            case ItemName.Item_Exp: return GameAssets.instantiate.iconExp;
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
            case ItemName.Scroll_Deploy:
                return "Deploy";
            //case ItemName.Scroll_Guide:
            //    return "Guide";
            default:
                return "";
        }
    } 

    // ������ũ�� ȿ�� ���
}
