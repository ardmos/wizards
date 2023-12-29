using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// ���� �ֹ��� �������� ����� �����ϴ� Ŭ���� 
/// !!! ���� ���
/// 1. �Ӽ��� �浹 ���
/// </summary>
public abstract class IceSpell : Spell
{
    // 1. �Ӽ��� �浹 ���
    public override SpellInfo CollisionHandling(SpellInfo thisSpell, SpellInfo opponentsSpell)
    {
        SpellInfo result = new SpellInfo();

        // Lvl ��
        int resultLevel = thisSpell.level - opponentsSpell.level;
        result.level = resultLevel;
        // resultLevel ���� 0���� ���ų� ������ �� ����� �ʿ� ����. 
        //      0�̸� ���Ŵϱ� ���� �ʿ� ����
        //      ���̳ʽ��� ���Ŵϱ� ���� �ʿ� ����.
        //      �� �޼ҵ带 ȣ���ϴ� �� ���� ��ũ��Ʈ������ resultLevel���� ���� �ļ� ���� ������Ʈ �������θ� �Ǵ��ϸ� ��. 
        if (resultLevel <= 0)
        {
            return result;
        }
        // resultLevel���� 0���� ū ���� ���� �̱� ���. ��� ���� �Ӽ��� ����� �� ���� ������ ������� ��ȯ�Ѵ�.
        switch (opponentsSpell.spellType)
        {
            case SpellType.Fire:
                result.spellType = SpellType.Water;
                break;
            case SpellType.Water:
            case SpellType.Ice:
            case SpellType.Arcane:
            case SpellType.Lightning:
            default:
                result.spellType = SpellType.Ice;
                break;
        }

        return result;
    }
}
