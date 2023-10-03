using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct SpellData
{
    public enum SpellType
    {
        Fire,
        Water,
        Ice,
        Lightning,
        Arcane
    }

    public float coolTime;
    public float lifeTime;
    public float moveSpeed;
    public int damage;
    public int price;
    public string spellName;
    public GameObject spellObject;
}
