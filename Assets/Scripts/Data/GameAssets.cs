using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// ���ӿ��� ���Ǵ� �ּµ��� �����ϴ� ��ũ��Ʈ �Դϴ�.
/// �ּµ��� �������� ��û�Ͽ� ���� �� �ֽ��ϴ�.
/// </summary>
public class GameAssets : MonoBehaviour
{
    private static GameAssets _instantiate;
    public static GameAssets instantiate { 
        get{
            if (_instantiate == null) _instantiate = Instantiate(Resources.Load<GameAssets>("GameAssets"));
            return _instantiate;
        }
    }

    #region Icons
    public Sprite icon_WizardClass;
    public Sprite icon_KnightClass;
    public Sprite icon_Gold;
    public Sprite icon_BonusGold;
    public Sprite icon_Exp;
    public Sprite icon_ScrollEffect_LevelUp;
    public Sprite icon_ScrollEffect_FireRateUp;
    public Sprite icon_ScrollEffect_FlySpeedUp;
    public Sprite icon_ScrollEffect_Deploy;
    public Sprite icon_RemoveAds;
    #endregion

    #region Spells
    [System.Serializable]
    public struct SpellAssets
    {
        public Sprite icon;
        public GameObject prefab;
    }
    // ���� �����ܰ� ������
    public List<SpellAssets> spellAssetsList = new List<SpellAssets>();
    #endregion

    #region etc
    // ĳ����
    public GameObject wizard_Male_ForLobby;
    public GameObject knight_Male_ForLobby;
    public GameObject wizard_Male;
    public GameObject knight_Male;    

    // VFX
    public GameObject vfx_Heal;
    public GameObject vfx_SpellUpgrade;
    public GameObject vfx_txtDamageValue;
    #endregion

    #region Music
    public AudioClip music_Title;
    public AudioClip music_Lobby;
    public AudioClip[] music_Game;
    #endregion

    #region SFX
    public AudioClip sfx_btnClick;
    public AudioClip[] sfx_Fireball_Lv1;
    public AudioClip[] sfx_Waterball_Lv1;
    public AudioClip[] sfx_Iceball_Lv1;
    public AudioClip sfx_PotionHP;
    public AudioClip sfx_OpenScroll;
    public AudioClip sfx_ScrollLvUp;    
    public AudioClip sfx_ScrollFireRateUp;
    public AudioClip sfx_ScrollFlySpeedUp;
    public AudioClip sfx_ScrollAttach;
    public AudioClip sfx_ScrollGuide;

    public AudioClip[] sfx_Win;
    public AudioClip[] sfx_Lose;
    #endregion

    #region Colors
    public Color color_Owner;
    public Color color_Ally; // ���ͽý��� �߰��� ���
    public Color color_Enemy;
    #endregion

    public GameObject GetSpellPrefab(SpellName spellName)
    {
        return spellAssetsList[SearchSpellNameIndex(spellName)].prefab;
    }

    public Sprite GetSpellIconImage(SpellName spellName)
    {
        return spellAssetsList[SearchSpellNameIndex(spellName)].icon;
    }

    public Sprite GetScrollEffectIconImage(ItemName itemName)
    {
        switch (itemName)
        {
            case ItemName.Scroll_LevelUp:
                return icon_ScrollEffect_LevelUp;
            case ItemName.Scroll_FireRateUp:
                return icon_ScrollEffect_FireRateUp;
            case ItemName.Scroll_FlySpeedUp:
                return icon_ScrollEffect_FlySpeedUp;
            case ItemName.Scroll_Deploy:
                return icon_ScrollEffect_Deploy;
            default:
                return null;
        }
    }

