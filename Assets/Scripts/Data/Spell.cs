using UnityEngine;

public class Spell
{
    /// <summary>
    /// ������ ������ �´� �̸��� �������ִ� �޼ҵ� �Դϴ�.
    /// </summary>
    /// <param name="level"></param>
    /// <param name="spellType"></param>
    public static SkillName GetSpellName(byte level, SpellType spellType)
    {
        SkillName spellName = SkillName.FireSpellStart;

        switch (spellType)
        {
            case SpellType.Normal:
                // ��� ������ ���� ���� ����. �ϴ� �׽�Ʈ��.
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