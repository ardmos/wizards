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

    [Header("��ų�� ������ �޴� �÷��̾� ���")]
    [SerializeField] private List<GameObject> playersInArea = new List<GameObject>();

    [Header("AI�� �ǰݵ��� �� Ÿ������ ������ ������ ������ �÷��̾� ������Ʈ.")]
    public GameObject spellOwnerObject;

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
            �÷��̾���ڵ彽�ο�ȿ������(player);
            // AI�� ���
            AI���ڵ彽�ο�ȿ������(player);
        }
    }

    // 2�� �ֱ�� ����� �޼���
    private void DealDamage()
    {
        // �� �÷��̾�� ����� �ֱ�. �����ڴ� �÷��̾� �ڽ����� ó��
        foreach (var player in playersInArea)
        {
            // �����ڴ� ���� �ȹ޵��� ����
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

        Debug.Log($"AI �÷��̾�{shooterClientID}");
    }

    public virtual void InitAoESpell(SpellInfo spellInfoFromServer)
    {
        if (IsClient) return;

        spellInfo = spellInfoFromServer;
        slowValue = 2f;
    }

    private void �÷��̾�����Ʈ���ſ���(Collider other)
    {
        �÷��̾���ڵ彽�ο�ȿ������(other.gameObject);

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
        AI���ڵ彽�ο�ȿ������(other.gameObject);

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
        �÷��̾���ڵ彽�ο�ȿ������(other.gameObject);

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
        AI���ڵ彽�ο�ȿ������(other.gameObject);

        // �浹�� ���� �÷��̾ ����Ʈ���� ����
        playersInArea.Remove(other.gameObject);

        // �� �̻� �÷��̾ �浹 ������ �ʴٸ� InvokeRepeating �޼��� ����
        if (playersInArea.Count == 0)
        {
            CancelInvoke(nameof(DealDamage));
        }
    }

    private void �÷��̾���ڵ彽�ο�ȿ������(GameObject other)
    {
        if (!other) return;

        // �浹�� �÷��̾� �̵��ӵ� ����.
        if (other.TryGetComponent<PlayerMovementServer>(out PlayerMovementServer playerMovement))
        {
            playerMovement.ReduceMoveSpeed(slowValue);
            //Debug.Log($"player{other.GetComponent<NetworkObject>().OwnerClientId} ReduceMoveSpeed result : {playerMovement.GetMoveSpeed()} ");
        }
        // ������ ����Ʈ ����
        if (other.TryGetComponent<PlayerClient>(out PlayerClient playerClient))
        {
            //Debug.Log($"is playerClient found: {playerClient}");
            playerClient.ActivateFrozenEffectClientRPC();
        }
    }
    private void �÷��̾���ڵ彽�ο�ȿ������(GameObject other)
    {
        if(!other) return;

        if (other.TryGetComponent<PlayerMovementServer>(out PlayerMovementServer playerMovement))
        {
            playerMovement.AddMoveSpeed(slowValue);
            //Debug.Log($"player{player.GetComponent<NetworkObject>().OwnerClientId} AddMoveSpeed result : {playerMovement.GetMoveSpeed()} ");
        }
        // ������ ����Ʈ ����
        if (other.TryGetComponent<PlayerClient>(out PlayerClient playerClient))
        {
            //Debug.Log($"is playerClient found: {playerClient}");
            playerClient.DeactivateFrozenEffectClientRPC();
        }
    }

    private void AI���ڵ彽�ο�ȿ������(GameObject other)
    {
        if (!other) return;

        // �浹�� �÷��̾� �̵��ӵ� ����.
        if (other.TryGetComponent<WizardRukeAIMovementServer>(out WizardRukeAIMovementServer aiPlayerMovement))
        {
            aiPlayerMovement.ReduceMoveSpeed(slowValue);
            //Debug.Log($"player{other.GetComponent<NetworkObject>().OwnerClientId} ReduceMoveSpeed result : {playerMovement.GetMoveSpeed()} ");
        }
        // ������ ����Ʈ ����
        if (other.TryGetComponent<WizardRukeAIClient>(out WizardRukeAIClient aiPlayerClient))
        {
            //Debug.Log($"is playerClient found: {playerClient}");
            aiPlayerClient.ActivateFrozenEffectClientRPC();
        }
    }
    private void AI���ڵ彽�ο�ȿ������(GameObject other)
    {
        if (!other) return;

        if (other.TryGetComponent<WizardRukeAIMovementServer>(out WizardRukeAIMovementServer aiPlayerMovement))
        {
            aiPlayerMovement.AddMoveSpeed(slowValue);
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
