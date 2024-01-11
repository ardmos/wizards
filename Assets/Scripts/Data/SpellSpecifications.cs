
using UnityEngine;
/// <summary>
/// 각 스펠들의 상세 스펙을 담고있는 스크립트입니다.
/// 요청받으면 스펙을 제공합니다.
/// Game씬의 SpellSpecifications 오브젝트의 컴포넌트 입니다. Game씬과 라이프사이클을 함께합니다.
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
    /// Spell의 스펙을 설정해주는 메소드 입니다.
    /// Spell 강화 스크롤을 먹었을 시에도 이 메소드를 이용해 스펠을 강화시켜줍니다.
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