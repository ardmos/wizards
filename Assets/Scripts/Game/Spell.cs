using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 마법의 공통적인 기능들을 관리하는 클래스
/// </summary>
public class Spell : MonoBehaviour
{
    public enum SpellType
    {
        Fire,
        Water,
        Ice,
        Lightning,
        Arcane
    }
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
