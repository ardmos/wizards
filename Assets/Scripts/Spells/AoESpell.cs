using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Matchmaker.Models;
using UnityEngine;

public class AoESpell : NetworkBehaviour, IOwnerSeter
{
    [SerializeField] protected SpellInfo spellInfo;
    [SerializeField] protected float slowValue;

    [SerializeField] private ulong _shooterClientID;
    [SerializeField] private float intervalTime = 2f;

    [Header("스킬의 영향을 받는 플레이어 목록")]
    [SerializeField] private List<GameObject> playersInArea = new List<GameObject>();

    [Header("AI가 피격됐을 시 타겟으로 설정될 마법을 소유한 플레이어 오브젝트.")]
    public GameObject spellOwnerObject;

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
            플레이어블리자드슬로우효과복구(player);
            // AI의 경우
            AI블리자드슬로우효과복구(player);
        }
    }

    // 2초 주기로 실행될 메서드
    private void DealDamage()
    {
        // 각 플레이어에게 대미지 주기. 공격자는 플레이어 자신으로 처리
        foreach (var player in playersInArea)
        {
            // 시전자는 피해 안받도록 설정
            if (player == spellOwnerObject) continue;

            if (player == null || player.gameObject == null) continue;

            if(player.TryGetComponent<PlayerHPManagerServer>(out PlayerHPManagerServer playerHPManagerServer))
            {
                playerHPManagerServer.TakingDamage((sbyte)spellInfo.damage, player.GetComponent<PlayerClient>().OwnerClientId);
            }
            if(player.TryGetComponent<WizardRukeAIHPManagerServer>(out WizardRukeAIHPManagerServer wizardRukeAIHPManagerServer))
            {
                wizardRukeAIHPManagerServer.TakingDamage((sbyte)spellInfo.damage, player.GetComponent<WizardRukeAIServer>().AIClientId);
            }
        }
    }

    public void SetOwner(ulong shooterClientID, GameObject spellOwnerObject)
    {
        if (!IsServer) return;
        _shooterClientID = shooterClientID;
        this.spellOwnerObject = spellOwnerObject;

        Debug.Log($"AI 플레이어{shooterClientID}");
    }

    public virtual void InitAoESpell(SpellInfo spellInfoFromServer)
    {
        if (IsClient) return;

        spellInfo = spellInfoFromServer;
        slowValue = 2f;
    }

    private void 플레이어전용트리거엔터(Collider other)
    {
        플레이어블리자드슬로우효과적용(other.gameObject);

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
        AI블리자드슬로우효과적용(other.gameObject);

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
        플레이어블리자드슬로우효과복구(other.gameObject);

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
        AI블리자드슬로우효과복구(other.gameObject);

        // 충돌을 끝낸 플레이어를 리스트에서 제거
        playersInArea.Remove(other.gameObject);

        // 더 이상 플레이어가 충돌 중이지 않다면 InvokeRepeating 메서드 중지
        if (playersInArea.Count == 0)
        {
            CancelInvoke(nameof(DealDamage));
        }
    }

    private void 플레이어블리자드슬로우효과적용(GameObject other)
    {
        if (!other) return;

        // 충돌한 플레이어 이동속도 저하.
        if (other.TryGetComponent<PlayerMovementServer>(out PlayerMovementServer playerMovement))
        {
            playerMovement.ReduceMoveSpeed(slowValue);
            //Debug.Log($"player{other.GetComponent<NetworkObject>().OwnerClientId} ReduceMoveSpeed result : {playerMovement.GetMoveSpeed()} ");
        }
        // 프로즌 이펙트 실행
        if (other.TryGetComponent<PlayerClient>(out PlayerClient playerClient))
        {
            //Debug.Log($"is playerClient found: {playerClient}");
            playerClient.ActivateFrozenEffectClientRPC();
        }
    }
    private void 플레이어블리자드슬로우효과복구(GameObject other)
    {
        if(!other) return;

        if (other.TryGetComponent<PlayerMovementServer>(out PlayerMovementServer playerMovement))
        {
            playerMovement.AddMoveSpeed(slowValue);
            //Debug.Log($"player{player.GetComponent<NetworkObject>().OwnerClientId} AddMoveSpeed result : {playerMovement.GetMoveSpeed()} ");
        }
        // 프로즌 이펙트 해제
        if (other.TryGetComponent<PlayerClient>(out PlayerClient playerClient))
        {
            //Debug.Log($"is playerClient found: {playerClient}");
            playerClient.DeactivateFrozenEffectClientRPC();
        }
    }

    private void AI블리자드슬로우효과적용(GameObject other)
    {
        if (!other) return;

        // 충돌한 플레이어 이동속도 저하.
        if (other.TryGetComponent<WizardRukeAIMovementServer>(out WizardRukeAIMovementServer aiPlayerMovement))
        {
            aiPlayerMovement.ReduceMoveSpeed(slowValue);
            //Debug.Log($"player{other.GetComponent<NetworkObject>().OwnerClientId} ReduceMoveSpeed result : {playerMovement.GetMoveSpeed()} ");
        }
        // 프로즌 이펙트 실행
        if (other.TryGetComponent<WizardRukeAIClient>(out WizardRukeAIClient aiPlayerClient))
        {
            //Debug.Log($"is playerClient found: {playerClient}");
            aiPlayerClient.ActivateFrozenEffectClientRPC();
        }
    }
    private void AI블리자드슬로우효과복구(GameObject other)
    {
        if (!other) return;

        if (other.TryGetComponent<WizardRukeAIMovementServer>(out WizardRukeAIMovementServer aiPlayerMovement))
        {
            aiPlayerMovement.AddMoveSpeed(slowValue);
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
