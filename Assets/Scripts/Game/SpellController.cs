using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// �÷��̾� ĳ���� ������Ʈ�� ���̴� ��ũ��Ʈ
/// ���� ���
///   1. ���� ���� ��Ȳ ����
///   2. ��ų �ߵ� 
/// </summary>
public class SpellController : MonoBehaviour
{
    [SerializeField] private GameObject currentSpell1Prefab, currentSpell2Prefab, currentSpell3Prefab;
    [SerializeField] private Player player;
    [SerializeField] private Transform muzzle;
    [SerializeField] private float restTimeCurrentSpell_1 = 0f;
    [SerializeField] private float restTimeCurrentSpell_2 = 0f;
    [SerializeField] private float restTimeCurrentSpell_3 = 0f;

    void Start()
    {
        currentSpell1Prefab.GetComponent<Spell>().InitSpellInfoDetail();
        currentSpell2Prefab.GetComponent<Spell>().InitSpellInfoDetail();
        currentSpell3Prefab.GetComponent<Spell>().InitSpellInfoDetail();
    }

    void Update()
    {
        CheckCastSpellSlot1();
        CheckCastSpellSlot2();
        CheckCastSpellSlot3();
    }

    #region ���� ������ ���� ����
    public void CheckCastSpellSlot1()
    {
        if (currentSpell1Prefab == null) return;
        if (!currentSpell1Prefab.GetComponent<Spell>().spellInfo.castAble)
        {
            restTimeCurrentSpell_1 += Time.deltaTime;
            if (restTimeCurrentSpell_1 >= currentSpell1Prefab.GetComponent<Spell>().spellInfo.coolTime)
            {
                currentSpell1Prefab.GetComponent<Spell>().spellInfo.castAble = true;
                restTimeCurrentSpell_1 = 0f;
            }
            return;
        }

        if (player.IsAttack1())
        {
            currentSpell1Prefab.GetComponent<Spell>().CastSpell(new Spell.SpellLvlType { level = currentSpell1Prefab.GetComponent<Spell>().spellInfo.level, spellType = currentSpell1Prefab.GetComponent<Spell>().spellInfo.spellType}, muzzle);  ;
            currentSpell1Prefab.GetComponent<Spell>().spellInfo.castAble = false;
        }
    }
    public void CheckCastSpellSlot2()
    {
        if (currentSpell2Prefab == null) return;
        if (!currentSpell2Prefab.GetComponent<Spell>().spellInfo.castAble)
        {
            restTimeCurrentSpell_2 += Time.deltaTime;
            if (restTimeCurrentSpell_2 >= currentSpell2Prefab.GetComponent<Spell>().spellInfo.coolTime)
            {
                currentSpell2Prefab.GetComponent<Spell>().spellInfo.castAble = true;
                restTimeCurrentSpell_2 = 0f;
            }
            return;
        }

        if (player.IsAttack2())
        {
            currentSpell2Prefab.GetComponent<Spell>().CastSpell(new Spell.SpellLvlType { level = currentSpell2Prefab.GetComponent<Spell>().spellInfo.level, spellType = currentSpell2Prefab.GetComponent<Spell>().spellInfo.spellType }, muzzle);
            currentSpell2Prefab.GetComponent<Spell>().spellInfo.castAble = false;
        }
    }
    public void CheckCastSpellSlot3()
    {
        if (currentSpell3Prefab == null) return;
        if (!currentSpell3Prefab.GetComponent<Spell>().spellInfo.castAble)
        {
            restTimeCurrentSpell_3 += Time.deltaTime;
            if (restTimeCurrentSpell_3 >= currentSpell3Prefab.GetComponent<Spell>().spellInfo.coolTime)
            {
                currentSpell3Prefab.GetComponent<Spell>().spellInfo.castAble = true;
                restTimeCurrentSpell_3 = 0f;
            }
            return;
        }

        if (player.IsAttack3())
        {
            currentSpell3Prefab.GetComponent<Spell>().CastSpell(new Spell.SpellLvlType { level = currentSpell3Prefab.GetComponent<Spell>().spellInfo.level, spellType = currentSpell3Prefab.GetComponent<Spell>().spellInfo.spellType }, muzzle);
            currentSpell3Prefab.GetComponent<Spell>().spellInfo.castAble = false;
        }
    }
    #endregion

    #region ���� ���� �̸� ���
    public string GetCurrentSpell1Name()
    {
        if (currentSpell1Prefab == null) return "";
        return currentSpell1Prefab.GetComponent<Spell>().spellInfo.spellName;
    }
    public string GetCurrentSpell2Name()
    {
        if (currentSpell2Prefab == null) return "";
        return currentSpell2Prefab.GetComponent<Spell>().spellInfo.spellName;
    }
    public string GetCurrentSpell3Name()
    {
        if (currentSpell3Prefab == null) return "";
        return currentSpell3Prefab.GetComponent<Spell>().spellInfo.spellName;
    }
    #endregion

    // ���� ���� ����
    public void SetCurrentSpell(GameObject spellObjectPrefab, int slotNumber)
    {
        switch (slotNumber)
        {
            case 1 : 
                currentSpell1Prefab = spellObjectPrefab; 
                currentSpell1Prefab.GetComponent<Spell>().InitSpellInfoDetail(); 
                break;
            case 2 : 
                currentSpell2Prefab = spellObjectPrefab;
                currentSpell2Prefab.GetComponent<Spell>().InitSpellInfoDetail();
                break;
            case 3 : 
                currentSpell3Prefab = spellObjectPrefab;
                currentSpell3Prefab.GetComponent<Spell>().InitSpellInfoDetail();
                break;
            default:
                break;
        }
    }
}
