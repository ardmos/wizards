using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Experimental.Playables;

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

        // Player 힐링 효과 적용
        ApplyHealingEffect(collisionedClientId);

        // Player 힐링 VFX 실행
        GameObject vfxHeal = Instantiate(GameAssetsManager.Instance.gameAssets.vfx_Heal, collision.gameObject.transform);
        vfxHeal.GetComponent<NetworkObject>().Spawn();
        vfxHeal.transform.SetParent(collision.transform);
        vfxHeal.transform.localPosition = new Vector3(0f, 0.1f, 0f);

        // SFX 실행
        SoundManager.Instance?.PlayItemSFX(ItemName.Potion_HP, transform);

        // 파괴
        GetComponent<NetworkObject>().Despawn();
    }

    private void ApplyHealingEffect(ulong collisionedClientId) {
        PlayerInGameData playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(collisionedClientId);
        sbyte newHP = (sbyte)(playerData.hp + healingValue);
        if(newHP > playerData.maxHp) newHP = playerData.maxHp;

        PlayerHPManager.Instance.UpdatePlayerHP(collisionedClientId, newHP, playerData.maxHp);
    }
}
