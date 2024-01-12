using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// ���� �ֹ��� �������� ����� �����ϴ� Ŭ���� 
/// ���� ���
///   1. �Ӽ��� �浹 ���
/// </summary>
public abstract class WaterSpell : Spell
{
    // 1. �Ӽ��� �浹 ���
    public override SpellInfo CollisionHandling(SpellInfo thisSpell, SpellInfo opponentsSpell)
    {
        SpellInfo result = thisSpell;

        // Lvl ��
        sbyte resultLevel = (sbyte)(thisSpell.level - opponentsSpell.level);
        result.level = resultLevel;
        // resultLevel ���� 0���� ���ų� ������ �� ����� �ʿ� ����. (��� �ֹ����� ������ ���� ��츸 �Ӽ� ����� �ϸ� �ȴ�.) 
        // �� �޼ҵ带 ȣ���ϴ� �� ���� ��ũ��Ʈ������ resultLevel���� ���� �ļ� ���� ������Ʈ �������θ� �Ǵ��ϸ� ��. 
        if (resultLevel <= 0)
        {
            return result;
        }
        // resultLevel���� 0���� ū ���� ���� �ֹ� ������ ���� ���. ��� ���� �Ӽ��� ����� �� ���� ������ ��������� ��ȯ�Ѵ�.
        switch (opponentsSpell.spellType)
        {
            case SpellType.Fire:
            case SpellType.Water:
            case SpellType.Ice:
            case SpellType.Arcane:
            case SpellType.Lightning:
            default:
                result.spellType = SpellType.Water;
                break;
        }

        result.SetSpellName(result.level, result.spellType);
        return result;
    }
}
