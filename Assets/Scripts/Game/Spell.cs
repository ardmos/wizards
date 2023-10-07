using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// ������ �������� ��ɵ��� �����ϴ� Ŭ����
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
    /// �� ������ �����ϰ�(spellData)�� ���� ���ε��� ��ӹ޾Ƽ� ����
    /// </summary>
    public abstract void InitSpellDataDetail();
}
