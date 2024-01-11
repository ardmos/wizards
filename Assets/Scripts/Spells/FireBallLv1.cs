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
        spellInfo = SpellSpecifications.Instance.GetSpellSpec(SpellName.FireBallLv1);
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
