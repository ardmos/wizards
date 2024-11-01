using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 스크롤아이템의 획득과정을 처리하는 스크립트 입니다. 
/// 서버에서 동작합니다.
/// 
/// 아이템 자동생성 로직을 만들게되면, 생성시 spellSlotIndex값을 랜덤으로 지정해줘야합니다.
/// </summary>

public class Scroll : NetworkBehaviour
{
    public byte spellSlotIndex;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        // spell slot 랜덤 설정
        spellSlotIndex = (byte) Random.Range(0, 3);
        //Debug.Log($"spellSlotIndex: {spellSlotIndex}");
    }

    /// <summary>
    /// 서버에서 처리되는 메소드 입니다.
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return;

        // 충돌한 플레이어의 Scroll Queue에 추가.
        ulong collisionedClientId = collision.gameObject.GetComponent<NetworkObject>().OwnerClientId;
        SpellManager.Instance.EnqueuePlayerScrollSpellSlotQueueOnServer(collisionedClientId, spellSlotIndex);

        // 오브젝트 파괴
        GetComponent<NetworkObject>().Despawn();
    }
}