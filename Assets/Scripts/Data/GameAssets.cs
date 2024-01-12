using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
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

    // ��ų Icon Images
    public Sprite[] spellIconsArray;
    #endregion

    #region Mesh
    public Mesh m_Body_1;
    public Mesh m_Body_2;
    public Mesh m_Body_3;
    public Mesh m_Body_4;
    public Mesh m_Body_5;
    public Mesh m_Body_6;
    public Mesh m_Body_7;
    public Mesh m_Body_8;
    public Mesh m_Body_9;
    public Mesh m_Body_10;
    public Mesh m_Body_11;
    public Mesh m_Body_12;
    public Mesh m_Body_13;
    public Mesh m_Body_14;
    public Mesh m_Body_15;
    public Mesh m_Body_16;
    public Mesh m_Body_17;
    public Mesh m_Body_18;
    public Mesh m_Body_19;
    public Mesh m_Body_20;
    public Mesh m_BackPack_1;
    public Mesh m_BackPack_2;
    public Mesh m_BackPack_3;
    public Mesh m_Wand_1;
    public Mesh m_Wand_2;
    public Mesh m_Wand_3;
    public Mesh m_Wand_4;
    public Mesh m_Wand_5;
    public Mesh m_Wand_6;
    public Mesh m_Wand_7;
    public Mesh m_Hat_1;
    public Mesh m_Hat_2;
    public Mesh m_Hat_3;
    public Mesh m_Hat_4;
    public Mesh m_Hat_5;
    public Mesh m_Hat_6;
    public Mesh m_Hat_7;
    public Mesh m_Hat_8;
    public Mesh m_Hat_9;
    public Mesh m_Hat_10;
    public Mesh m_Hat_11;
    public Mesh m_Hat_12;
    public Mesh m_Hat_13;
    public Mesh m_Hat_14;
    public Mesh m_Scroll_1;
    public Mesh m_Scroll_2;
    public Mesh m_Scroll_3;
    public Mesh m_Scroll_4;
    public Mesh m_Scroll_5;
    #endregion Mesh

    #region Prefab
    // ���� ������
    public GameObject[] spellPrefabList;

    // ĳ���� ������\
    public GameObject wizard_Male_ForLobby;
    public GameObject knight_Male_ForLobby;
    public GameObject wizard_Male;
    public GameObject knight_Male;

    // Game�� ������ ������
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
        return spellPrefabList[SearchSpellNameIndex(spellName)];
    }

    public Sprite GetSpellIconImage(SpellName spellName)
    {
        return spellIconsArray[SearchSpellNameIndex(spellName)];
    }

    /// <summary>
    /// ���� ������ ������ ����Ÿ�Ժ�Start, End �̳��� ������ �ش� ������ ������ Index���� �˷��ִ� �޼ҵ� �Դϴ�.
    /// SpellName�� �����Ͱ� ����� ��� �� �޼ҵ嵵 �Բ� �������־�� �մϴ�.
    /// </summary>
    /// <param name="spellName"></param>
    /// <returns></returns>
    private sbyte SearchSpellNameIndex(SpellName spellName)
    {
        sbyte adjustValue = 0;
        if(spellName >= SpellName.SlashSpellStart)
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
        return (sbyte)(spellName - adjustValue);
    }
}
