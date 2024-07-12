using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ChickenAIClient : NetworkBehaviour
{
    [Header("VFX�� ���͸����")]
    [SerializeField] private Material currentMaterial;
    public Material originalMaterial;
    public Material highlightMaterial;
    public Material frozenMaterial;
    public Renderer[] ownRenderers;
    public ChickenAIHPManagerClient chickenAIHPManagerClient;
    public DamageTextUIController damageTextUIController;

    // ĳ���� ������ ����Ʈ ����
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
    // ĳ���� ������ ����Ʈ ����
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

    // �ǰ� ĳ���� ��¦ ����Ʈ ����
    [ClientRpc]
    public void ActivateHitByAttackEffectClientRPC()
    {
        foreach (Renderer renderer in ownRenderers)
        {
            renderer.material = highlightMaterial;
        }

        StartCoroutine(ResetFlashEffect());
    }
    // �ǰ� ĳ���� ��¦ ȿ���� ���� �ð� �Ŀ� ��Ȱ��ȭ�ϴ� �ڷ�ƾ �޼���
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
