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
    // 현재 사용하는 파이어볼 VFX를 자연스럽게 하기위한 부분
    public override void OnNetworkSpawn()
    {
        trails[0].SetActive(false);
    }

    public override void Shoot(Vector3 force, ForceMode forceMode)
    {
        base.Shoot(force, forceMode);

        // 꼬리연기 효과 활성화
        trails[0].SetActive(true);
    }
}
