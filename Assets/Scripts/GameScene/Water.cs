using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Water : NetworkBehaviour
{
    [SerializeField] private sbyte damageValue = 1;
    [SerializeField] private float intervalTime = 1f;

    private List<GameObject> playersInWater = new List<GameObject>(); // 충돌한 플레이어를 저장할 리스트

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return;
        if (!collision.collider.CompareTag("Player") && !collision.collider.CompareTag("AI")) return;

        // 충돌한 플레이어를 리스트에 추가
        playersInWater.Add(collision.gameObject);

        // 최초로 충돌한 플레이어인 경우에만 InvokeRepeating 메서드 호출
        if (playersInWater.Count == 1)
        {
            InvokeRepeating(nameof(DealDamage), 0f, intervalTime);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (!IsServer) return;
        if (!collision.collider.CompareTag("Player") && !collision.collider.CompareTag("AI")) return;

        // 충돌을 끝낸 플레이어를 리스트에서 제거
        playersInWater.Remove(collision.gameObject);

        // 더 이상 플레이어가 충돌 중이지 않다면 InvokeRepeating 메서드 중지
        if (playersInWater.Count == 0)
        {
            CancelInvoke(nameof(DealDamage));
        }
    }

    private void DealDamage()
    {
        // 각 플레이어에게 대미지 주기. 공격자는 플레이어 자신으로 처리
        foreach (var player in playersInWater)
        {
            if(player == null &&  player.gameObject == null) continue;

            if (player.TryGetComponent<PlayerHPManagerServer>(out PlayerHPManagerServer playerHPManagerServer))
            {
                playerHPManagerServer.TakingDamage(damageValue, player.GetComponent<PlayerClient>().OwnerClientId);
            }
            if (player.TryGetComponent<WizardRukeAIHPManagerServer>(out WizardRukeAIHPManagerServer wizardRukeAIHPManagerServer))
            {
                wizardRukeAIHPManagerServer.TakingDamage(damageValue, player.GetComponent<WizardRukeAIServer>().AIClientId);
            }
            if (player.TryGetComponent<ChickenAIHPManagerServer>(out ChickenAIHPManagerServer chickenAIHPManagerServer))
            {
                chickenAIHPManagerServer.TakingDamage(damageValue);
            }
        }
    }
}
