using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Matchmaker.Models;
using UnityEngine;

public class AoESpell : NetworkBehaviour, IOwnerSeter
{
    [SerializeField] private ulong _shooterClientID;
    [SerializeField] private LayerMask _shooterLayer;
    [SerializeField] private sbyte damageValue = 1;
    [SerializeField] private float intervalTime = 2f;

    [Header("스킬의 영향을 받는 플레이어 목록")]
    [SerializeField] private List<GameObject> playersInArea = new List<GameObject>();

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;
        if (other.CompareTag("Player"))
        {
            플레이어전용트리거엔터(other);
        }
        else if (other.CompareTag("AI"))
        {
            AI전용트리거엔터(other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsServer) return;
        if (other.CompareTag("Player"))
        {
            플레이어전용트리거엑싯(other);
        }
        else if (other.CompareTag("AI"))
        {
            AI전용트리거엑싯(other);
        }
    }

    public override void OnDestroy()
    {
        // 스킬이 사라질 때 아직 스킬의 영역에 남아있는 플레이어가 있는 경우 이동속도 복구시켜주고 스킬 제거
        foreach (var player in playersInArea)
        {
            // 플레이어의 경우
            플레이어블리자드효과복구(player);
            // AI의 경우
            AI블리자드효과복구(player);
        }
    }

    // 2초 주기로 실행될 메서드
    private void DealDamage()
    {
        // 각 플레이어에게 대미지 주기. 공격자는 플레이어 자신으로 처리
        foreach (var player in playersInArea)
        {
            if (player == null || player.gameObject == null) continue;

            if(player.TryGetComponent<PlayerHPManagerServer>(out PlayerHPManagerServer playerHPManagerServer))
            {
                playerHPManagerServer.TakingDamage(damageValue, player.GetComponent<PlayerClient>().OwnerClientId);
            }
            if(player.TryGetComponent<WizardRukeAIHPManagerServer>(out WizardRukeAIHPManagerServer wizardRukeAIHPManagerServer))
            {
                wizardRukeAIHPManagerServer.TakingDamage(damageValue, player.GetComponent<WizardRukeAIServer>().AIClientId);
            }
        }
    }

    public void SetOwner(ulong shooterClientID)
    {
        if (!IsServer) return;
        _shooterClientID = shooterClientID;

        // 플레이어 Layer 설정
        switch (_shooterClientID)
        {
            case 0:
                gameObject.layer = LayerMask.NameToLayer("Attack Magic Player0");
                _shooterLayer = LayerMask.NameToLayer("Player0");
                break;
            case 1:
                gameObject.layer = LayerMask.NameToLayer("Attack Magic Player1");
                _shooterLayer = LayerMask.NameToLayer("Player1");
                break;
            case 2:
                gameObject.layer = LayerMask.NameToLayer("Attack Magic Player2");
                _shooterLayer = LayerMask.NameToLayer("Player2");
                break;
            case 3:
                gameObject.layer = LayerMask.NameToLayer("Attack Magic Player3");
                _shooterLayer = LayerMask.NameToLayer("Player3");
                break;
            default:
                _shooterLayer = LayerMask.NameToLayer("Player");
                break;
        }
        
        // 플레이어 본인 Layer는 충돌체크에서 제외합니다
        Physics.IgnoreLayerCollision(gameObject.layer, _shooterLayer, true);
    }

    private void 플레이어전용트리거엔터(Collider other)
    {
        플레이어블리자드효과적용(other.gameObject);

        // 충돌한 플레이어를 리스트에 추가
        playersInArea.Add(other.gameObject);

        // 최초로 충돌한 플레이어인 경우에만 InvokeRepeating 메서드 호출
        if (playersInArea.Count == 1)
        {
            InvokeRepeating(nameof(DealDamage), 0f, intervalTime);
        }
    }
    private void AI전용트리거엔터(Collider other)
    {
        AI블리자드효과적용(other.gameObject);

        // 충돌한 플레이어를 리스트에 추가
        playersInArea.Add(other.gameObject);

        // 최초로 충돌한 플레이어인 경우에만 InvokeRepeating 메서드 호출
        if (playersInArea.Count == 1)
        {
            InvokeRepeating(nameof(DealDamage), 0f, intervalTime);
        }
    }

