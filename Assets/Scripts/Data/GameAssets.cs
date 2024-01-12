using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 게임에 필요한 애셋들(Mesh 등등...)을 갖고있는 스크립트
/// 필요할때마다 접근해서 꺼내 쓰면 된다.
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

    // 스킬 Icon Images
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
    // 스펠 프리팹
    public GameObject[] spellPrefabList;

    // 캐릭터 프리팹\
    public GameObject wizard_Male_ForLobby;
    public GameObject knight_Male_ForLobby;
    public GameObject wizard_Male;
    public GameObject knight_Male;

    // Game씬 아이템 프리팹
    /// <summary>
    /// 1. 레벨업
    /// 2. 발사속도
    /// 3. 점착
    /// 4. 유도(호버링 )
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
    /// 스펠 네임을 넣으면 마법타입별Start, End 이넘을 제외한 해당 스펠의 순수한 Index값을 알려주는 메소드 입니다.
    /// SpellName에 데이터가 변경될 경우 이 메소드도 함께 수정해주어야 합니다.
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
