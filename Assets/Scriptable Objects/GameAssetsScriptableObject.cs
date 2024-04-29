using UnityEngine;

[CreateAssetMenu(fileName = "NewGameAssets", menuName = "ScriptableObjects/GameAssets", order = 1)]
public class GameAssetsScriptableObject : ScriptableObject
{
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
    public struct SkillAssets
    {
        public Sprite icon;
        public GameObject prefab;
        public string name;
    }
    // 스펠 아이콘과 프리팹
    public SkillAssets[] skillAssetsList;
    #endregion

    #region etc
    // 캐릭터
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
    [System.Serializable]
    public struct SFX_Clip
    {
        public AudioClip audioClip;
        public string audioName;
    }

    public AudioClip sfx_btnClick;
    [Header("0:Casting, 1:Shooting, 2:Hit")]
    public SFX_Clip[] sfx_Fireball_Lv1;
    public SFX_Clip[] sfx_Waterball_Lv1;
    public SFX_Clip[] sfx_Iceball_Lv1;
    public SFX_Clip[] sfx_MagicShield_Lv1;
    public SFX_Clip[] sfx_ElectricSlashAttack1_Lv1;
    public SFX_Clip[] sfx_ElectricSlashAttack2_Lv1;
    public SFX_Clip[] sfx_Dash_Lv1;
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
    public Color color_Ally; // 동맹시스템 추가시 사용
    public Color color_Enemy;
    #endregion
}
