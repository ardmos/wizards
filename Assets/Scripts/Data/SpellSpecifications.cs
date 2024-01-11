
using UnityEngine;
/// <summary>
/// �� ������� �� ������ ����ִ� ��ũ��Ʈ�Դϴ�.
/// ��û������ ������ �����մϴ�.
/// Game���� SpellSpecifications ������Ʈ�� ������Ʈ �Դϴ�. Game���� ����������Ŭ�� �Բ��մϴ�.
/// </summary>
public class SpellSpecifications : MonoBehaviour
{
    public static SpellSpecifications Instance;

    public SpellInfo[] SpellInfoArray = new SpellInfo[(int)SpellName.Max];

    private void Awake()
    {
        Instance = this;
        InitSpellSpecs();
    }

    private void InitSpellSpecs()
    {
        SetSpellSpec(SpellType.Fire, 2.0f, 10.0f, 10.0f, 30, 1, SpellName.FireBallLv1, SpellState.Ready);
        SetSpellSpec(SpellType.Water, 2.0f, 10.0f, 10.0f, 30, 1, SpellName.WaterBallLv1, SpellState.Ready);
        SetSpellSpec(SpellType.Ice, 2.0f, 10.0f, 10.0f, 30, 1, SpellName.IceBallLv1, SpellState.Ready);
        SetSpellSpec(SpellType.Normal, 2.0f, 1.0f, 10.0f, 30, 1, SpellName.SlashLv1, SpellState.Ready);
    }

    public SpellInfo GetSpellSpec(SpellName spellName)
    {
        return SpellInfoArray[(int)spellName];
    }

    /// <summary>
    /// Spell�� ������ �������ִ� �޼ҵ� �Դϴ�.
    /// Spell ��ȭ ��ũ���� �Ծ��� �ÿ��� �� �޼ҵ带 �̿��� ������ ��ȭ�����ݴϴ�.
    /// </summary>
    /// <param name="spellType"></param>
    /// <param name="coolTime"></param>
    /// <param name="lifeTime"></param>
    /// <param name="moveSpeed"></param>
    /// <param name="price"></param>
    /// <param name="level"></param>
    /// <param name="spellName"></param>
    /// <param name="spellState"></param>
    /// <returns></returns>
    public void SetSpellSpec(SpellType spellType, float coolTime, float lifeTime, float moveSpeed, int price, int level, SpellName spellName, SpellState spellState)
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
            spellState = spellState
        };

        SpellInfoArray[(int)spellName] = spellInfo;
    }
}