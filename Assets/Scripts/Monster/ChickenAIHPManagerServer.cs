using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ChickenAIHPManagerServer : NetworkBehaviour
{
    public ChickenAIServer chickenAIServer;
    public ChickenAIClient chickenAIClient;
    public ChickenAIHPManagerClient chickenAIHPManagerClient;

    public sbyte hp;
    private sbyte maxHp;

    public void Start()
    {
        InitHP();
    }

    private void InitHP()
    {
        hp = 3;
        maxHp = 3;
    }

    public void TakingDamage(sbyte damage)
    {
        sbyte newPlayerHP = hp;

        // HP보다 Damage가 클 경우 GameOver
        if (newPlayerHP <= damage)
        {
            // HP 0
            newPlayerHP = 0;

            // 게임오버 처리, GameOver 애니메이션 실행
            chickenAIServer.GameOver();
        }
        else
        {
            // HP 감소 계산
            newPlayerHP -= (sbyte)damage;
        }

        // 변경된 HP값 서버에 저장
        hp = newPlayerHP;

        // 피격 대미지 숫자 표시 실행. ClientRPC
        chickenAIClient.ShowDamageTextPopupClientRPC(damage);
        // HP바 UI 업데이트 ClientRPC       
        chickenAIHPManagerClient.SetHPClientRPC(hp, maxHp);
        // 쉐이더 피격 이펙트 실행 ClientRPC
        chickenAIClient.ActivateHitByAttackEffectClientRPC();
    }

    /// <summary>
    /// 공격한 플레이어의 화면 쉐이크 기능이 추가된 메서드.
    /// </summary>
    /// <param name="damage"></param>
    /// <param name="clientObjectWhoAttacked"></param>
    public void TakingDamageWithCameraShake(sbyte damage, GameObject clientObjectWhoAttacked)
    {
        Debug.Log($"{gameObject}.TakingDamageWithCameraShake! damage:{damage}, clientWhoAttacked:{clientObjectWhoAttacked}");

        TakingDamage(damage);

        // 공격자가 Player라면 카메라 쉐이크 
        if (clientObjectWhoAttacked.TryGetComponent<PlayerClient>(out PlayerClient playerClient))
        {
            playerClient.ActivateHitCameraShakeClientRPC();
        }
    }

    // 파이어볼 도트 대미지를 받는 Coroutine
    public IEnumerator TakeDamageOverTime(sbyte damagePerSecond, float duration)
    {
        float elapsed = 0;
        while (elapsed < duration)
        {
            // 1초 대기
            yield return new WaitForSeconds(1);

            TakingDamage(damagePerSecond);

            elapsed += 1;
        }
    }
}
