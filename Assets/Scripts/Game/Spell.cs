using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static FireSpell;

/// <summary>
/// ������ �������� ��ɵ��� �����ϴ� Ŭ����
/// </summary>
public abstract class Spell : MonoBehaviour
{
    public struct SpellLvlType
    {
        public Spell.SpellType spellType;
        public int level;
    }

    public enum SpellType
    {
        Fire,
        Water,
        Ice,
        Lightning,
        Arcane
    }
    public class SpellInfo
    {
        public SpellType spellType;

        public float coolTime;
        public float restTime;
        public float lifeTime;
        public float moveSpeed;
        public int damage;
        public int price;
        public int level;
        public string spellName;
        public bool castAble;

        public GameObject spellObjectPref;
    }
    // �Ӽ��� �浹 ���. ���⼱ Lvl�� Type�� ��ȯ�ϰ� ������ �Ӽ����� �� �������翡�� �Է��� ����Ѵ�. (������ ������ ���ε� ������ �״�� ����ϰԵ�)
    public abstract SpellLvlType CollisionHandling(SpellLvlType thisSpell, SpellLvlType opponentsSpell);
}
