
using UnityEngine;
/// <summary>
/// 각 스펠들의 상세 스펙을 담고있는 스크립트입니다.
/// 요청받으면 스펙을 제공합니다.
/// Game씬의 SpellSpecifications 오브젝트의 컴포넌트 입니다. Game씬과 라이프사이클을 함께합니다.
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
        SetSpellDefaultSpec(SpellType.Fire, 2.0f, 10.0f, 10.0f, 30, 1, SpellName.FireBallLv1, SpellState.Ready);
        SetSpellDefaultSpec(SpellType.Water, 2.0f, 10.0f, 10.0f, 30, 1, SpellName.WaterBallLv1, SpellState.Ready);
        SetSpellDefaultSpec(SpellType.Ice, 2.0f, 10.0f, 10.0f, 30, 1, SpellName.IceBallLv1, SpellState.Ready);
        SetSpellDefaultSpec(SpellType.Normal, 2.0f, 1.0f, 10.0f, 30, 1, SpellName.SlashLv1, SpellState.Ready);
    }

    public SpellInfo GetSpellDefaultSpec(SpellName spellName)
    {
        return spellDefaultSpec[(int)spellName];
    }

    /// <summary>
    /// Spell의 기본 스펙을 설정해주는 메소드 입니다.
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