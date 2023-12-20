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

    #region Sprites
    public Sprite s_RemoveAds;
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
    // 직업별 스펠 프리팹
    public GameObject[] spellListWizard;
    public GameObject[] spellListKnight;


    // 테스트용 프리팹
    public GameObject fireBall_1;
    public GameObject waterBall_1;
    public GameObject iceBall_1;

    // 캐릭터 프리팹\
    public GameObject wizard_Male_ForLobby;
    public GameObject knight_Male_ForLobby;
    public GameObject wizard_Male;
    public GameObject knight_Male;
    #endregion

    #region Music
    public AudioClip music_Title;
    public AudioClip music_Lobby;
    #endregion

    #region SFX

    #endregion

    public GameObject GetWizardSpellPrefab(CharacterClasses.Class playerClass, SpellNames.Wizard spellName)
    {
        return spellListWizard[(int)spellName];
    }
    public GameObject GetKnightSpellPrefab(CharacterClasses.Class playerClass, SpellNames.Knight spellName)
    {
        return spellListKnight[(int)spellName];
    }
}
