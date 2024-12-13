using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// ���� �ֹ��� �������� ����� �����ϴ� Ŭ���� 
/// ���� ���
///   1. �Ӽ��� �浹 ���
/// </summary>
public abstract class WaterSpell : AttackSpell
{
    // 1. �Ӽ��� �浹 ���
    public override SpellInfo CollisionHandling(SpellInfo thisSpell, SpellInfo opponentsSpell)
    {
        SpellInfo result = new SpellInfo(thisSpell);

        // Lvl ��
        int resultLevel = thisSpell.damage - opponentsSpell.damage;

        // resultLevel ���� 0���� ���ų� ������ �� ����� �ʿ� ����. (��� �ֹ����� ������ ���� ��츸 �Ӽ� ����� �ϸ� �ȴ�.) 
        // �� �޼ҵ带 ȣ���ϴ� �� ���� ��ũ��Ʈ������ resultLevel���� ���� �ļ� ���� ������Ʈ �������θ� �Ǵ��ϸ� ��. 
        if (resultLevel <= 0)
        {
            result.damage = 0;
            return result;
        }

        // resultLevel���� 0���� ū ���� ���� �ֹ� ������ ���� ���. ��� ���� �Ӽ��� ����� �� ���� ������ ��������� ��ȯ�Ѵ�.
        result.damage = (byte)resultLevel;

        switch (opponentsSpell.spellType)
        {
            case SpellType.Fire:
            case SpellType.Water:
            case SpellType.Ice:
            case SpellType.Arcane:
            case SpellType.Electric:
            default:
                result.spellType = SpellType.Water;
                break;
        }

        result.spellName = Spell.GetSpellName(result.damage, result.spellType);
        return result;
    }
}
