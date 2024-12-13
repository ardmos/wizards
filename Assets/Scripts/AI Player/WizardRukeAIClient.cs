using System.Collections;
using Unity.Netcode;
using UnityEngine;
using static ComponentValidator;

/// <summary>
/// AI 마법사 Ruke의 클라이언트 측 동작(UI, VFX)을 관리하는 클래스입니다. 
/// </summary>
public class WizardRukeAIClient : NetworkBehaviour
{
    #region Constants & Fields
    // 에러 메시지 상수들
    private const string ERROR_RIGIDBODY_NOT_SET = "WizardRukeAIClient mRigidbody 설정이 안되어있습니다.";
    private const string ERROR_USERNAME_UI_CONTROLLER_NOT_SET = "WizardRukeAIClient userNameUIController 설정이 안되어있습니다.";
    private const string ERROR_TEAMCOLOR_CONTROLLER_NOT_SET = "WizardRukeAIClient teamColorController 설정이 안되어있습니다.";
    private const string ERROR_HP_UI_CONTROLLER_NOT_SET = "WizardRukeAIClient HPUIController 설정이 안되어있습니다.";
    private const string ERROR_DAMAGE_TEXT_UI_CONTROLLER_NOT_SET = "WizardRukeAIClient DamageTextUIController 설정이 안되어있습니다.";

    [SerializeField] private TeamColorController teamColorController;
    [SerializeField] private UserNameUIController userNameUIController;
    [SerializeField] private HPBarUIController hPBarUIController;
    [SerializeField] private DamageTextUIController damageTextUIController;
    [SerializeField] private Rigidbody mRigidbody;

    [Header("VFX용 메터리얼들")]
    [SerializeField] private Material originalMaterial;
    [SerializeField] private Material highlightMaterial;
    [SerializeField] private Material frozenMaterial;
    [SerializeField] private Renderer[] ownRenderers;
    #endregion

    #region Initialization
    /// <summary>
    /// AI 캐릭터를 초기화합니다.
    /// 이름과 팀컬러를 설정해줍니다.
    /// </summary>
    /// <param name="aiName">AI캐릭터 이름</param>
    [ClientRpc]
    public virtual void InitializeAIClientRPC(string aiName)
    {
        // 필요한 컴포넌트들의 유효성 검사
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
    /// HP바 UI를 업데이트합니다.
    /// </summary>
    /// <param name="currentHP">현재 HP값</param>
    /// <param name="maxHP">최대 HP값</param>
    [ClientRpc]
    public void UpdateHPBarUIClientRPC(sbyte currentHP, sbyte maxHP)
    {
        if (!ValidateComponent(hPBarUIController, ERROR_HP_UI_CONTROLLER_NOT_SET)) return;

        hPBarUIController.SetHP(currentHP, maxHP);
    }

    /// <summary>
    /// 대미지 텍스트 팝업을 표시합니다.
    /// </summary>
    /// <param name="damageAmount">대미지 수치</param>
    [ClientRpc]
    public void ShowDamageTextPopupClientRPC(sbyte damageAmount)
    {
        if (!ValidateComponent(damageTextUIController, ERROR_DAMAGE_TEXT_UI_CONTROLLER_NOT_SET)) return;

        damageTextUIController.CreateTextObject(damageAmount);
    }

    /// <summary>
    /// 플레이어 UI를 비활성화합니다.
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
    /// 프로즌 이펙트를 활성화합니다.
    /// </summary>
    [ClientRpc]
    public void ActivateFrozenEffectClientRPC() => SetMaterialForAllRenderers(frozenMaterial);

    /// <summary>
    /// 프로즌 이펙트를 비활성화합니다.
    /// </summary>
    [ClientRpc]
    public void DeactivateFrozenEffectClientRPC() => SetMaterialForAllRenderers(originalMaterial);

    /// <summary>
    /// 피격 이펙트를 활성화합니다.
    /// </summary>
    [ClientRpc]
    public void ActivateHitByAttackEffectClientRPC() => StartCoroutine(ActivateHitEffectAndRestore());

    /// <summary>
    /// 피격 캐릭터 반짝 효과를 활성화하고, 일정 시간 후에 자동으로 효과를 비활성화하는 코루틴 메서드입니다.
    /// </summary>
    private IEnumerator ActivateHitEffectAndRestore()
    {
        Material previousMaterial = ownRenderers[0].material;
        SetMaterialForAllRenderers(highlightMaterial);
        yield return new WaitForSeconds(0.1f);
        SetMaterialForAllRenderers(previousMaterial);
    }

    /// <summary>
    /// 모든 렌더러에 동일한 메터리얼을 적용시켜줍니다.
    /// 각종 이펙트 연출에 사용됩니다.
    /// </summary>
    /// <param name="material">새로 적용할 메터리얼</param>
    private void SetMaterialForAllRenderers(Material material)
    {
        foreach (Renderer renderer in ownRenderers)
        {
            renderer.material = material;
        }
    }
    #endregion
}