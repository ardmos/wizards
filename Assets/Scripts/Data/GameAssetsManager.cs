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
    /// 인게임용 아닌, 로비씬 등에서 사용되는 캐릭터 프리팹을 리턴해는 메소드 입니다.
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
                Debug.LogError($"적절하지 않은 characterClass 정보입니다. characterClass: {characterClass}");
                break;
        }
        return resultObejct;
    }

    /// <summary>
    /// 인게임용 캐릭터 프리팹을 리턴해주는 메소드 입니다
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
                Debug.LogError($"적절하지 않은 characterClass 정보입니다. characterClass: {characterClass}");
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
        else if (sceneName == LoadSceneManager.Scene.GameScene.ToString())
        {
            return gameAssets.music_Game[UnityEngine.Random.Range(0, gameAssets.music_Game.Length)];
        }
        else
        {
            Debug.Log($"{nameof(GetMusic)} sceneName 파라미터가 잘못됐습니다.");
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
                Debug.LogError($"{nameof(GetSkillSFXSound)}. {skillName}은 알맞은 skillName이 아닙니다.");
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
            case ItemName.ScrollStart:
                return gameAssets.sfx_PickupScroll;
            case ItemName.OpenScroll:
                return gameAssets.sfx_OpenScroll;
            case ItemName.PickupScroll:
                return gameAssets.sfx_PickupScroll;
            default:
                Debug.LogError($"적절하지 않은 itemName 정보입니다. itemName: {itemName}");
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
}
