using UnityEngine;

public class Spell
{
    /// <summary>
    /// ������ ������ �´� �̸��� �������ִ� �޼ҵ� �Դϴ�.   �޼���� �ٲ� �ʿ� �־��. �浹���ó���ϴ°ǵ� �������� ������
    /// �� �Ӽ����� Start��� enum�� ��ҵ��� ���ݱ� ������ -1 ���ְ�����
    /// </summary>
    /// <param name="level"></param>
    /// <param name="spellType"></param>
    public static SkillName GetSpellName(byte level, SpellType spellType)
    {
        SkillName spellName = SkillName.FireBallLv1;

        switch (spellType)
        {
            case SpellType.Normal:
                // ��� ������ ���� ���� ����. �ϴ� �׽�Ʈ��.
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