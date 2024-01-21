using UnityEngine;
/// <summary>
/// 
/// 1레벨 파이어볼 스크립트입니다.
/// 
/// !!! 현재 기능
/// 1. 상세 능력치 설정
/// </summary>
public class FireBallLv1 : FireSpell
{

    /// <summary>
    /// 1. 상세 능력치 설정(마법 사용시에 Server에서 부여해주는 능력치 입니다.)
    /// </summary>
    public override void InitSpellInfoDetail(SpellInfo spellInfoFromServer)
    {
        //Debug.Log("InitSpellInfoDetail() FireBall Lv1");
        spellInfo = spellInfoFromServer;
    }

    // 현재 사용하는 파이어볼 VFX를 자연스럽게 하기위한 부분
    public override void OnNetworkSpawn()
    {
        trails[0].SetActive(false);
    }

    public override void Shoot(Vector3 force, ForceMode forceMode)
    {
        base.Shoot(force, forceMode);
        trails[0].SetActive(true);
        Debug.Log($"Fireball Lv1 Shoot");

    }
}
