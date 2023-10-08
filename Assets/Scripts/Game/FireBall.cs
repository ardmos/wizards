using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 이곳은 파이어볼 스크립트입니다.
/// !!! 현재 기능
/// 1. Lvl에 따른 상세 능력치 설정
/// 2. 게임 월드에서의 Spell Object의 이동 처리
/// 3. CollisionEnter 충돌 처리
/// </summary>
public class FireBall : FireSpell
{
    SpellInfo fireBallLv1;
    SpellInfo fireBallLv2;
    SpellInfo fireBallLv3;

    // Start is called before the first frame update
    void Start()
    {
        InitSpellInfoDetail();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// 1. Lvl에 따른 상세 능력치 설정
    /// </summary>
    public override void InitSpellInfoDetail()
    {
        fireBallLv1 = new SpellInfo()
        {
            spellType = SpellType.Fire,
            coolTime = 5.0f,
            restTime = 0.0f,
            lifeTime = 10.0f,
            moveSpeed = 10.0f,
            price = 30,
            level = 1,
            spellName = "FireBall Lv.1",
            castAble = true
        };
        fireBallLv2 = new SpellInfo()
        {
            spellType = SpellType.Fire,
            coolTime = 5.0f,
            restTime = 0.0f,
            lifeTime = 10.0f,
            moveSpeed = 10.0f,
            price = 40,
            level = 2,
            spellName = "FireBall Lv.2",
            castAble = true
        };
        fireBallLv3 = new SpellInfo()
        {
            spellType = SpellType.Fire,
            coolTime = 5.0f,
            restTime = 0.0f,
            lifeTime = 10.0f,
            moveSpeed = 10.0f,
            price = 50,
            level = 3,
            spellName = "FireBall Lv.3",
            castAble = true
        };
    }
}
