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

    [Header("��ų�� ������ �޴� �÷��̾� ���")]
    [SerializeField] private List<GameObject> playersInArea = new List<GameObject>();

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;
        if (other.CompareTag("Player"))
        {
            �÷��̾�����Ʈ���ſ���(other);
        }
        else if (other.CompareTag("AI"))
        {
            AI����Ʈ���ſ���(other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsServer) return;
        if (other.CompareTag("Player"))
        {
            �÷��̾�����Ʈ���ſ���(other);
        }
        else if (other.CompareTag("AI"))
        {
            AI����Ʈ���ſ���(other);
        }
    }

    public override void OnDestroy()
    {
        // ��ų�� ����� �� ���� ��ų�� ������ �����ִ� �÷��̾ �ִ� ��� �̵��ӵ� ���������ְ� ��ų ����
        foreach (var player in playersInArea)
        {
            // �÷��̾��� ���
            �÷��̾���ڵ�ȿ������(player);
            // AI�� ���
            AI���ڵ�ȿ������(player);
        }
    }

    // 2�� �ֱ�� ����� �޼���
    private void DealDamage()
    {
        // �� �÷��̾�� ����� �ֱ�. �����ڴ� �÷��̾� �ڽ����� ó��
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

        // �÷��̾� Layer ����
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
        
        // �÷��̾� ���� Layer�� �浹üũ���� �����մϴ�
        Physics.IgnoreLayerCollision(gameObject.layer, _shooterLayer, true);
    }

    private void �÷��̾�����Ʈ���ſ���(Collider other)
    {
        �÷��̾���ڵ�ȿ������(other.gameObject);

        // �浹�� �÷��̾ ����Ʈ�� �߰�
        playersInArea.Add(other.gameObject);

        // ���ʷ� �浹�� �÷��̾��� ��쿡�� InvokeRepeating �޼��� ȣ��
        if (playersInArea.Count == 1)
        {
            InvokeRepeating(nameof(DealDamage), 0f, intervalTime);
        }
    }
    private void AI����Ʈ���ſ���(Collider other)
    {
        AI���ڵ�ȿ������(other.gameObject);

        // �浹�� �÷��̾ ����Ʈ�� �߰�
        playersInArea.Add(other.gameObject);

        // ���ʷ� �浹�� �÷��̾��� ��쿡�� InvokeRepeating �޼��� ȣ��
        if (playersInArea.Count == 1)
        {
            InvokeRepeating(nameof(DealDamage), 0f, intervalTime);
        }
    }

    private void �÷��̾�����Ʈ���ſ���(Collider other)
    {
        �÷��̾���ڵ�ȿ������(other.gameObject);

        // �浹�� ���� �÷��̾ ����Ʈ���� ����
        playersInArea.Remove(other.gameObject);

        // �� �̻� �÷��̾ �浹 ������ �ʴٸ� InvokeRepeating �޼��� ����
        if (playersInArea.Count == 0)
        {
            CancelInvoke(nameof(DealDamage));
        }
    }
    private void AI����Ʈ���ſ���(Collider other)
    {
        AI���ڵ�ȿ������(other.gameObject);

        // �浹�� ���� �÷��̾ ����Ʈ���� ����
        playersInArea.Remove(other.gameObject);

        // �� �̻� �÷��̾ �浹 ������ �ʴٸ� InvokeRepeating �޼��� ����
        if (playersInArea.Count == 0)
        {
            CancelInvoke(nameof(DealDamage));
        }
    }

    private void �÷��̾���ڵ�ȿ������(GameObject other)
    {
        // �浹�� �÷��̾� �̵��ӵ� ����.
        if (other.TryGetComponent<PlayerMovementServer>(out PlayerMovementServer playerMovement))
        {
            playerMovement.ReduceMoveSpeed(2f);
            //Debug.Log($"player{other.GetComponent<NetworkObject>().OwnerClientId} ReduceMoveSpeed result : {playerMovement.GetMoveSpeed()} ");
        }
        // ������ ����Ʈ ����
        if (other.TryGetComponent<PlayerClient>(out PlayerClient playerClient))
        {
            //Debug.Log($"is playerClient found: {playerClient}");
            playerClient.ActivateFrozenEffectClientRPC();
        }
    }
    private void �÷��̾���ڵ�ȿ������(GameObject other)
    {
        if (other.TryGetComponent<PlayerMovementServer>(out PlayerMovementServer playerMovement))
        {
            playerMovement.AddMoveSpeed(2f);
            //Debug.Log($"player{player.GetComponent<NetworkObject>().OwnerClientId} AddMoveSpeed result : {playerMovement.GetMoveSpeed()} ");
        }
        // ������ ����Ʈ ����
        if (other.TryGetComponent<PlayerClient>(out PlayerClient playerClient))
        {
            //Debug.Log($"is playerClient found: {playerClient}");
            playerClient.DeactivateFrozenEffectClientRPC();
        }
    }

    private void AI���ڵ�ȿ������(GameObject other)
    {
        // �浹�� �÷��̾� �̵��ӵ� ����.
        if (other.TryGetComponent<WizardRukeAIServer>(out WizardRukeAIServer aiPlayerMovement))
        {
            aiPlayerMovement.ReduceMoveSpeed(2f);
            //Debug.Log($"player{other.GetComponent<NetworkObject>().OwnerClientId} ReduceMoveSpeed result : {playerMovement.GetMoveSpeed()} ");
        }
        // ������ ����Ʈ ����
        if (other.TryGetComponent<WizardRukeAIClient>(out WizardRukeAIClient aiPlayerClient))
        {
            //Debug.Log($"is playerClient found: {playerClient}");
            aiPlayerClient.ActivateFrozenEffectClientRPC();
        }
    }
    private void AI���ڵ�ȿ������(GameObject other)
    {
        if (other.TryGetComponent<WizardRukeAIServer>(out WizardRukeAIServer aiPlayerMovement))
        {
            aiPlayerMovement.AddMoveSpeed(2f);
            //Debug.Log($"player{other.GetComponent<NetworkObject>().OwnerClientId} AddMoveSpeed result : {playerMovement.GetMoveSpeed()} ");
        }
        // ������ ����Ʈ ����
        if (other.TryGetComponent<WizardRukeAIClient>(out WizardRukeAIClient aiPlayerClient))
        {
            //Debug.Log($"is playerClient found: {playerClient}");
            aiPlayerClient.DeactivateFrozenEffectClientRPC();
        }
    }
}
