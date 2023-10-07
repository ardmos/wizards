using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 마법의 공통적인 기능들을 관리하는 클래스
/// </summary>
public abstract class Spell : MonoBehaviour
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

    /// <summary>
    /// 각 스펠의 디테일값(spellData)은 스펠 본인들이 상속받아서 정의
    /// </summary>
    public abstract void InitSpellDataDetail();
}
