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
    private Spell currentSpell;
    public Transform muzzle;

    // Start is called before the first frame update
    void Start()
    {
        currentSpell.CastSpell(muzzle);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
