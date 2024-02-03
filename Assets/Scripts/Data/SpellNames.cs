using UnityEngine;

public enum SpellName
{
    FireSpellStart,
    FireBallLv1,
    FireSpellEnd,
    WaterSpellStart,
    WaterBallLv1,
    WaterSpellEnd,
    IceSpellStart,
    IceBallLv1,
    IceSpellEnd,
    SlashSpellStart,
    StoneSlashLv1,
    SlashSpellEnd,
    DefenceSpellStart,
    MagicShieldLv1,
    DefenceSpellEnd,
    Max
}

public class SpellNames
{
    /// <summary>
    /// ������ ������ �´� �̸��� �������ִ� �޼ҵ� �Դϴ�.
    /// </summary>
    /// <param name="level"></param>
    /// <param name="spellType"></param>
    public static SpellName GetSpellName(byte level, SpellType spellType)
    {
        SpellName spellName = SpellName.FireSpellStart;

        switch (spellType)
        {
            case SpellType.Normal:
                // ��� ������ ���� ���� ����. �ϴ� �׽�Ʈ��.
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