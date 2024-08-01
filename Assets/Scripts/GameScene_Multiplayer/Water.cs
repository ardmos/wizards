using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Water : NetworkBehaviour
{
    [SerializeField] private sbyte damageValue = 1;
    [SerializeField] private float intervalTime = 1f;

    private List<GameObject> playersInWater = new List<GameObject>(); // �浹�� �÷��̾ ������ ����Ʈ

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return;
        if (!collision.collider.CompareTag("Player") && !collision.collider.CompareTag("AI")) return;

        // �浹�� �÷��̾ ����Ʈ�� �߰�
        playersInWater.Add(collision.gameObject);

        // ���ʷ� �浹�� �÷��̾��� ��쿡�� InvokeRepeating �޼��� ȣ��
        if (playersInWater.Count == 1)
        {
            InvokeRepeating(nameof(DealDamage), 0f, intervalTime);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (!IsServer) return;
        if (!collision.collider.CompareTag("Player") && !collision.collider.CompareTag("AI")) return;

        // �浹�� ���� �÷��̾ ����Ʈ���� ����
        playersInWater.Remove(collision.gameObject);

        // �� �̻� �÷��̾ �浹 ������ �ʴٸ� InvokeRepeating �޼��� ����
        if (playersInWater.Count == 0)
        {
            CancelInvoke(nameof(DealDamage));
        }
    }

    private void DealDamage()
    {
        // �� �÷��̾�� ����� �ֱ�. �����ڴ� �÷��̾� �ڽ����� ó��
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
