using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 접촉한 플레이어 HP 회복메소드 동작. 서버에서 동작해야 합니다.
/// </summary>
public class PotionHP : NetworkBehaviour
{
    [SerializeField] private sbyte healingValue = 2;

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return;

        ulong collisionedClientId = collision.gameObject.GetComponent<NetworkObject>().OwnerClientId;
        sbyte newPlayerHP = GetNewPlayerHP(collisionedClientId);

        PlayerHPManager.Instance.SetPlayerHPOnServer(newPlayerHP, collisionedClientId);

        // 파괴시 효과 추가 필요.
        GetComponent<NetworkObject>().Despawn();
    }

    private sbyte GetNewPlayerHP(ulong collisionedClientId) {
        PlayerData playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(collisionedClientId);
        sbyte newHP = (sbyte)(playerData.playerHP + healingValue);
        if(newHP > playerData.playerMaxHP) newHP = playerData.playerMaxHP;
        return newHP;
    }
}
