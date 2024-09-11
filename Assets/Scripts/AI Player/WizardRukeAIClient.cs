using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class WizardRukeAIClient : NetworkBehaviour
{
    [Header("UI 컨트롤러들")]    
    public TeamColorController teamColorController;
    public UserNameUIController userNameUIController;
    public HPBarUIController hPBarUIController;
    public DamageTextUIController damageTextUIController;
    public Rigidbody mRigidbody;

    [Header("VFX용 메터리얼들")]
    [SerializeField] private Material currentMaterial;
    public Material originalMaterial;
    public Material highlightMaterial;
    public Material frozenMaterial;
    public Material enemyMaterial;
    public Renderer[] ownRenderers;

    /// <summary>
    /// AI 캐릭터 이름 & 메터리얼 초기화
    /// </summary>
    [ClientRpc]
    public virtual void InitializeAIClientRPC(string aiName)
    {
        Debug.Log($"WizardRuke AI Player InitializeAIClientRPC");

        // 자꾸 isKinematic이 켜져서 추가한 코드. Rigidbody network에서 계속 켜는 것 같다.
        mRigidbody.isKinematic = false;
        // 플레이어 닉네임 설정    
        userNameUIController?.SetName(aiName); 

        // 기본 메터리얼 초기화
        currentMaterial = originalMaterial;

        // UI 팀 컬러 설정. AI는 항상 Enemy 컬러
        teamColorController?.Setup(isOwner: false);

        // enemy 플레이어 테두리 컬러 설정
        foreach (Renderer renderer in ownRenderers)
        {
            renderer.material = enemyMaterial;
        }
    }

    // 캐릭터 프로즌 이펙트 실행
    [ClientRpc]
    public void ActivateFrozenEffectClientRPC()
    {
        currentMaterial = frozenMaterial;
        foreach (Renderer renderer in ownRenderers)
        {
            renderer.material = frozenMaterial;
        }
    }
    // 캐릭터 프로즌 이펙트 종료
    [ClientRpc]
    public void DeactivateFrozenEffectClientRPC()
    {
        currentMaterial = originalMaterial;
        foreach (Renderer renderer in ownRenderers)
        {
            renderer.material = originalMaterial;
        }
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
    public void SetHPClientRPC(sbyte hp, sbyte maxHP)
    {
        hPBarUIController?.SetHP(hp, maxHP);
    }

    [ClientRpc]
    public void ShowDamageTextPopupClientRPC(sbyte damageAmount)
    {
        damageTextUIController?.CreateTextObject(damageAmount);
    }

    [ClientRpc]
    public void OffPlayerUIClientRPC()
    {
        hPBarUIController.gameObject.SetActive(false);
        userNameUIController.gameObject?.SetActive(false);
    }

    public ICharacter GetCharacterData()
    {
        throw new System.NotImplementedException();
    }
}
