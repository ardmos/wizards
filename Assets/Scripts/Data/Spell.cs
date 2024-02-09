using UnityEngine;

public class Spell
{
    /// <summary>
    /// 스펠의 레벨에 맞는 이름을 설정해주는 메소드 입니다.
    /// </summary>
    /// <param name="level"></param>
    /// <param name="spellType"></param>
    public static SpellName GetSpellName(byte level, SpellType spellType)
    {
        SpellName spellName = SpellName.FireSpellStart;

        switch (spellType)
        {
            case SpellType.Normal:
                // 기사 마법은 추후 수정 예정. 일단 테스트용.
                spellName = SpellName.SlashSpellStart + level;
                break;
            case SpellType.Fire:
                spellName = SpellName.FireSpellStart + level;
                break;

            case SpellType.Water:
                spellName = (SpellName)((int)SpellName.WaterSpellStart + (int)level);
                Debug.Log($"!!! result spell name:{spellName} !!! level:{level}");
                break;

            case SpellType.Ice:
                spellName = SpellName.IceSpellStart + level;
                break;

            default: break;
        }

        return spellName;
    }
}