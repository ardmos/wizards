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

    private SpellInfo[] spellDefaultSpec = new SpellInfo[Enum.GetNames(typeof(SpellName)).Length];

    private void Awake()
    {
        Instance = this;
        InitSpellDefaultSpecs();
    }

    private void InitSpellDefaultSpecs()
    {
        // Wizard_Male
        // 공격 마법
        SetSpellDefaultSpec(
            spellType: SpellType.Fire,
            coolTime: 5.0f,
            lifeTime: 10.0f,
            moveSpeed: 10.0f,
            price: 30,
            damage: 1,
            spellName: SpellName.FireBallLv1,
            spellState: SpellState.Ready,
            upgradeOptionsCount: System.Enum.GetValues(typeof(FireballUpgradeOption)).Length
        );
        SetSpellDefaultSpec(
            spellType: SpellType.Water,
            coolTime: 5.0f,
            lifeTime: 10.0f,
            moveSpeed: 5.0f,
            price: 30,
            damage: 1,
            spellName: SpellName.WaterBallLv1,
            spellState: SpellState.Ready,
            upgradeOptionsCount: System.Enum.GetValues(typeof(WaterballUpgradeOption)).Length
        );
        SetSpellDefaultSpec(
            spellType: SpellType.Ice,
            coolTime: 5.0f,
            lifeTime: 10.0f,
            moveSpeed: 10.0f,
            price: 30,
            damage: 1,
            spellName: SpellName.IceBallLv1,
            spellState: SpellState.Ready,
            upgradeOptionsCount: 0
        );

        SetSpellDefaultSpec(
            spellType: SpellType.Ice,
            coolTime: 10.0f,
            lifeTime: 4.0f,
            moveSpeed: 0f,
            price: 30,
            damage: 1,
            spellName: SpellName.BlizzardLv1,
            spellState: SpellState.Ready,
            upgradeOptionsCount: System.Enum.GetValues( typeof( BlizzardUpgradeOption ) ).Length
        );
        // 방어 마법
        SetSpellDefaultSpec(
            spellType: SpellType.Arcane,
            coolTime: 10.0f,
            lifeTime: 2.2f,
            moveSpeed: 0f,
            price: 30,
            damage: 1,
            spellName: SpellName.MagicShieldLv1,
            spellState: SpellState.Ready,
            upgradeOptionsCount: 0
        );

        // Knight_Male
        // 공격 스킬
        SetSpellDefaultSpec(
            spellType: SpellType.Stone,
            coolTime: 5.0f,
            lifeTime: 1.0f,
            moveSpeed: 10.0f,
            price: 30,
            damage: 1,
            spellName: SpellName.StoneSlashAttack1_Lv1,
            spellState: SpellState.Ready,
            upgradeOptionsCount: 0
        );
        SetSpellDefaultSpec(
            spellType: SpellType.Electric,
            coolTime: 5.0f,
            lifeTime: 1.0f,
            moveSpeed: 10.0f,
            price: 30,
            damage: 1,
            spellName: SpellName.ElectricSlashAttackVertical_Lv1,
            spellState: SpellState.Ready,
            upgradeOptionsCount: 0
        );
        SetSpellDefaultSpec(
            spellType: SpellType.Electric,
            coolTime: 5.0f,
            lifeTime: 1.0f,
            moveSpeed: 0f,
            price: 30,
            damage: 1,
            spellName: SpellName.ElectricSlashAttackWhirlwind_Lv1,
            spellState: SpellState.Ready,
            upgradeOptionsCount: 0
        );
        SetSpellDefaultSpec(
            spellType: SpellType.Electric,
            coolTime: 10.0f,
            lifeTime: 1.0f,
            moveSpeed: 10.0f,
            price: 30,
            damage: 1,
            spellName: SpellName.ElectricSlashAttackChargeSlash_Lv1,
            spellState: SpellState.Ready,
            upgradeOptionsCount: 0
        );
        // 방어
        SetSpellDefaultSpec(
            spellType: SpellType.Normal,
            coolTime: 5.0f,
            lifeTime: 0f,
            moveSpeed: 0f,
            price: 30,
            damage: 1,
            spellName: SpellName.Dash_Lv1,
            spellState: SpellState.Ready,
            upgradeOptionsCount : 0
        );
    }

    public SpellInfo GetSpellDefaultSpec(SpellName spellName)
    {
        if (spellDefaultSpec.Length <= (int)spellName)
        {
            Debug.LogError("해당 이름의 스펠 스펙을 찾을 수 없습니다.");
            return null;
        }

        return spellDefaultSpec[(int)spellName];
    }

    /// <summary>
    /// Spell의 기본 스펙을 설정해주는 메소드 입니다.
    /// </summary>
    /// <returns></returns>
    private void SetSpellDefaultSpec(SpellType spellType, float coolTime, float lifeTime, float moveSpeed, int price, byte damage, SpellName spellName, SpellState spellState, int upgradeOptionsCount)
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
            damage,
            upgradeOptionsCount);

        spellDefaultSpec[(int)spellName] = spellInfo;
    }
}