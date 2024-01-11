using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// ������ �÷��̾� HP ȸ���޼ҵ� ����. �������� �����ؾ� �մϴ�.
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

        // Player ���� VFX ����
        GameObject vfxHeal = Instantiate(GameAssets.instantiate.vfxHeal, collision.transform);
        vfxHeal.GetComponent<NetworkObject>().Spawn();
        vfxHeal.transform.SetParent(collision.transform);
        vfxHeal.transform.localPosition = new Vector3(0f, 0.1f, 0f);

        // �ı�
        GetComponent<NetworkObject>().Despawn();
    }

    private sbyte GetNewPlayerHP(ulong collisionedClientId) {
        PlayerData playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(collisionedClientId);
        sbyte newHP = (sbyte)(playerData.playerHP + healingValue);
        if(newHP > playerData.playerMaxHP) newHP = playerData.playerMaxHP;
        return newHP;
    }
}
