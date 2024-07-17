using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameAssetsScriptableObject;

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
        return gameAssets.icon_Scroll;
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
            return gameAssets.music_Title[UnityEngine.Random.Range(0, gameAssets.music_Title.Length)];
        }
        else if (sceneName == LoadSceneManager.Scene.LoadingScene.ToString())
        {
            return null;
        }
        else if (sceneName == LoadSceneManager.Scene.LobbyScene.ToString())
        {
            return gameAssets.music_Lobby[UnityEngine.Random.Range(0, gameAssets.music_Lobby.Length)];
        }
        else if (sceneName == LoadSceneManager.Scene.GameScene_MultiPlayer.ToString())
        {
            return gameAssets.music_Game[UnityEngine.Random.Range(0, gameAssets.music_Game.Length)];
        }
        else if (sceneName == LoadSceneManager.Scene.GameScene_SinglePlayer.ToString())
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

    private AudioClip GetSFXAudioClip(SFX_Clip sFX_Clip, SFX_Type sFX_Type)
    {
        switch (sFX_Type)
        {
            case SFX_Type.Aiming:
                return sFX_Clip.audioClipAiming;
            case SFX_Type.Shooting:
                return sFX_Clip.audioClipShooting;
            case SFX_Type.Hit:
                return sFX_Clip.audioClipHit;
            default: 
                return null;
        }
    }

    public AudioClip GetSkillSFXSound(SkillName skillName, SFX_Type sFX_Type)
    {
        switch (skillName)
        {
            case SkillName.FireBallLv1:
                return GetSFXAudioClip(gameAssets.sfx_Fireball_Lv1, sFX_Type);
            case SkillName.WaterBallLv1:
                return GetSFXAudioClip(gameAssets.sfx_Waterball_Lv1, sFX_Type);
            case SkillName.IceBallLv1:
                return GetSFXAudioClip(gameAssets.sfx_Iceball_Lv1, sFX_Type);
            case SkillName.BlizzardLv1:
                return GetSFXAudioClip(gameAssets.sfx_Blizzard_Lv1, sFX_Type);
            case SkillName.MagicShieldLv1:
                return GetSFXAudioClip(gameAssets.sfx_MagicShield_Lv1, sFX_Type);
            case SkillName.StoneSlashAttack1_Lv1:
                return GetSFXAudioClip(gameAssets.sfx_ElectricSlashAttackVertical_Lv1, sFX_Type);
            case SkillName.ElectricSlashAttackVertical_Lv1:
                return GetSFXAudioClip(gameAssets.sfx_ElectricSlashAttackVertical_Lv1, sFX_Type);
            case SkillName.ElectricSlashAttackWhirlwind_Lv1:
                return GetSFXAudioClip(gameAssets.sfx_ElectricSlashAttackWhirlwind_Lv1, sFX_Type);
            case SkillName.ElectricSlashAttackChargeSlash_Lv1:
                return GetSFXAudioClip(gameAssets.sfx_ElectricSlashAttackChargeSlash_Lv1, sFX_Type);
            case SkillName.Dash_Lv1:
                return GetSFXAudioClip(gameAssets.sfx_Dash_Lv1, sFX_Type);
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
            case ItemName.ScrollOpen:
                return gameAssets.sfx_OpenScroll;
            case ItemName.ScrollPickup:
                return gameAssets.sfx_PickupScroll;
            case ItemName.ScrollUse:
                return gameAssets.sfx_UseScroll;
            default:
                Debug.LogError($"�������� ���� itemName �����Դϴ�. itemName: {itemName}");
                return null;
        }
    }

    public AudioClip GetUISFX(UISFX_Type uISFX_Type)
    {
        switch (uISFX_Type)
        {
            case UISFX_Type.Succeeded_Match:
                return gameAssets.sfx_GameMatchFound;
            case UISFX_Type.Failed_Match:
                return gameAssets.sfx_GameMatchCanceled;
            default: return null;
        }
    }

    public AudioClip GetCountdownAnnouncerSFXSound(int index)
    {
        return gameAssets.sfx_GameStartCountdown[index];
    }

    public AudioClip[] GetWinSFXSound()
    {
        return gameAssets.sfx_Win;
    }
    public AudioClip[] GetLoseSFXSound()
    {
        return gameAssets.sfx_Lose;
    }

    public GameObject GetItemHPPotionObject()
    {
        return gameAssets.item_HPPotion;
    }

    public GameObject GetItemScrollObject()
    {
        return gameAssets.item_Scroll;
    }
}
