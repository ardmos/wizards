using Unity.Netcode;
using UnityEngine;

/// <summary>
/// ��ũ�Ѿ������� ȹ������� ó���ϴ� ��ũ��Ʈ �Դϴ�. 
/// �������� �����մϴ�.
/// 
/// ������ �ڵ����� ������ ����ԵǸ�, ������ spellSlotIndex���� �������� ����������մϴ�.
/// </summary>

//[System.Serializable]
public class Scroll : NetworkBehaviour//, INetworkSerializable
{
    //public ItemName scrollName;
    public byte spellSlotIndex;
    /*
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            //serializer.SerializeValue(ref scrollName);
            serializer.SerializeValue(ref spellSlotIndex);
        }*/

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        // spell slot ���� ����
        spellSlotIndex = (byte) Random.Range(0, 3);
        Debug.Log($"spellSlotIndex: {spellSlotIndex}");
    }

    /// <summary>
    /// �������� ó���Ǵ� �޼ҵ� �Դϴ�.
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return;

        // �÷��̾�� ��ũ�� ȿ���� ������ ���� ���� �˾� ����ֱ�
        /*        ulong collisionedClientId = collision.gameObject.GetComponent<NetworkObject>().OwnerClientId;
                collision.gameObject.GetComponent<Player>().ShowSelectSpellPopupClientRPC(this);*/

        // �浹�� �÷��̾��� Scroll Queue�� �߰�.
        ulong collisionedClientId = collision.gameObject.GetComponent<NetworkObject>().OwnerClientId;
        SpellManager.Instance.EnqueuePlayerScrollSpellSlotQueueOnServer(collisionedClientId, spellSlotIndex);

        // ������Ʈ �ı�
        GetComponent<NetworkObject>().Despawn();
    }

    /// <summary>
    /// Ŭ���̾�Ʈ���� ó���Ǵ� �޼ҵ��Դϴ�.
    /// ȿ���� �����ϰ���� ������ spellIndex�� �Ķ���ͷ� �Ѱ��ָ� �������� ������� ȿ���� ����˴ϴ�.
    /// </summary>
    /// <param name="spellInfo"></param>
 /*   public void UpdateScrollEffectToServer(byte spellIndex)
    {
        // �������� ���ο� ���� ������ ���ε�
        //SpellManager.Instance.UpdateScrollEffectServerRPC(scrollName, spellIndex);
    }*/
}