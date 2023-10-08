using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// ���� �ֹ����� �������� ����� �����ϴ� Ŭ���� 
/// !!! ���� ���
/// 1. �� �Ӽ����� �� �������� ���ε鿡�� �����ϵ��� ��
/// 2. �Ӽ��� �浹 ���
/// </summary>
public abstract class FireSpell : Spell
{
    /// <summary>
    /// �� �Ӽ����� �� �������� ���ε鿡�� ����
    /// </summary>
    public abstract void InitSpellInfoDetail();

    // �Ӽ��� �浹 ��� ������ ����. ���⼱ Lvl�� Type�� ��ȯ�ϰ� ������ �Ӽ����� �� ��ũ��Ʈ�� ȣ���� �� �������翡�� �Է��� ����Ѵ�. (������ ������ ���ε� ������ �״�� ����ϰԵ�)
    public override SpellLvlType CollisionHandling(SpellLvlType thisSpell, SpellLvlType opponentsSpell)
    {
        SpellLvlType result = new SpellLvlType();

        // 1. Lvl ��
        int resultLevel = thisSpell.level - opponentsSpell.level;
        result.level = resultLevel;
        // 2. resultLevel ���� 0���� ���ų� ������ �� ����� �ʿ� ����. 
        //      0�̸� ���Ŵϱ� ���� �ʿ� ����
        //      ���̳ʽ��� ���Ŵϱ� ���� �ʿ� ����.
        //      �� �޼ҵ带 ȣ���ϴ� �� ���� ��ũ��Ʈ������ resultLevel���� ���� �ļ� ���� ������Ʈ �������θ� �Ǵ��ϸ� ��. 
        if (resultLevel <= 0)
        {           
            return result;
        }

        // 3. resultLevel���� 0���� ū ���� ���� �̱� ���. ��� ���� �Ӽ��� ����� �� ���� ������ ������� ��ȯ�Ѵ�.
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

        return result;
    }
}
