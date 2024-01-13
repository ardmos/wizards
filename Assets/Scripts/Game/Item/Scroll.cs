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
    /// 서버에서 처리되는 메소드 입니다.
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return;

        // 플레이어에게 스크롤 효과를 적용할 스펠 선택 팝업 띄워주기
        ulong collisionedClientId = collision.gameObject.GetComponent<NetworkObject>().OwnerClientId;
        collision.gameObject.GetComponent<Player>().ShowSelectSpellPopupClientRPC(this);

        // 오브젝트 파괴
        GetComponent<NetworkObject>().Despawn();
    }

    /// <summary>
    /// 클라이언트에서 처리되는 메소드입니다.
    /// 효과를 적용하고싶은 스펠의 spellIndex를 파라미터로 넘겨주면 서버권한 방식으로 효과가 적용됩니다.
    /// </summary>
    /// <param name="spellInfo"></param>
    public void UpdateScrollEffectToServer(sbyte spellIndex)
    {
        // 레벨업된 새로운 스펠 정보를 업로드
        SpellManager.Instance.UpdateScrollEffectServerRPC(scrollName, spellIndex);
    }
}