using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Experimental.Playables;

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

        // Player ���� ȿ�� ����
        ApplyHealingEffect(collisionedClientId);

        // Player ���� VFX ����
        GameObject vfxHeal = Instantiate(GameAssetsManager.Instance.gameAssets.vfx_Heal, collision.gameObject.transform);
        vfxHeal.GetComponent<NetworkObject>().Spawn();
        vfxHeal.transform.SetParent(collision.transform);
        vfxHeal.transform.localPosition = new Vector3(0f, 0.1f, 0f);

        // SFX ����
        SoundManager.Instance?.PlayItemSFX(ItemName.Potion_HP, transform);

        // �ı�
        GetComponent<NetworkObject>().Despawn();
    }

    private void ApplyHealingEffect(ulong collisionedClientId) {
        PlayerInGameData playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(collisionedClientId);
        sbyte newHP = (sbyte)(playerData.hp + healingValue);
        if(newHP > playerData.maxHp) newHP = playerData.maxHp;

        PlayerHPManager.Instance.UpdatePlayerHP(collisionedClientId, newHP, playerData.maxHp);
    }
}
