using UnityEngine;

public class Spell
{
    /// <summary>
    /// 스펠의 레벨에 맞는 이름을 설정해주는 메소드 입니다.
    /// </summary>
    /// <param name="level"></param>
    /// <param name="spellType"></param>
    public static SkillName GetSpellName(byte level, SpellType spellType)
    {
        SkillName spellName = SkillName.FireSpellStart;

        switch (spellType)
        {
            case SpellType.Normal:
                // 기사 마법은 추후 수정 예정. 일단 테스트용.
                spellName = SkillName.SlashSpellStart + level;
                break;
            case SpellType.Fire:
                spellName = SkillName.FireSpellStart + level;
                break;

            case SpellType.Water:
                spellName = (SkillName)((int)SkillName.WaterSpellStart + (int)level);
                Debug.Log($"!!! result spell name:{spellName} !!! level:{level}");
                break;

            case SpellType.Ice:
                spellName = SkillName.IceSpellStart + level;
                break;

            default: break;
        }

        return spellName;
    }
}