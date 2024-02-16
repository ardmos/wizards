using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 스크롤아이템의 획득과정을 처리하는 스크립트 입니다. 
/// 서버에서 동작합니다.
/// 
/// 아이템 자동생성 로직을 만들게되면, 생성시 spellSlotIndex값을 랜덤으로 지정해줘야합니다.
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
        // spell slot 랜덤 설정
        spellSlotIndex = (byte) Random.Range(0, 3);
        Debug.Log($"spellSlotIndex: {spellSlotIndex}");
    }

    /// <summary>
    /// 서버에서 처리되는 메소드 입니다.
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return;

        // 플레이어에게 스크롤 효과를 적용할 스펠 선택 팝업 띄워주기
        /*        ulong collisionedClientId = collision.gameObject.GetComponent<NetworkObject>().OwnerClientId;
                collision.gameObject.GetComponent<Player>().ShowSelectSpellPopupClientRPC(this);*/

        // 충돌한 플레이어의 Scroll Queue에 추가.
        ulong collisionedClientId = collision.gameObject.GetComponent<NetworkObject>().OwnerClientId;
        SpellManager.Instance.EnqueuePlayerScrollSpellSlotQueueOnServer(collisionedClientId, spellSlotIndex);

        // 오브젝트 파괴
        GetComponent<NetworkObject>().Despawn();
    }

    /// <summary>
    /// 클라이언트에서 처리되는 메소드입니다.
    /// 효과를 적용하고싶은 스펠의 spellIndex를 파라미터로 넘겨주면 서버권한 방식으로 효과가 적용됩니다.
    /// </summary>
    /// <param name="spellInfo"></param>
 /*   public void UpdateScrollEffectToServer(byte spellIndex)
    {
        // 레벨업된 새로운 스펠 정보를 업로드
        //SpellManager.Instance.UpdateScrollEffectServerRPC(scrollName, spellIndex);
    }*/
}