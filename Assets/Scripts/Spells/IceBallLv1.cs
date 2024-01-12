using UnityEngine;
/// <summary>
/// 
/// 1레벨 파이어볼 스크립트입니다.
/// 
/// !!! 현재 기능
/// 1. 상세 능력치 설정
/// 2. CollisionEnter 충돌 처리
/// </summary>
public class IceBallLv1 : IceSpell
{
    /// <summary>
    /// 1. 상세 능력치 설정(마법 사용시에 Server에서 부여해주는 능력치 입니다.)
    /// </summary>
    public override void InitSpellInfoDetail(SpellInfo spellInfoFromServer) 
    { 
        spellInfo = spellInfoFromServer; 
    }

    /// <summary>
    /// 2. CollisionEnter 충돌 처리
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter(Collision collision)
    {
        SpellManager.Instance.SpellHitOnServer(collision, this);
    }
}
