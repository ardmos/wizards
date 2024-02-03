using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// �� ��ũ��Ʈ ���� �ʿ�. ����ϰ�. 
/// 
/// ���ӿ� �ʿ��� �ּµ�(Mesh ���...)�� �����ִ� ��ũ��Ʈ
/// �ʿ��Ҷ����� �����ؼ� ���� ���� �ȴ�.
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

    #region Item Icons
    public Sprite IconWizardClass;
    public Sprite IconKnightClass;
    public Sprite IconGold;
    public Sprite IconBonusGold;
    public Sprite IconExp;
    #endregion


    #region Sprites
    public Sprite s_RemoveAds;
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


    #region Prefab etc
    // ĳ���� ������\
    public GameObject wizard_Male_ForLobby;
    public GameObject knight_Male_ForLobby;
    public GameObject wizard_Male;
    public GameObject knight_Male;

    // Game�� ȹ�� ������ �����۵� ������
    /// <summary>
    /// 1. ������
    /// 2. �߻�ӵ�
    /// 3. ����
    /// 4. ����(ȣ���� )
    /// </summary>
    public GameObject scrollLevelUp;
    public GameObject scrollFireRateUp;
    public GameObject scrollFlySpeedUp;
    public GameObject scrollAttach;
    public GameObject scrollGuide;
    public GameObject potionHP;
    public GameObject coin1;
    public GameObject coin3;
    public GameObject coin5;

    // VFX
    public GameObject vfxHeal;
    public GameObject vfxSpellUpgrade;

    #endregion

    #region Music
    public AudioClip music_Title;
    public AudioClip music_Lobby;
    #endregion

    #region SFX

    #endregion

    public GameObject GetSpellPrefab(SpellName spellName)
    {
        Debug.Log($"{nameof(GetSpellPrefab)} requested spellName: {spellName}");
        return spellAssetsList[SearchSpellNameIndex(spellName)].prefab;
    }

    public Sprite GetSpellIconImage(SpellName spellName)
    {
        return spellAssetsList[SearchSpellNameIndex(spellName)].icon;
    }

    /// <summary>
    /// ���� ������ ������ ���� ī�װ��� ���۰� ���� �˸��� Start&End �̳��� ������ ������ �ش� ������ ������ Index���� �˷��ִ� �޼ҵ� �Դϴ�.
    /// SpellName�� ���ο� ���� Start&End ī�װ��� �߰��ǰų�, ���� ī�װ� ������ ����� ��� �� �޼ҵ嵵 �Բ� �������־�� �մϴ�.
    /// ���� ī�װ��� �ƴ�, �ϳ� �ϳ��� �������� �߰��ǰų� ������ ����Ǵ°� ��������ϴ�.
    /// ���� ī�װ� : ex) ������Lv1�� ��� FireSpellStart, FireSpellEnd, WaterSpellStart �� �� ���� �����ϱ� ������ adjustValue�� 3�� �˴ϴ�.
    /// </summary>
    /// <param name="spellName"></param>
    /// <returns></returns>
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

    // Lobby Scene ����. Client ���� ����� 
    public GameObject GetCharacterPrefab_NotInGame(CharacterClass characterClass)
    {
        //Debug.Log($"GetCurrentSelectedCharacterPrefab_NotInGame currentSelectedClass: {currentSelectedClass}");
        // ������ ���⼭ ������� �ݿ��ؼ� ��ȯ�������. ������ Ŭ������ �ݿ��ؼ� ��ȯ����
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
                break;
        }
        //Debug.Log($"GetCurrentSelectedCharacterPrefab_NotInGame resultObject: {resultObejct?.name}");
        return resultObejct;
    }

    // Lobby Scene ����. Client ���� �����
    public GameObject GetCharacterPrefab_InGame(CharacterClass characterClass)
    {
        // ������ ���⼭ ������� �ݿ��ؼ� ��ȯ�������. ������ Ŭ������ �ݿ��ؼ� ��ȯ����
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
                break;
        }

        return resultObejct;
    }
}
