using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChickenAIHPManagerServer : MonoBehaviour
{
    public ChickenAIServer chickenAIServer;

    public sbyte hp = 3;

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
