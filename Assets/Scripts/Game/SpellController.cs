using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 플레이어 캐릭터 오브젝트에 붙이는 스크립트
/// !!! 현재 기능
/// 1. 마법 보유 현황 관리
/// 2. 스킬 발동 
/// </summary>
public class SpellController : MonoBehaviour
{
    [SerializeField]
    private GameObject currentSpell1Prefab;
    [SerializeField]
    private GameObject currentSpell2Prefab;
    [SerializeField]
    private GameObject currentSpell3Prefab;
    [SerializeField]
    private Player player;
    [SerializeField]
    private Transform muzzle;
    [SerializeField]
    private float restTimeCurrentSpell_1 = 0f;
    [SerializeField]
    private float restTimeCurrentSpell_2 = 0f;
    [SerializeField]
    private float restTimeCurrentSpell_3 = 0f;


    // Start is called before the first frame update
    void Start()
    {
        currentSpell1Prefab.GetComponent<Spell>().InitSpellInfoDetail();
        currentSpell2Prefab.GetComponent<Spell>().InitSpellInfoDetail();
        currentSpell3Prefab.GetComponent<Spell>().InitSpellInfoDetail();
    }

    // Update is called once per frame
    void Update()
    {
        CheckCastSpellSlot1();
        CheckCastSpellSlot2();
        CheckCastSpellSlot3();
    }

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
            currentSpell1Prefab.GetComponent<Spell>().CastSpell(currentSpell1Prefab, muzzle);
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
            currentSpell2Prefab.GetComponent<Spell>().CastSpell(currentSpell2Prefab, muzzle);
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
            currentSpell3Prefab.GetComponent<Spell>().CastSpell(currentSpell3Prefab, muzzle);
            currentSpell3Prefab.GetComponent<Spell>().spellInfo.castAble = false;
        }
    }

    // #Test Code 10/16 : Switch current spell


}
