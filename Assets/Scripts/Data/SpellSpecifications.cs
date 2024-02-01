
using UnityEngine;
/// <summary>
/// �� ������� �� ������ ����ִ� ��ũ��Ʈ�Դϴ�.
/// ��û������ ������ �����մϴ�.
/// Game���� SpellSpecifications ������Ʈ�� ������Ʈ �Դϴ�. Game���� ����������Ŭ�� �Բ��մϴ�.
/// </summary>
public class SpellSpecifications : MonoBehaviour
{
    public static SpellSpecifications Instance;

    private SpellInfo[] spellDefaultSpec = new SpellInfo[(int)SpellName.Max];

    private void Awake()
    {
        Instance = this;
        InitSpellDefaultSpecs();
    }

    private void InitSpellDefaultSpecs()
    {
        // ���� ����
        SetSpellDefaultSpec(SpellType.Fire, 2.0f, 10.0f, 10.0f, 30, 1, SpellName.FireBallLv1, SpellState.Ready);
        SetSpellDefaultSpec(SpellType.Water, 2.0f, 10.0f, 10.0f, 30, 1, SpellName.WaterBallLv1, SpellState.Ready);
        SetSpellDefaultSpec(SpellType.Ice, 2.0f, 10.0f, 10.0f, 30, 1, SpellName.IceBallLv1, SpellState.Ready);
        // ��� ����
        SetSpellDefaultSpec(SpellType.Arcane, 10.0f, 2.2f, 0f, 30, 1, SpellName.MagicShieldLv1, SpellState.Ready);
        // ���� ��ų
        SetSpellDefaultSpec(SpellType.Stone, 2.0f, 1.0f, 10.0f, 30, 1, SpellName.StoneSlashLv1, SpellState.Ready);        
    }

    public SpellInfo GetSpellDefaultSpec(SpellName spellName)
    {
        return spellDefaultSpec[(int)spellName];
    }

    /// <summary>
    /// Spell�� �⺻ ������ �������ִ� �޼ҵ� �Դϴ�.
    /// </summary>
    /// <returns></returns>
    private void SetSpellDefaultSpec(SpellType spellType, float coolTime, float lifeTime, float moveSpeed, int price, sbyte level, SpellName spellName, SpellState spellState)
    {
        SpellInfo spellInfo;
        spellInfo = new SpellInfo()
        {
            spellType = spellType,
            coolTime = coolTime,
            lifeTime = lifeTime,
            moveSpeed = moveSpeed,
            price = price,
            level = level,
            spellName = spellName,
            spellState = spellState,
        };

        spellDefaultSpec[(int)spellName] = spellInfo;
    }
}