    private void 플레이어전용트리거엑싯(Collider other)
    {
        플레이어블리자드효과복구(other.gameObject);

        // 충돌을 끝낸 플레이어를 리스트에서 제거
        playersInArea.Remove(other.gameObject);

        // 더 이상 플레이어가 충돌 중이지 않다면 InvokeRepeating 메서드 중지
        if (playersInArea.Count == 0)
        {
            CancelInvoke(nameof(DealDamage));
        }
    }
    private void AI전용트리거엑싯(Collider other)
    {
        AI블리자드효과복구(other.gameObject);

        // 충돌을 끝낸 플레이어를 리스트에서 제거
        playersInArea.Remove(other.gameObject);

        // 더 이상 플레이어가 충돌 중이지 않다면 InvokeRepeating 메서드 중지
        if (playersInArea.Count == 0)
        {
            CancelInvoke(nameof(DealDamage));
        }
    }

    private void 플레이어블리자드효과적용(GameObject other)
    {
        // 충돌한 플레이어 이동속도 저하.
        if (other.TryGetComponent<PlayerMovementServer>(out PlayerMovementServer playerMovement))
        {
            playerMovement.ReduceMoveSpeed(2f);
            //Debug.Log($"player{other.GetComponent<NetworkObject>().OwnerClientId} ReduceMoveSpeed result : {playerMovement.GetMoveSpeed()} ");
        }
        // 프로즌 이펙트 실행
        if (other.TryGetComponent<PlayerClient>(out PlayerClient playerClient))
        {
            //Debug.Log($"is playerClient found: {playerClient}");
            playerClient.ActivateFrozenEffectClientRPC();
        }
    }
    private void 플레이어블리자드효과복구(GameObject other)
    {
        if (other.TryGetComponent<PlayerMovementServer>(out PlayerMovementServer playerMovement))
        {
            playerMovement.AddMoveSpeed(2f);
            //Debug.Log($"player{player.GetComponent<NetworkObject>().OwnerClientId} AddMoveSpeed result : {playerMovement.GetMoveSpeed()} ");
        }
        // 프로즌 이펙트 해제
        if (other.TryGetComponent<PlayerClient>(out PlayerClient playerClient))
        {
            //Debug.Log($"is playerClient found: {playerClient}");
            playerClient.DeactivateFrozenEffectClientRPC();
        }
    }

    private void AI블리자드효과적용(GameObject other)
    {
        // 충돌한 플레이어 이동속도 저하.
        if (other.TryGetComponent<WizardRukeAIServer>(out WizardRukeAIServer aiPlayerMovement))
        {
            aiPlayerMovement.ReduceMoveSpeed(2f);
            //Debug.Log($"player{other.GetComponent<NetworkObject>().OwnerClientId} ReduceMoveSpeed result : {playerMovement.GetMoveSpeed()} ");
        }
        // 프로즌 이펙트 실행
        if (other.TryGetComponent<WizardRukeAIClient>(out WizardRukeAIClient aiPlayerClient))
        {
            //Debug.Log($"is playerClient found: {playerClient}");
            aiPlayerClient.ActivateFrozenEffectClientRPC();
        }
    }
    private void AI블리자드효과복구(GameObject other)
    {
        if (other.TryGetComponent<WizardRukeAIServer>(out WizardRukeAIServer aiPlayerMovement))
        {
            aiPlayerMovement.AddMoveSpeed(2f);
            //Debug.Log($"player{other.GetComponent<NetworkObject>().OwnerClientId} AddMoveSpeed result : {playerMovement.GetMoveSpeed()} ");
        }
        // 프로즌 이펙트 해제
        if (other.TryGetComponent<WizardRukeAIClient>(out WizardRukeAIClient aiPlayerClient))
        {
            //Debug.Log($"is playerClient found: {playerClient}");
            aiPlayerClient.DeactivateFrozenEffectClientRPC();
        }
    }
}
