using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ChickenAIClient : NetworkBehaviour
{
    [Header("VFX용 메터리얼들")]
    [SerializeField] private Material currentMaterial;
    public Material originalMaterial;
    public Material highlightMaterial;
    public Material frozenMaterial;
    public Renderer[] ownRenderers;
    public ChickenAIHPManagerClient chickenAIHPManagerClient;
    public DamageTextUIController damageTextUIController;

    // 캐릭터 프로즌 이펙트 실행
    [ClientRpc]
    public void ActivateFrozenEffectClientRPC()
    {
        foreach (Renderer renderer in ownRenderers)
        {
            renderer.material = frozenMaterial;
        }
        currentMaterial = frozenMaterial;

        Debug.Log($"ActivateFrozenEffectClientRPC. {currentMaterial}");
    }
    // 캐릭터 프로즌 이펙트 종료
    [ClientRpc]
    public void DeactivateFrozenEffectClientRPC()
    {
        foreach (Renderer renderer in ownRenderers)
        {
            renderer.material = originalMaterial;
        }
        currentMaterial = originalMaterial;

        Debug.Log($"ActivateFrozenEffectClientRPC. {currentMaterial}");
    }

    // 피격 캐릭터 반짝 이펙트 실행
    [ClientRpc]
    public void ActivateHitByAttackEffectClientRPC()
    {
        foreach (Renderer renderer in ownRenderers)
        {
            renderer.material = highlightMaterial;
        }

        StartCoroutine(ResetFlashEffect());
    }
    // 피격 캐릭터 반짝 효과를 일정 시간 후에 비활성화하는 코루틴 메서드
    private IEnumerator ResetFlashEffect()
    {
        yield return new WaitForSeconds(0.1f);

        foreach (Renderer renderer in ownRenderers)
        {
            renderer.material = currentMaterial;
        }
    }

    [ClientRpc]
    public void ShowDamageTextPopupClientRPC(sbyte damageAmount)
    {
        damageTextUIController?.CreateTextObject(damageAmount);
    }
}
