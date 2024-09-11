using UnityEngine;

public class Spell
{
    /// <summary>
    /// ������ ������ �´� �̸��� �������ִ� �޼ҵ� �Դϴ�.   �޼���� �ٲ� �ʿ� �־��. �浹���ó���ϴ°ǵ� �������� ������
    /// �� �Ӽ����� Start��� enum�� ��ҵ��� ���ݱ� ������ -1 ���ְ�����
    /// </summary>
    /// <param name="level"></param>
    /// <param name="spellType"></param>
    public static SpellName GetSpellName(byte level, SpellType spellType)
    {
        SpellName spellName = SpellName.FireBallLv1;

        switch (spellType)
        {
            case SpellType.Normal:
                // ��� ������ ���� ���� ����. �ϴ� �׽�Ʈ��.
                spellName = SpellName.FireBallLv1 + level - 1;
                break;
            case SpellType.Fire:
                spellName = SpellName.FireBallLv1 + level - 1;
                break;

            case SpellType.Water:
                spellName = SpellName.WaterBallLv1 + level - 1;
                break;

            case SpellType.Ice:
                spellName = SpellName.IceBallLv1 + level - 1;
                break;

            default: break;
        }

        return spellName;
    }
}