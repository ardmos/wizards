using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 접촉한 플레이어 Level Up 시킬 스킬 선택창 띄워주는 아이템. 서버에서 동작합니다.
/// </summary>
public class ScrollLevelUp : Scroll
{
    private void OnCollisionEnter(Collision collision)
    {
        if(!IsServer) return;

        ulong collisionedClientId = collision.gameObject.GetComponent<NetworkObject>().OwnerClientId;

        collision.gameObject.GetComponent<Player>().ShowSelectSpellPopupClientRPC(this);

    }
}
