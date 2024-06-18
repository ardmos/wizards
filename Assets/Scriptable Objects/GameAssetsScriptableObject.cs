using UnityEngine;

[CreateAssetMenu(fileName = "NewGameAssets", menuName = "ScriptableObjects/GameAssets", order = 1)]
public class GameAssetsScriptableObject : ScriptableObject
{
    #region Icons
    [Header("Icons")]
    public Sprite icon_WizardClass;
    public Sprite icon_KnightClass;
    public Sprite icon_Gold;
    public Sprite icon_BonusGold;
    public Sprite icon_Exp;
    public Sprite icon_RemoveAds;
    [Header("Icons InGame")]
    public Sprite icon_Scroll;
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
    [Header("Spells")]
    public SkillAssets[] skillAssetsList;
    #endregion

    #region etc
    [Header("AI플레이어 프리팹")]
    public GameObject wizard_Male_AI;

    [Header("플레이어 캐릭터 프리팹")]
    public GameObject wizard_Male_ForLobby;
    public GameObject knight_Male_ForLobby;
    public GameObject wizard_Male;
    public GameObject knight_Male;

    [Header("VFX 프리팹")]
    public GameObject vfx_Heal;
    public GameObject vfx_SpellUpgrade;
    public GameObject vfx_txtDamageValue;
    #endregion

    #region Items
    [Header("Items")]
    public GameObject item_Scroll;
    #endregion

    #region Musics
    [Header("Musics")]
    public AudioClip[] music_Title;
    public AudioClip[] music_Lobby;
    public AudioClip[] music_Game;
    #endregion

    #region SFX
    [System.Serializable]
    public struct SFX_Clip
    {
        public AudioClip audioClipAiming;
        public AudioClip audioClipShooting;
        public AudioClip audioClipHit;
    }

    [Header("SFXs")]
    public AudioClip sfx_btnClick;
    public SFX_Clip sfx_Fireball_Lv1;
    public SFX_Clip sfx_Waterball_Lv1;
    public SFX_Clip sfx_Iceball_Lv1;
    public SFX_Clip sfx_Blizzard_Lv1;
    public SFX_Clip sfx_MagicShield_Lv1;
    public SFX_Clip sfx_ElectricSlashAttackVertical_Lv1;
    public SFX_Clip sfx_ElectricSlashAttackWhirlwind_Lv1;
    public SFX_Clip sfx_ElectricSlashAttackChargeSlash_Lv1;
    public SFX_Clip sfx_Dash_Lv1;
    public AudioClip sfx_PotionHP;
    public AudioClip sfx_PickupScroll;
    public AudioClip sfx_OpenScroll;
    public AudioClip sfx_UseScroll;

    public AudioClip[] sfx_Win;
    public AudioClip[] sfx_Lose;
    public AudioClip sfx_GameMatchFound;
    public AudioClip sfx_GameMatchCanceled;
    public AudioClip[] sfx_GameStartCountdown;
    public AudioClip sfx_GetHit;
    #endregion

    #region Colors
    [Header("Colors")]
    public Color color_Owner;
    public Color color_Ally; // 동맹시스템 추가시 사용
    public Color color_Enemy;
    #endregion
}
