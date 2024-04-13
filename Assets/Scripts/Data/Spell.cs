using UnityEngine;

public class Spell
{
    /// <summary>
    /// 스펠의 레벨에 맞는 이름을 설정해주는 메소드 입니다.   메서드명 바꿀 필요 있어보임. 충돌결과처리하는건데 직관성이 떨어짐
    /// 각 속성별로 Start라는 enum의 요소들을 없앴기 때문에 -1 해주고있음
    /// </summary>
    /// <param name="level"></param>
    /// <param name="spellType"></param>
    public static SkillName GetSpellName(byte level, SpellType spellType)
    {
        SkillName spellName = SkillName.FireBallLv1;

        switch (spellType)
        {
            case SpellType.Normal:
                // 기사 마법은 추후 수정 예정. 일단 테스트용.
                spellName = SkillName.FireBallLv1 + level - 1;
                break;
            case SpellType.Fire:
                spellName = SkillName.FireBallLv1 + level - 1;
                break;

            case SpellType.Water:
                spellName = SkillName.WaterBallLv1 + level - 1;
                break;

            case SpellType.Ice:
                spellName = SkillName.IceBallLv1 + level - 1;
                break;

            default: break;
        }

        return spellName;
    }
}