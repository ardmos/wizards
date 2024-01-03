using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 
/// 3레벨 파이어볼 스크립트입니다.
/// 
/// !!! 현재 기능
/// 1. 상세 능력치 설정
/// 2. 게임 월드에서의 Spell Object의 이동 처리
/// 3. CollisionEnter 충돌 처리
/// </summary>
public class FireBallLv3 : FireSpell
{
    /// <summary>
    /// 1. 상세 능력치 설정
    /// </summary>
    public override void InitSpellInfoDetail()
    {
        spellInfo = new SpellInfo()
        {
            spellType = SpellType.Fire,
            coolTime = 5.0f,
            // #Test Code 10/16 : 
            //restTime = 0.0f,
            lifeTime = 10.0f,
            moveSpeed = 10.0f,
            price = 50,
            level = 3,
            spellName = SpellName.FireBallLv1,
            spellState = SpellState.Ready
        };
    }
}
