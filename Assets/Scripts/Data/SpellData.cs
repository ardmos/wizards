using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 마법의 공통적인 속성들을 관리하는 클래스
/// </summary>
public class SpellData : MonoBehaviour
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

    public SpellData()
    {
        coolTime = 0;
        restTime = 0;
        lifeTime = 0;
        moveSpeed = 0;
        damage = 0;
        price = 0;
        level = 0;
        spellName = "";
        castAble = true;
        spellObjectPref = null;
    }
}
