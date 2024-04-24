using System;
using UnityEngine;

/// <summary>
/// ========= 수정 필요.  스펙들은 Wizard나 Knight처럼 각각 스크립트에 담겨있는게 나을지도 모릅니다. ==================
/// 각 스펠들의 상세 스펙을 담고있는 스크립트입니다.
/// 요청받으면 스펙을 제공합니다.
/// Game씬의 SpellSpecifications 오브젝트의 컴포넌트 입니다. Game씬과 라이프사이클을 함께합니다.
/// </summary>
public class SpellSpecifications : MonoBehaviour
{
    public static SpellSpecifications Instance;

    private SpellInfo[] spellDefaultSpec = new SpellInfo[Enum.GetNames(typeof(SkillName)).Length];

    private void Awake()
    {
        Instance = this;
        InitSpellDefaultSpecs();
    }

    private void InitSpellDefaultSpecs()
    {
        // Wizard_Male
        // 공격 마법
        SetSpellDefaultSpec(SpellType.Fire, 2.0f, 10.0f, 10.0f, 30, 1, SkillName.FireBallLv1, SpellState.Ready);
        SetSpellDefaultSpec(SpellType.Water, 2.0f, 10.0f, 10.0f, 30, 1, SkillName.WaterBallLv1, SpellState.Ready);
        SetSpellDefaultSpec(SpellType.Ice, 2.0f, 10.0f, 10.0f, 30, 1, SkillName.IceBallLv1, SpellState.Ready);
        // 방어 마법
        SetSpellDefaultSpec(SpellType.Arcane, 10.0f, 2.2f, 0f, 30, 1, SkillName.MagicShieldLv1, SpellState.Ready);

        // Knight_Male
        // 공격 스킬
        SetSpellDefaultSpec(SpellType.Stone, 2.0f, 1.0f, 10.0f, 30, 1, SkillName.StoneSlashAttack1_Lv1, SpellState.Ready);
        SetSpellDefaultSpec(SpellType.Electric, 2.0f, 1.0f, 10.0f, 30, 1, SkillName.ElectricSlashAttack1_Lv1, SpellState.Ready);
        SetSpellDefaultSpec(SpellType.Electric, 2.0f, 1.0f, 10.0f, 30, 1, SkillName.ElectricSlashAttack2_Lv1, SpellState.Ready);
        //방어
        SetSpellDefaultSpec(SpellType.Normal, 2.0f, 0f, 0f, 30, 1, SkillName.Dash_Lv1, SpellState.Ready);
    }

    public SpellInfo GetSpellDefaultSpec(SkillName spellName)
    {
        if (spellDefaultSpec.Length <= (int)spellName) return null;

        return spellDefaultSpec[(int)spellName];
    }

    /// <summary>
    /// Spell의 기본 스펙을 설정해주는 메소드 입니다.
    /// </summary>
    /// <returns></returns>
    private void SetSpellDefaultSpec(SpellType spellType, float coolTime, float lifeTime, float moveSpeed, int price, byte level, SkillName spellName, SpellState spellState)
    {
        SpellInfo spellInfo;
        spellInfo = new SpellInfo(
            spellType,
            spellName,
            spellState,
            coolTime,
            lifeTime,
            moveSpeed,
            price,
            level);

        spellDefaultSpec[(int)spellName] = spellInfo;
    }
}