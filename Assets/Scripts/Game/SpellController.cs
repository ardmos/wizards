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
    private Player player;
    [SerializeField]
    private Transform muzzle;
    // #Test Code 10/16 : Cool-Time sys
    [SerializeField]
    private float restTimeCurrentSpell_1 = 0f;
    [SerializeField]
    private float restTimeCurrentSpell_2 = 0f;
    [SerializeField]
    private float restTimeCurrentSpell_3 = 0f;


    // Start is called before the first frame update
    void Start()
    {
        // #Test Code 10/16 : Cool-Time sys
        currentSpell1Prefab.GetComponent<Spell>().InitSpellInfoDetail();
        Debug.Log("Start");
    }

    // Update is called once per frame
    void Update()
    {
        // Test Code : casting spell button clicked. ** Cool-time system required.
        // #Test Code 10/16 : current1 fire
        CheckCastSpellSlot1();
    }

    // #Test Code 10/14 : casting spell button clicked
    public void CheckCastSpellSlot1()
    {
        // #Test Code 10/16 : Cool-Time sys
        //Debug.Log("RestTime : " + restTimeCurrentSpell_1);

        if (player.IsAttack1() && currentSpell1Prefab.GetComponent<Spell>().spellInfo.castAble)
        {
            currentSpell1Prefab.GetComponent<Spell>().CastSpell(currentSpell1Prefab, muzzle);
            // muzzle ->> crash. 
            //currentSpell1Prefab.GetComponent<Spell>().MuzzleVFX(currentSpell1Prefab, muzzle);

            currentSpell1Prefab.GetComponent<Spell>().spellInfo.castAble = false;
        }

        if (currentSpell1Prefab.GetComponent<Spell>().spellInfo.castAble == false)
        {
            restTimeCurrentSpell_1 += Time.deltaTime;
            if (restTimeCurrentSpell_1 >= currentSpell1Prefab.GetComponent<Spell>().spellInfo.coolTime)
            {
                currentSpell1Prefab.GetComponent<Spell>().spellInfo.castAble = true;
                restTimeCurrentSpell_1 = 0f;
            }
        }
    }

    // #Test Code 10/16 : Switch current spell


}
