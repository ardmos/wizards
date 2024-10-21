using System.Collections;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Wizard 캐릭터의 서버측 스펠 매니저 스크립트 입니다.
/// Server Auth 방식으로 마법 발동을 관리합니다. 
/// </summary>
public class SpellManagerServerWizard : SpellManagerServer
{
    public Transform muzzlePos_Normal;
    public Transform muzzlePos_AoE;

    private GameObject playerCastingSpell;

    #region Defence Spell Cast
    /// <summary>
    /// 방어 마법 시전
    /// </summary>
    /// <param name="player"></param>
    [ServerRpc(RequireOwnership = false)]
    public void ActivateShieldServerRPC()
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
        StartCoroutine(StartAndResetAnimState(spellObject.GetComponent<DefenceSpell>().GetSpellInfo().lifetime));

        // 잠시 무적 처리
        tag = "Invincible";
    }

    IEnumerator StartAndResetAnimState(float lifeTime)
    {
        if (playerAnimator == null) yield break;

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
    /// Blizzard 스킬 전용 캐스팅 메서드
    /// </summary>
    /// <param name="spellIndex"></param>
    [ServerRpc (RequireOwnership = false)]
    public void CastingBlizzardServerRPC()
    {
        if (playerAnimator == null) return;

        // 범위 표시 오브젝트 생성
        GameObject spellObject = Instantiate(GameAssetsManager.Instance.GetSpellPrefab(SpellName.BlizzardLv1_Ready), muzzlePos_AoE.position, Quaternion.identity);
        spellObject.GetComponent<NetworkObject>().Spawn();

        spellObject.transform.SetParent(transform);
        // 포구에 발사체 위치시키기
        spellObject.transform.localPosition = muzzlePos_AoE.localPosition;

        // 플레이어가 보고있는 방향과 발사체가 바라보는 방향 일치시키기
        spellObject.transform.forward = transform.forward;

        // 플레이어가 시전중인 마법에 저장하기
        playerCastingSpell = spellObject;

        // 해당 플레이어의 마법 SpellState 업데이트
        UpdatePlayerSpellState(2, SpellState.Casting);

        // 캐스팅 애니메이션 실행
        playerAnimator.UpdateWizardMaleAnimationOnServer(WizardMaleAnimState.CastingAttackMagic);
    }

    [ServerRpc (RequireOwnership = false)]
    public void ReleaseBlizzardServerRPC(ServerRpcParams serverRpcParams = default)
    {
        if (playerAnimator == null) return;
        if (GetSpellInfo(2) == null) return; // 임시로 블리자드 스킬을 스펠인덱스2번으로 고정합니다.

        // 1. 시전중인 범위표시 오브젝트 제거
        Destroy(playerCastingSpell);
        // 2. 블리자드 스킬 이펙트오브젝트 생성
        GameObject spellObject = Instantiate(GameAssetsManager.Instance.GetSpellPrefab(SpellName.BlizzardLv1), muzzlePos_AoE.position, Quaternion.identity);
        spellObject.GetComponent<NetworkObject>().Spawn();
        if (spellObject.TryGetComponent<AoESpell>(out var aoESpell))
        {
            aoESpell.SetOwner(OwnerClientId, gameObject);
            aoESpell.InitAoESpell(GetSpellInfo(2));
        }

        // 해당 SpellState 업데이트
        UpdatePlayerSpellState(2, SpellState.Cooltime);
        spellObject.transform.SetParent(MultiplayerGameManager.Instance.transform);

        // 발사 애니메이션 실행
        playerAnimator.UpdateWizardMaleAnimationOnServer(WizardMaleAnimState.ShootingMagic);
    }


    /// <summary>
    /// 공격 마법 생성해주기. 캐스팅 시작 ( NetworkObject는 Server에서만 생성 가능합니다 )
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void CastingNormalSpellServerRPC(ushort spellIndex)
    {
        if (playerAnimator == null) return;
        SpellInfo spellInfo = GetSpellInfo(spellIndex);
        if (spellInfo == null) return;  

        // 발사체 오브젝트 생성
        GameObject spellObject = Instantiate(GameAssetsManager.Instance.GetSpellPrefab(spellInfo.spellName), muzzlePos_Normal.position, Quaternion.identity);
        spellObject.GetComponent<NetworkObject>().Spawn();

        // 발사체 스펙 초기화 해주기
        spellObject.GetComponent<AttackSpell>().InitSpellInfoDetail(spellInfo, gameObject);
        // 호밍 마법이라면 호밍 마법에 소유자 등록 & 속도 설정
        if (spellObject.TryGetComponent<HomingMissile>(out var ex))
        {
            ex.SetOwner(OwnerClientId, gameObject);
            ex.SetSpeed(spellInfo.moveSpeed);
        }
        spellObject.transform.SetParent(transform);

        // 포구에 발사체 위치시키기
        spellObject.transform.localPosition = muzzlePos_Normal.localPosition;

        // 마법 생성 SFX 실행
        SoundManager.Instance?.PlayWizardSpellSFX(spellInfo.spellName, SFX_Type.Aiming, transform);

        // 플레이어가 보고있는 방향과 발사체가 바라보는 방향 일치시키기
        spellObject.transform.forward = transform.forward;

        // 플레이어가 시전중인 마법에 저장하기
        playerCastingSpell = spellObject;

        // 해당 플레이어의 마법 SpellState 업데이트
        UpdatePlayerSpellState(spellIndex, SpellState.Casting);

        // 캐스팅 애니메이션 실행
        playerAnimator.UpdateWizardMaleAnimationOnServer(WizardMaleAnimState.CastingAttackMagic);
    }

    /// <summary>
    /// 플레이어의 요청으로 현재 캐스팅중인 노멀 타입 마법을 발사하는 메소드 입니다.
    /// </summary>
    /// <param name="serverRpcParams"></param>
    [ServerRpc(RequireOwnership = false)]
    public void ReleaseNormalSpellServerRPC(ushort spellIndex, ServerRpcParams serverRpcParams = default)
    {
        if (playerCastingSpell==null) return;
        if (playerAnimator==null) return;

        // 해당 SpellState 업데이트
        UpdatePlayerSpellState(spellIndex, SpellState.Cooltime);

        playerCastingSpell.transform.SetParent(MultiplayerGameManager.Instance.transform);
        float moveSpeed = playerCastingSpell.GetComponent<AttackSpell>().GetSpellInfo().moveSpeed;

        // 호밍 마법이라면 호밍 시작 처리
        if (playerCastingSpell.TryGetComponent<HomingMissile>(out var ex)) ex.StartHoming();
        // 설치 마법
        else if (moveSpeed == 0) {
            
        }
        // 마법 발사 (기본 직선 비행 마법)
        else
        {
            playerCastingSpell.GetComponent<AttackSpell>().Shoot(playerCastingSpell.transform.forward * moveSpeed, ForceMode.Impulse);
        }

        // 발사 SFX 실행 
        SpellInfo spellInfo = GetSpellInfo(spellIndex);
        SoundManager.Instance?.PlayWizardSpellSFX(spellInfo.spellName, SFX_Type.Shooting, transform);

        // 포구 VFX
        MuzzleVFX(playerCastingSpell.GetComponent<AttackSpell>().GetMuzzleVFXPrefab(), GetComponentInChildren<MuzzlePos>().transform);

        // 발사 애니메이션 실행
        playerAnimator.UpdateWizardMaleAnimationOnServer(WizardMaleAnimState.ShootingMagic);
    }
    #endregion

    #region Spell VFX
    // 포구 VFX 
    public void MuzzleVFX(GameObject muzzleVFXPrefab, Transform muzzleTransform)
    {
        if (muzzleVFXPrefab == null) return;

        GameObject muzzleVFX = Instantiate(muzzleVFXPrefab, muzzleTransform.position, Quaternion.identity);
        muzzleVFX.GetComponent<NetworkObject>().Spawn();
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
