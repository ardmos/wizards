using System.Collections;
using Unity.Netcode;
using UnityEngine;
using static ComponentValidator;

/// <summary>
/// AI ������ Ruke�� Ŭ���̾�Ʈ �� ����(UI, VFX)�� �����ϴ� Ŭ�����Դϴ�. 
/// </summary>
public class WizardRukeAIClient : NetworkBehaviour
{
    #region Constants & Fields
    // ���� �޽��� �����
    private const string ERROR_RIGIDBODY_NOT_SET = "WizardRukeAIClient mRigidbody ������ �ȵǾ��ֽ��ϴ�.";
    private const string ERROR_USERNAME_UI_CONTROLLER_NOT_SET = "WizardRukeAIClient userNameUIController ������ �ȵǾ��ֽ��ϴ�.";
    private const string ERROR_TEAMCOLOR_CONTROLLER_NOT_SET = "WizardRukeAIClient teamColorController ������ �ȵǾ��ֽ��ϴ�.";
    private const string ERROR_HP_UI_CONTROLLER_NOT_SET = "WizardRukeAIClient HPUIController ������ �ȵǾ��ֽ��ϴ�.";
    private const string ERROR_DAMAGE_TEXT_UI_CONTROLLER_NOT_SET = "WizardRukeAIClient DamageTextUIController ������ �ȵǾ��ֽ��ϴ�.";

    [SerializeField] private TeamColorController teamColorController;
    [SerializeField] private UserNameUIController userNameUIController;
    [SerializeField] private HPBarUIController hPBarUIController;
    [SerializeField] private DamageTextUIController damageTextUIController;
    [SerializeField] private Rigidbody mRigidbody;

    [Header("VFX�� ���͸����")]
    [SerializeField] private Material originalMaterial;
    [SerializeField] private Material highlightMaterial;
    [SerializeField] private Material frozenMaterial;
    [SerializeField] private Renderer[] ownRenderers;
    #endregion

    #region Initialization
    /// <summary>
    /// AI ĳ���͸� �ʱ�ȭ�մϴ�.
    /// �̸��� ���÷��� �������ݴϴ�.
    /// </summary>
    /// <param name="aiName">AIĳ���� �̸�</param>
    [ClientRpc]
    public virtual void InitializeAIClientRPC(string aiName)
    {
        // �ʿ��� ������Ʈ���� ��ȿ�� �˻�
        if (!ValidateComponent(mRigidbody, ERROR_RIGIDBODY_NOT_SET)) return;
        if (!ValidateComponent(userNameUIController, ERROR_HP_UI_CONTROLLER_NOT_SET)) return;
        if (!ValidateComponent(teamColorController, ERROR_TEAMCOLOR_CONTROLLER_NOT_SET)) return;

        mRigidbody.isKinematic = false;
        userNameUIController.SetName(aiName);
        teamColorController.Setup(isOwner: false);
    }
    #endregion

    #region UI Control
    /// <summary>
    /// HP�� UI�� ������Ʈ�մϴ�.
    /// </summary>
    /// <param name="currentHP">���� HP��</param>
    /// <param name="maxHP">�ִ� HP��</param>
    [ClientRpc]
    public void UpdateHPBarUIClientRPC(sbyte currentHP, sbyte maxHP)
    {
        if (!ValidateComponent(hPBarUIController, ERROR_HP_UI_CONTROLLER_NOT_SET)) return;

        hPBarUIController.SetHP(currentHP, maxHP);
    }

    /// <summary>
    /// ����� �ؽ�Ʈ �˾��� ǥ���մϴ�.
    /// </summary>
    /// <param name="damageAmount">����� ��ġ</param>
    [ClientRpc]
    public void ShowDamageTextPopupClientRPC(sbyte damageAmount)
    {
        if (!ValidateComponent(damageTextUIController, ERROR_DAMAGE_TEXT_UI_CONTROLLER_NOT_SET)) return;

        damageTextUIController.CreateTextObject(damageAmount);
    }

    /// <summary>
    /// �÷��̾� UI�� ��Ȱ��ȭ�մϴ�.
    /// </summary>
    [ClientRpc]
    public void OffPlayerUIClientRPC()
    {
        if (!ValidateComponent(hPBarUIController, ERROR_HP_UI_CONTROLLER_NOT_SET)) return;
        if (!ValidateComponent(userNameUIController, ERROR_USERNAME_UI_CONTROLLER_NOT_SET)) return;

        hPBarUIController.gameObject.SetActive(false);
        userNameUIController.gameObject.SetActive(false);
    }
    #endregion

    #region VFX Control
    /// <summary>
    /// ������ ����Ʈ�� Ȱ��ȭ�մϴ�.
    /// </summary>
    [ClientRpc]
    public void ActivateFrozenEffectClientRPC() => SetMaterialForAllRenderers(frozenMaterial);

    /// <summary>
    /// ������ ����Ʈ�� ��Ȱ��ȭ�մϴ�.
    /// </summary>
    [ClientRpc]
    public void DeactivateFrozenEffectClientRPC() => SetMaterialForAllRenderers(originalMaterial);

    /// <summary>
    /// �ǰ� ����Ʈ�� Ȱ��ȭ�մϴ�.
    /// </summary>
    [ClientRpc]
    public void ActivateHitByAttackEffectClientRPC() => StartCoroutine(ActivateHitEffectAndRestore());

    /// <summary>
    /// �ǰ� ĳ���� ��¦ ȿ���� Ȱ��ȭ�ϰ�, ���� �ð� �Ŀ� �ڵ����� ȿ���� ��Ȱ��ȭ�ϴ� �ڷ�ƾ �޼����Դϴ�.
    /// </summary>
    private IEnumerator ActivateHitEffectAndRestore()
    {
        Material previousMaterial = ownRenderers[0].material;
        SetMaterialForAllRenderers(highlightMaterial);
        yield return new WaitForSeconds(0.1f);
        SetMaterialForAllRenderers(previousMaterial);
    }

    /// <summary>
    /// ��� �������� ������ ���͸����� ��������ݴϴ�.
    /// ���� ����Ʈ ���⿡ ���˴ϴ�.
    /// </summary>
    /// <param name="material">���� ������ ���͸���</param>
    private void SetMaterialForAllRenderers(Material material)
    {
        foreach (Renderer renderer in ownRenderers)
        {
            renderer.material = material;
        }
    }
    #endregion
}