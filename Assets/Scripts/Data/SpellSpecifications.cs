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

    private SpellInfo[] spellDefaultSpec = new SpellInfo[Enum.GetNames(typeof(SkillName)).Length];

    private void Awake()
    {
        Instance = this;
        InitSpellDefaultSpecs();
    }

    private void InitSpellDefaultSpecs()
    {
        // Wizard_Male
        // ���� ����
        SetSpellDefaultSpec(SpellType.Fire, 2.0f, 10.0f, 10.0f, 30, 1, SkillName.FireBallLv1, SpellState.Ready);
        SetSpellDefaultSpec(SpellType.Water, 2.0f, 10.0f, 10.0f, 30, 1, SkillName.WaterBallLv1, SpellState.Ready);
        SetSpellDefaultSpec(SpellType.Ice, 2.0f, 10.0f, 10.0f, 30, 1, SkillName.IceBallLv1, SpellState.Ready);
        // ��� ����
        SetSpellDefaultSpec(SpellType.Arcane, 10.0f, 2.2f, 0f, 30, 1, SkillName.MagicShieldLv1, SpellState.Ready);

        // Knight_Male
        // ���� ��ų
        SetSpellDefaultSpec(SpellType.Stone, 2.0f, 1.0f, 10.0f, 30, 1, SkillName.StoneSlashAttack1_Lv1, SpellState.Ready);
        SetSpellDefaultSpec(SpellType.Electric, 2.0f, 1.0f, 10.0f, 30, 1, SkillName.ElectricSlashAttack1_Lv1, SpellState.Ready);
        SetSpellDefaultSpec(SpellType.Electric, 2.0f, 1.0f, 10.0f, 30, 1, SkillName.ElectricSlashAttack2_Lv1, SpellState.Ready);
        //���
        SetSpellDefaultSpec(SpellType.Normal, 2.0f, 0f, 0f, 30, 1, SkillName.Dash_Lv1, SpellState.Ready);
    }

    public SpellInfo GetSpellDefaultSpec(SkillName spellName)
    {
        if (spellDefaultSpec.Length <= (int)spellName) return null;

        return spellDefaultSpec[(int)spellName];
    }

    /// <summary>
    /// Spell�� �⺻ ������ �������ִ� �޼ҵ� �Դϴ�.
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