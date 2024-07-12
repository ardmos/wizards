using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.Playables;

/// <summary>
/// ������ �÷��̾� HP ȸ���޼ҵ� ����. �������� �����ؾ� �մϴ�.
/// </summary>
public class PotionHP : NetworkBehaviour
{
    [SerializeField] private sbyte healingValue = 1;

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return;

        // Player ���� ����
        if (collision.collider.CompareTag("Player")) 
        {
            if(collision.gameObject.TryGetComponent<PlayerHPManagerServer>(out PlayerHPManagerServer playerHPManagerServer)){
                playerHPManagerServer.ApplyHeal(healingValue);
            }
        }
        // AI ���� ����
        else if (collision.collider.CompareTag("AI"))
        {
            if (collision.gameObject.TryGetComponent<WizardRukeAIHPManagerServer>(out WizardRukeAIHPManagerServer wizardRukeAIHPManagerServer))
            {
                wizardRukeAIHPManagerServer.ApplyHeal(healingValue);
            }
        }

        // ���� VFX ����
        GameObject vfxHeal = Instantiate(GameAssetsManager.Instance.gameAssets.vfx_Heal, collision.gameObject.transform);
        vfxHeal.GetComponent<NetworkObject>().Spawn();
        vfxHeal.transform.SetParent(collision.transform);
        vfxHeal.transform.localPosition = new Vector3(0f, 0.1f, 0f);

        // ���� SFX ����
        SoundManager.Instance?.PlayItemSFX(ItemName.Potion_HP, transform);

        // �ı�
        GetComponent<NetworkObject>().Despawn();
    }
}
