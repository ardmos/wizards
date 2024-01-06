using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
/// <summary>
/// 
/// 1레벨 파이어볼 스크립트입니다.
/// 
/// !!! 현재 기능
/// 1. 상세 능력치 설정
/// 2. CollisionEnter 충돌 처리
/// 3. 마법 시전
/// </summary>
public class FireBallLv1 : FireSpell
{

    /// <summary>
    /// 1. 상세 능력치 설정
    /// </summary>
    public override void InitSpellInfoDetail()
    {
        //Debug.Log("InitSpellInfoDetail() FireBall Lv1");
        spellInfo = new SpellInfo()
        {
            spellType = SpellType.Fire,
            coolTime = 2.0f,
            lifeTime = 10.0f,
            //moveSpeed = 10.0f, //테스트용
            moveSpeed = 0.0f,
            price = 30,
            level = 1,
            spellName = SpellName.FireBallLv1,
            spellState = SpellState.Ready,
        };
    }

    /// <summary>
    /// 2. CollisionEnter 충돌 처리 (서버 권한 방식)
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter(Collision collision)
    {
        SpellHit(collision);
    }

    /// <summary>
    /// 3. 마법 시전
    /// </summary>
    public override void CastSpell(SpellInfo spellInfo, NetworkObject player)
    {
        base.CastSpell(spellInfo, player);
    }
}
