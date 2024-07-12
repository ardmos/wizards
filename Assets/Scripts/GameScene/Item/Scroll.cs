using Unity.Netcode;
using UnityEngine;

/// <summary>
/// ��ũ�Ѿ������� ȹ������� ó���ϴ� ��ũ��Ʈ �Դϴ�. 
/// �������� �����մϴ�.
/// 
/// ������ �ڵ����� ������ ����ԵǸ�, ������ spellSlotIndex���� �������� ����������մϴ�.
/// </summary>

public class Scroll : NetworkBehaviour
{
    /// <summary>
    /// �������� ó���Ǵ� �޼ҵ� �Դϴ�.
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter(Collision collision)
    {
        //  ���� AI�� ȹ������ �ʰ� �ֽ��ϴ�. 
        if (!IsServer) return;

        // �浹�� �÷��̾��� Scroll Queue�� �߰�.
        if(collision.gameObject.TryGetComponent<PlayerServer>(out PlayerServer playerServer))
        {
            // �ǹ� ���� ���� 0
            ScrollManagerServer.Instance?.EnqueuePlayerScrollSpellSlotQueueOnServer(playerServer.OwnerClientId, 0); 
        }

        // ȹ�� SFX ���
        SoundManager.Instance?.PlayItemSFX(ItemName.ScrollPickup, transform);

        // ������Ʈ �ı�
        GetComponent<NetworkObject>().Despawn();
    }
}