using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class WizardRukeAIClient : NetworkBehaviour
{
    [Header("UI ��Ʈ�ѷ���")]    
    public UserNameUIController userNameUIController;
    public HPBarUIController hPBarUIController;
    public DamageTextUIController damageTextUIController;
    public Rigidbody mRigidbody;
    public PlayerSpellScrollQueueManagerClient playerSpellScrollQueueManager;

    [Header("VFX�� ���͸����")]
    [SerializeField] private Material currentMaterial;
    public Material originalMaterial;
    public Material highlightMaterial;
    public Material frozenMaterial;
    public Material enemyMaterial;
    public Renderer[] ownRenderers;

    /// <summary>
    /// AI ĳ���� �̸� & ���͸��� �ʱ�ȭ
    /// </summary>
    [ClientRpc]
    public virtual void InitializeAIClientRPC(string aiName)
    {
        Debug.Log($"WizardRuke AI Player InitializeAIClientRPC");

        // �ڲ� isKinematic�� ������ �߰��� �ڵ�. Rigidbody network���� ��� �Ѵ� �� ����.
        mRigidbody.isKinematic = false;
        // �÷��̾� �г��� ����    
        userNameUIController?.Setup(aiName, false); 

        // �⺻ ���͸��� �ʱ�ȭ
        currentMaterial = originalMaterial;

        // enemy �÷��̾� �׵θ� �÷� ����
        foreach (Renderer renderer in ownRenderers)
        {
            renderer.material = enemyMaterial;
        }
        return;
    }

    // ĳ���� ������ ����Ʈ ����
    [ClientRpc]
    public void ActivateFrozenEffectClientRPC()
    {
        currentMaterial = frozenMaterial;
        foreach (Renderer renderer in ownRenderers)
        {
            renderer.material = frozenMaterial;
        }
    }
    // ĳ���� ������ ����Ʈ ����
    [ClientRpc]
    public void DeactivateFrozenEffectClientRPC()
    {
        currentMaterial = originalMaterial;
        foreach (Renderer renderer in ownRenderers)
        {
            renderer.material = originalMaterial;
        }
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
    public void SetHPClientRPC(sbyte hp, sbyte maxHP)
    {
        hPBarUIController?.SetHP(hp, maxHP, false);
    }

    [ClientRpc]
    public void ShowDamageTextPopupClientRPC(sbyte damageAmount)
    {
        damageTextUIController?.CreateTextObject(damageAmount);
    }

    public ICharacter GetCharacterData()
    {
        throw new System.NotImplementedException();
    }
}