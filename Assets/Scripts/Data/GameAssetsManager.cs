using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameAssetsManager : MonoBehaviour
{
    public GameAssetsScriptableObject gameAssets;

    private static GameAssetsManager instance;
    public static GameAssetsManager Instance
    {
        get
        {
            if (instance == null) instance = Instantiate(Resources.Load<GameAssetsManager>("GameAssetsManager"));
            return instance;
        }
    }

    public GameObject GetSpellPrefab(SkillName skillName)
    {
        //int index = SearchSpellNameIndex(spellName);
        int index = (int)skillName;
        return gameAssets.skillAssetsList[index].prefab;
    }

    public Sprite GetSpellIconImage(SkillName skillName)
    {
        //int index = SearchSpellNameIndex(spellName);
        int index = (int)skillName;
        return gameAssets.skillAssetsList[index].icon;
    }

    public Sprite GetScrollEffectIconImage(ItemName itemName)
    {
        switch (itemName)
        {
            case ItemName.Scroll_LevelUp:
                return gameAssets.icon_ScrollEffect_LevelUp;
            case ItemName.Scroll_FireRateUp:
                return gameAssets.icon_ScrollEffect_FireRateUp;
            case ItemName.Scroll_FlySpeedUp:
                return gameAssets.icon_ScrollEffect_FlySpeedUp;
            case ItemName.Scroll_Deploy:
                return gameAssets.icon_ScrollEffect_Deploy;
            default:
                return null;
        }
    }

    /// <summary>
    /// �ΰ��ӿ� �ƴ�, �κ�� ��� ���Ǵ� ĳ���� �������� �����ش� �޼ҵ� �Դϴ�.
    /// </summary>
    public GameObject GetCharacterPrefab_NotInGame(Character characterClass)
    {
        GameObject resultObejct = null;
        switch (characterClass)
        {
            case Character.Wizard:
                resultObejct = gameAssets.wizard_Male_ForLobby;
                break;
            case Character.Knight:
                resultObejct = gameAssets.knight_Male_ForLobby;
                break;
            default:
                Debug.LogError($"�������� ���� characterClass �����Դϴ�. characterClass: {characterClass}");
                break;
        }
        return resultObejct;
    }

    /// <summary>
    /// �ΰ��ӿ� ĳ���� �������� �������ִ� �޼ҵ� �Դϴ�
    /// </summary>
    public GameObject GetCharacterPrefab_InGame(Character characterClass)
    {
        GameObject resultObejct = null;
        switch (characterClass)
        {
            case Character.Wizard:
                resultObejct = gameAssets.wizard_Male;
                break;
            case Character.Knight:
                resultObejct = gameAssets.knight_Male;
                break;
            default:
                Debug.LogError($"�������� ���� characterClass �����Դϴ�. characterClass: {characterClass}");
                break;
        }
        return resultObejct;
    }

    public AudioClip GetMusic(string sceneName)
    {
        if (sceneName == LoadSceneManager.Scene.TitleScene.ToString())
        {
            return gameAssets.music_Title;
        }
        else if (sceneName == LoadSceneManager.Scene.LoadingScene.ToString())
        {
            return null;
        }
        else if (sceneName == LoadSceneManager.Scene.LobbyScene.ToString())
        {
            return gameAssets.music_Lobby;
        }
        else if (sceneName == LoadSceneManager.Scene.GameScene.ToString())
        {
            return gameAssets.music_Game[UnityEngine.Random.Range(0, gameAssets.music_Game.Length)];
        }
        else
        {
            Debug.Log($"{nameof(GetMusic)} sceneName �Ķ���Ͱ� �߸��ƽ��ϴ�.");
            return null;
        }
    }

    public AudioClip GetButtonClickSound() { return gameAssets.sfx_btnClick; }

    public AudioClip GetMagicSFXSound(SkillName spellName, byte state)
    {
        switch (spellName)
        {
            case SkillName.FireBallLv1:
                return gameAssets.sfx_Fireball_Lv1[state];
            case SkillName.WaterBallLv1:
                return gameAssets.sfx_Waterball_Lv1[state];
            case SkillName.IceBallLv1:
                return gameAssets.sfx_Iceball_Lv1[state];
            case SkillName.MagicShieldLv1:
                return gameAssets.sfx_MagicShield_Lv1[state];
            default:
                Debug.LogError($"{nameof(GetMagicSFXSound)}. {spellName}�� �˸��� spellName�� �ƴմϴ�.");
                return null;
        }
    }

    public AudioClip GetSkillSFXSound(SkillName skillName, byte state)
    {
        switch (skillName)
        {
            case SkillName.StoneSlashAttack1_Lv1:
                return gameAssets.sfx_ElectricSlashAttack1_Lv1[state];
            case SkillName.ElectricSlashAttack1_Lv1:
                return gameAssets.sfx_ElectricSlashAttack1_Lv1[state];
            case SkillName.ElectricSlashAttack2_Lv1:
                return gameAssets.sfx_ElectricSlashAttack2_Lv1[state];
            case SkillName.Dash_Lv1:
                return gameAssets.sfx_Dash_Lv1[state];
            default:
                Debug.LogError($"{nameof(GetSkillSFXSound)}. {skillName}�� �˸��� skillName�� �ƴմϴ�.");
                return null;
        }
    }

    public AudioClip GetItemSFXSound(ItemName itemName)
    {
        switch (itemName)
        {
            case ItemName.Potion_HP:
                return gameAssets.sfx_PotionHP;
            case ItemName.Scroll_LevelUp:
                return gameAssets.sfx_ScrollLvUp;
            case ItemName.Scroll_FireRateUp:
                return gameAssets.sfx_ScrollFireRateUp;
            case ItemName.Scroll_FlySpeedUp:
                return gameAssets.sfx_ScrollFlySpeedUp;
            case ItemName.Scroll_Deploy:
                return gameAssets.sfx_ScrollAttach;
            default:
                Debug.LogError($"�������� ���� itemName �����Դϴ�. itemName: {itemName}");
                return null;
        }
    }

    public AudioClip[] GetWinSFXSound()
    {
        return gameAssets.sfx_Win;
    }
    public AudioClip[] GetLoseSFXSound()
    {
        return gameAssets.sfx_Lose;
    }
    public AudioClip GetOpenScrollItemSFXSound()
    {
        return gameAssets.sfx_OpenScroll;
    }
}