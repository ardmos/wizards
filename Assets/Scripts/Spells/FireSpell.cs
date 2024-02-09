using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// ���� �ֹ����� �������� ����� �����ϴ� Ŭ���� 
/// !!! ���� ���
/// 1. �Ӽ��� �浹 ���
/// </summary>
public abstract class FireSpell : AttackSpell
{
    /// <summary>
    /// �ҼӼ� ������ �ٸ� ������ �浹 ������� �˷��ִ� �޼ҵ�.
    /// </summary>
    public override SpellInfo CollisionHandling(SpellInfo thisSpell, SpellInfo opponentsSpell)
    {
        SpellInfo result = new SpellInfo(thisSpell);

        // Lvl ��
        int resultLevel = thisSpell.level - opponentsSpell.level;

        // resultLevel ���� 0���� ���ų� ������ �� ����� �ʿ� ����. 
        //      0�̸� ���Ŵϱ� ���� �ʿ� ����
        //      ���̳ʽ��� ���Ŵϱ� ���� �ʿ� ����.
        //      �� �޼ҵ带 ȣ���ϴ� �� ���� ��ũ��Ʈ������ resultLevel���� ���� �ļ� ���� ������Ʈ �������θ� �Ǵ��ϸ� ��. 
        if (resultLevel <= 0)
        {
            result.level = 0;
            return result;
        }

        // resultLevel���� 0���� ū ���� ���� �̱� ���. ��� ���� �Ӽ��� ����� �� ���� ������ ������� ��ȯ�Ѵ�.
        result.level = (byte)resultLevel;

        switch (opponentsSpell.spellType)
        {
            case SpellType.Fire:
            case SpellType.Water:
            case SpellType.Ice:
            case SpellType.Arcane:
            default:
                result.spellType = SpellType.Fire;         
                break;
            case SpellType.Lightning:
                result.spellType = SpellType.Arcane;
                break;
        }

        result.spellName = Spell.GetSpellName(result.level, result.spellType);
        return result;
    }
}
