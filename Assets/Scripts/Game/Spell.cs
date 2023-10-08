using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static FireSpell;

/// <summary>
/// 마법의 공통적인 기능들을 관리하는 클래스
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
    // 속성별 충돌 계산. 여기선 Lvl와 Type만 반환하고 나머지 속성값은 각 마법스펠에서 입력해 사용한다. (보통은 기존의 본인들 스탯을 그대로 사용하게됨)
    public abstract SpellLvlType CollisionHandling(SpellLvlType thisSpell, SpellLvlType opponentsSpell);
}