    /// <summary>
    /// ���� ������ ������ ���� ī�װ��� ���۰� ���� �˸��� Start&End �̳��� ������ ������ �ش� ������ ������ Index���� �˷��ִ� �޼ҵ� �Դϴ�.
    /// SpellName�� ���ο� ���� Start&End ī�װ��� �߰��ǰų�, ���� ī�װ� ������ ����� ��� �� �޼ҵ嵵 �Բ� �������־�� �մϴ�.
    /// ���� ī�װ��� �ƴ�, �ϳ� �ϳ��� �������� �߰��ǰų� ������ ����Ǵ°� ��������ϴ�.
    /// ���� ī�װ� : ex) ������Lv1�� ��� FireSpellStart, FireSpellEnd, WaterSpellStart �� �� ���� �����ϱ� ������ adjustValue�� 3�� �˴ϴ�.
    /// </summary>
    private byte SearchSpellNameIndex(SpellName spellName)
    {
        byte adjustValue = 0;
        if (spellName >= SpellName.DefenceSpellStart)
        {
            adjustValue = 9;
        }
        else if(spellName >= SpellName.SlashSpellStart)
        {
            adjustValue = 7;
        }
        else if(spellName >= SpellName.IceSpellStart)
        {
            adjustValue = 5;
        }
        else if (spellName >= SpellName.WaterSpellStart)
        {
            adjustValue = 3;
        }
        else if (spellName >= SpellName.FireSpellStart)
        {
            adjustValue = 1;
        }

        return (byte)(spellName - adjustValue);
    }

    /// <summary>
    /// �ΰ��ӿ� �ƴ�, �κ�� ��� ���Ǵ� ĳ���� �������� �����ش� �޼ҵ� �Դϴ�.
    /// </summary>
    public GameObject GetCharacterPrefab_NotInGame(CharacterClass characterClass)
    {
        GameObject resultObejct = null;
        switch (characterClass)
        {
            case CharacterClass.Wizard:
                resultObejct = wizard_Male_ForLobby;
                break;
            case CharacterClass.Knight:
                resultObejct = knight_Male_ForLobby;
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
    public GameObject GetCharacterPrefab_InGame(CharacterClass characterClass)
    {
        GameObject resultObejct = null;
        switch (characterClass)
        {
            case CharacterClass.Wizard:
                resultObejct = wizard_Male;
                break;
            case CharacterClass.Knight:
                resultObejct = knight_Male;
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
            return music_Title;
        }
        else if (sceneName == LoadSceneManager.Scene.LoadingScene.ToString())
        {
            return null;
        }
        else if (sceneName == LoadSceneManager.Scene.LobbyScene.ToString())
        {
            return music_Lobby;
        }
        else if (sceneName == LoadSceneManager.Scene.GameScene.ToString())
        {
            return music_Game[UnityEngine.Random.Range(0, music_Game.Length)];
        }
        else
        {
            Debug.Log($"{nameof(GetMusic)} sceneName �Ķ���Ͱ� �߸��ƽ��ϴ�.");
            return null;
        }
    }

    public AudioClip GetButtonClickSound() { return sfx_btnClick; }

    public AudioClip GetMagicSFXSound(SpellName spellName, byte state) 
    {
        switch (spellName) {
            case SpellName.FireBallLv1:
                return sfx_Fireball_Lv1[state];
            case SpellName.WaterBallLv1:
                return sfx_Waterball_Lv1[state];
            case SpellName.IceBallLv1:
                return sfx_Iceball_Lv1[state];
            default: 
                Debug.LogError($"{nameof(GetMagicSFXSound)}. {spellName}�� �˸��� spellName�� �ƴմϴ�."); 
                return null;
        }
    }

    public AudioClip GetItemSFXSound(ItemName itemName)
    {
        switch (itemName)
        {
            case ItemName.Potion_HP:
                return sfx_PotionHP;
            case ItemName.Scroll_LevelUp:
                return sfx_ScrollLvUp;
            case ItemName.Scroll_FireRateUp:
                return sfx_ScrollFireRateUp;
            case ItemName.Scroll_FlySpeedUp:
                return sfx_ScrollFlySpeedUp;
            case ItemName.Scroll_Deploy:
                return sfx_ScrollAttach;
            default:
                Debug.LogError($"�������� ���� itemName �����Դϴ�. itemName: {itemName}");
                return null;
        }
    }

    public AudioClip[] GetWinSFXSound() {
        return sfx_Win;
    }
    public AudioClip[] GetLoseSFXSound()
    {
        return sfx_Lose;
    }
    public AudioClip GetOpenScrollItemSFXSound() {
        return sfx_OpenScroll;
    }
}
