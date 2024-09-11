using System;
using UnityEngine;

/// <summary>
/// ========= ���� �ʿ�.  ������� Wizard�� Knightó�� ���� ��ũ��Ʈ�� ����ִ°� �������� �𸨴ϴ�. ==================
/// �� ������� �� ������ ����ִ� ��ũ��Ʈ�Դϴ�.
/// ��û������ ������ �����մϴ�.
/// Game���� SpellSpecifications ������Ʈ�� ������Ʈ �Դϴ�. Game���� ����������Ŭ�� �Բ��մϴ�.
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
        // ���� ����
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
        // ��� ����
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
        // ���� ��ų
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
        // ���
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
            Debug.LogError("�ش� �̸��� ���� ������ ã�� �� �����ϴ�.");
            return null;
        }

        return spellDefaultSpec[(int)spellName];
    }

    /// <summary>
    /// Spell�� �⺻ ������ �������ִ� �޼ҵ� �Դϴ�.
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