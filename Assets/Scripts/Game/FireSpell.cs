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
    public struct CollisionCalcResult
    {
        Spell.SpellType spellType;
        int level;
    }

    /// <summary>
    /// �� �Ӽ����� �� �������� ���ε鿡�� ����
    /// </summary>
    public abstract void InitSpellDataDetail();

    // �Ӽ��� �浹 ��� ������ ����. ���⼱ Lvl�� Type�� ��ȯ�ϰ� ������ �Ӽ����� �� ��ũ��Ʈ�� ȣ���� �� �������翡�� �Է��� ����Ѵ�. (������ ������ ���ε� ������ �״�� ����ϰԵ�)
    public CollisionCalcResult CollisionHandling(Spell thisSpell, Spell opponentsSpell)
    {
        CollisionCalcResult result = new CollisionCalcResult();

        swit


        return result;
    }
}
