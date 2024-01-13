using Unity.Netcode;
using UnityEngine;

[System.Serializable]
public class Scroll : NetworkBehaviour, INetworkSerializable
{
    public Item.ItemName scrollName;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref scrollName);
    }

    /// <summary>
    /// �������� ó���Ǵ� �޼ҵ� �Դϴ�.
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return;

        // �÷��̾�� ��ũ�� ȿ���� ������ ���� ���� �˾� ����ֱ�
        ulong collisionedClientId = collision.gameObject.GetComponent<NetworkObject>().OwnerClientId;
        collision.gameObject.GetComponent<Player>().ShowSelectSpellPopupClientRPC(this);

        // ������Ʈ �ı�
        GetComponent<NetworkObject>().Despawn();
    }

    /// <summary>
    /// Ŭ���̾�Ʈ���� ó���Ǵ� �޼ҵ��Դϴ�.
    /// ȿ���� �����ϰ���� ������ spellIndex�� �Ķ���ͷ� �Ѱ��ָ� �������� ������� ȿ���� ����˴ϴ�.
    /// </summary>
    /// <param name="spellInfo"></param>
    public void UpdateScrollEffectToServer(sbyte spellIndex)
    {
        // �������� ���ο� ���� ������ ���ε�
        SpellManager.Instance.UpdateScrollEffectServerRPC(scrollName, spellIndex);
    }
}