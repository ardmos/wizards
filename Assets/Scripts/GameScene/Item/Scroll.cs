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
    public byte spellSlotIndex;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        // spell slot ���� ����
        spellSlotIndex = (byte) Random.Range(0, 3);
        //Debug.Log($"spellSlotIndex: {spellSlotIndex}");
    }

    /// <summary>
    /// �������� ó���Ǵ� �޼ҵ� �Դϴ�.
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return;

        // �浹�� �÷��̾��� Scroll Queue�� �߰�.  <--- �� �κ� Ȯ��. ����ó�� �� �ǰ��ִ���. ���� �� ����� �� ������ �߻��Ѵ�. �ϴ±迡 �ɷµ鵵 ����ȭ.
        ulong collisionedClientId = collision.gameObject.GetComponent<NetworkObject>().OwnerClientId;
        ScrollManagerServer.Instance?.EnqueuePlayerScrollSpellSlotQueueOnServer(collisionedClientId, spellSlotIndex);

        // ȹ�� SFX ���
        SoundManager.Instance?.PlayItemSFX(ItemName.ScrollStart, transform);

        // ������Ʈ �ı�
        GetComponent<NetworkObject>().Despawn();
    }
}