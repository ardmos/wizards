using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.Playables;

/// <summary>
/// 접촉한 플레이어 HP 회복메소드 동작. 서버에서 동작해야 합니다.
/// </summary>
public class PotionHP : NetworkBehaviour
{
    [SerializeField] private sbyte healingValue = 1;

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return;

        // Player 힐링 적용
        if (collision.collider.CompareTag("Player")) 
        {
            if(collision.gameObject.TryGetComponent<PlayerHPManagerServer>(out PlayerHPManagerServer playerHPManagerServer)){
                playerHPManagerServer.ApplyHeal(healingValue);
            }
        }
        // AI 힐링 적용
        else if (collision.collider.CompareTag("AI"))
        {
            if (collision.gameObject.TryGetComponent<WizardRukeAIHPManagerServer>(out WizardRukeAIHPManagerServer wizardRukeAIHPManagerServer))
            {
                wizardRukeAIHPManagerServer.ApplyHeal(healingValue);
            }
        }

        // 힐링 VFX 실행
        GameObject vfxHeal = Instantiate(GameAssetsManager.Instance.gameAssets.vfx_Heal, collision.gameObject.transform);
        vfxHeal.GetComponent<NetworkObject>().Spawn();
        vfxHeal.transform.SetParent(collision.transform);
        vfxHeal.transform.localPosition = new Vector3(0f, 0.1f, 0f);

        // 힐링 SFX 실행
        SoundManager.Instance?.PlayItemSFX(ItemName.Potion_HP, transform);

        // 파괴
        GetComponent<NetworkObject>().Despawn();
    }
}
