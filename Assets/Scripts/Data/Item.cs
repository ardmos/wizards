using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// GameAssets와 유사하게 게임 내에 존재하는 모든 Item들의 가격, Icon, 이름 등을 동적으로 반환해주는 스크립트 입니다. 
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
            case ItemName.RemoveAds: return GameAssetsManager.Instance.gameAssets.icon_RemoveAds;
            case ItemName.Item_Wizard: return GameAssetsManager.Instance.gameAssets.icon_WizardClass;
            case ItemName.Item_Knight: return GameAssetsManager.Instance.gameAssets.icon_KnightClass;
            case ItemName.Item_Gold: return GameAssetsManager.Instance.gameAssets.icon_Gold;
            case ItemName.Item_BonusGold: return GameAssetsManager.Instance.gameAssets.icon_BonusGold;
            case ItemName.Item_Exp: return GameAssetsManager.Instance.gameAssets.icon_Exp;
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
            default:
                return "";
        }
    } 
}
