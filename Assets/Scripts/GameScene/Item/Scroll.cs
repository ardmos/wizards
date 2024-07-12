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
    /// <summary>
    /// 서버에서 처리되는 메소드 입니다.
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter(Collision collision)
    {
        //  현재 AI는 획득하지 않고 있습니다. 
        if (!IsServer) return;

        // 충돌한 플레이어의 Scroll Queue에 추가.
        if(collision.gameObject.TryGetComponent<PlayerServer>(out PlayerServer playerServer))
        {
            // 의미 없는 숫자 0
            ScrollManagerServer.Instance?.EnqueuePlayerScrollSpellSlotQueueOnServer(playerServer.OwnerClientId, 0); 
        }

        // 획득 SFX 재생
        SoundManager.Instance?.PlayItemSFX(ItemName.ScrollPickup, transform);

        // 오브젝트 파괴
        GetComponent<NetworkObject>().Despawn();
    }
}