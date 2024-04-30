using System.Collections;
using Unity.Netcode;
using UnityEngine;
/// <summary>
/// 마법을 Server Auth 방식으로 시전할 수 있도록 도와주는 스크립트 입니다.
/// Scroll의 획득과 적용의 과정도 관리합니다.
/// 서버에서 동작하는 스크립트들이 모여있어야 합니다. 추후 코드 정리하면서 확인 필요.
/// </summary>
public class SpellManagerServerWizard : SkillSpellManagerServer
{
    private GameObject playerCastingSpell;

    #region Defence Spell Cast
    /// <summary>
    /// 방어 마법 시전
    /// </summary>
    /// <param name="player"></param>
    [ServerRpc(RequireOwnership = false)]
    public void StartActivateDefenceSpellServerRPC()
    {
        // 마법 시전
        GameObject spellObject = Instantiate(GameAssetsManager.Instance.GetSpellPrefab(GetSpellInfo(DEFENCE_SPELL_INDEX_DEFAULT).spellName), transform.position, Quaternion.identity);
        spellObject.GetComponent<NetworkObject>().Spawn();
        spellObject.GetComponent<DefenceSpell>().InitSpellInfoDetail(GetSpellInfo(DEFENCE_SPELL_INDEX_DEFAULT));
        spellObject.transform.SetParent(transform);
        spellObject.transform.localPosition = Vector3.zero;
        spellObject.GetComponent<DefenceSpell>().Activate();

        // 마법 생성 사운드 재생
        spellObject.GetComponent<DefenceSpell>().PlaySFX(SFX_Type.Aiming);

        // 해당 SpellState 업데이트
        UpdatePlayerSpellState(DEFENCE_SPELL_INDEX_DEFAULT, SpellState.Cooltime);

        // 애니메이션 실행
        StartCoroutine(StartAndResetAnimState(spellObject.GetComponent<DefenceSpell>().GetSpellInfo().lifeTime));

        // 잠시 무적 처리
        tag = "Invincible";
    }

    IEnumerator StartAndResetAnimState(float lifeTime)
    {
        playerAnimator.UpdateWizardMaleAnimationOnServer(WizardMaleAnimState.CastingDefensiveMagic);
        yield return new WaitForSeconds(lifeTime);

        // 무적 해제
        tag = "Player";

        // 플레이어 캐릭터가 Casting 애니메이션중이 아닐 경우에만 Idle로 변경
        if (!playerAnimator.playerAttackAnimState.Equals(WizardMaleAnimState.CastingAttackMagic))
        {
            playerAnimator.UpdateWizardMaleAnimationOnServer(WizardMaleAnimState.Idle);
        }
    }
    #endregion

    #region Attack Spell Cast&Fire
    /// <summary>
    /// 공격 마법 생성해주기. 캐스팅 시작 ( NetworkObject는 Server에서만 생성 가능합니다 )
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void CastingSpellServerRPC(ushort spellIndex)
    {
        // 포구 위치 찾기(Local posittion)
        Transform muzzleTransform = GetComponentInChildren<MuzzlePos>().transform;

        // 발사체 오브젝트 생성
        GameObject spellObject = Instantiate(GameAssetsManager.Instance.GetSpellPrefab(GetSpellInfo(spellIndex).spellName), muzzleTransform.position, Quaternion.identity);
        spellObject.GetComponent<NetworkObject>().Spawn();

        // 발사체 스펙 초기화 해주기
        SpellInfo spellInfo = new SpellInfo(GetSpellInfo(spellIndex));
        spellObject.GetComponent<AttackSpell>().InitSpellInfoDetail(spellInfo);
        spellObject.transform.SetParent(transform);
        // 포구에 발사체 위치시키기
        spellObject.transform.localPosition = muzzleTransform.localPosition;

        // 마법 생성 사운드 재생
        spellObject.GetComponent<AttackSpell>().PlaySFX(SFX_Type.Aiming);

        // 플레이어가 보고있는 방향과 발사체가 바라보는 방향 일치시키기
        spellObject.transform.forward = transform.forward;

        // 플레이어가 시전중인 마법에 저장하기
        playerCastingSpell = spellObject;

        // 해당 플레이어의 마법 SpellState 업데이트
        UpdatePlayerSpellState(spellIndex, SpellState.Aiming);

        // 캐스팅 애니메이션 실행
        playerAnimator.UpdateWizardMaleAnimationOnServer(WizardMaleAnimState.CastingAttackMagic);
    }

    /// <summary>
    /// 플레이어의 요청으로 현재 캐스팅중인 마법을 발사하는 메소드 입니다.
    /// </summary>
    /// <param name="serverRpcParams"></param>
    [ServerRpc(RequireOwnership = false)]
    public void ShootSpellServerRPC(ushort spellIndex, ServerRpcParams serverRpcParams = default)
    {
        ulong clientId = serverRpcParams.Receive.SenderClientId;
        GameObject spellObject = playerCastingSpell;
        if (spellObject == null)
        {
            //Debug.Log($"ShootSpellServerRPC : Wrong Request. Player{clientId} has no casting spell object.");
            return;
        }

        // 해당 SpellState 업데이트
        UpdatePlayerSpellState(spellIndex, SpellState.Cooltime);

        //Debug.Log($"{nameof(ShootSpellServerRPC)} ownerClientId {clientId}");

        spellObject.transform.SetParent(GameManager.Instance.transform);

        // 마법 발사 (기본 직선 비행 마법)
        float moveSpeed = spellObject.GetComponent<AttackSpell>().GetSpellInfo().moveSpeed;
        spellObject.GetComponent<AttackSpell>().Shoot(spellObject.transform.forward * moveSpeed, ForceMode.Impulse);

        // 포구 VFX
        MuzzleVFX(spellObject.GetComponent<AttackSpell>().GetMuzzleVFXPrefab(), GetComponentInChildren<MuzzlePos>().transform);

        // 발사 애니메이션 실행
        playerAnimator.UpdateWizardMaleAnimationOnServer(WizardMaleAnimState.ShootingMagic);
    }
    #endregion

    #region Spell VFX
    // 포구 VFX 
    public void MuzzleVFX(GameObject muzzleVFXPrefab, Transform muzzleTransform)
    {
        if (muzzleVFXPrefab == null)
        {
            //Debug.Log($"MuzzleVFX muzzleVFXPrefab is null");
            return;
        }

        //Debug.Log($"MuzzleVFX muzzlePos:{muzzleTransform.position}, muzzleLocalPos:{muzzleTransform.localPosition}");
        GameObject muzzleVFX = Instantiate(muzzleVFXPrefab, muzzleTransform.position, Quaternion.identity);
        muzzleVFX.GetComponent<NetworkObject>().Spawn();
        //muzzleVFX.transform.SetParent(transform);
        muzzleVFX.transform.position = muzzleTransform.position;
        muzzleVFX.transform.forward = muzzleTransform.forward;
        var particleSystem = muzzleVFX.GetComponent<ParticleSystem>();

        if (particleSystem == null)
        {
            particleSystem = muzzleVFX.transform.GetChild(0).GetComponent<ParticleSystem>();
        }

        Destroy(muzzleVFX, particleSystem.main.duration);
    }
    #endregion
}
