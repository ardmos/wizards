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
    public Transform muzzlePos_Normal;
    public Transform muzzlePos_AoE;

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
    /// Blizzard 스킬 전용 캐스팅 메서드
    /// </summary>
    /// <param name="spellIndex"></param>
    [ServerRpc (RequireOwnership = false)]
    public void CastingBlizzardServerRPC()
    {
        // 범위 표시 오브젝트 생성
        GameObject spellObject = Instantiate(GameAssetsManager.Instance.GetSpellPrefab(SkillName.BlizzardLv1_Ready), muzzlePos_AoE.position, Quaternion.identity);
        spellObject.GetComponent<NetworkObject>().Spawn();

        spellObject.transform.SetParent(transform);
        // 포구에 발사체 위치시키기
        spellObject.transform.localPosition = muzzlePos_AoE.localPosition;

        // 플레이어가 보고있는 방향과 발사체가 바라보는 방향 일치시키기
        spellObject.transform.forward = transform.forward;

        // 플레이어가 시전중인 마법에 저장하기
        playerCastingSpell = spellObject;

        // 해당 플레이어의 마법 SpellState 업데이트
        UpdatePlayerSpellState(2, SpellState.Aiming);

        // 캐스팅 애니메이션 실행
        playerAnimator.UpdateWizardMaleAnimationOnServer(WizardMaleAnimState.CastingAttackMagic);
    }

    [ServerRpc (RequireOwnership = false)]
    public void SetBlizzardServerRPC()
    {
        // 1. 시전중인 범위표시 오브젝트 제거
        Destroy(playerCastingSpell);
        // 2. 블리자드 스킬 이펙트오브젝트 생성
        GameObject spellObject = Instantiate(GameAssetsManager.Instance.GetSpellPrefab(SkillName.BlizzardLv1), muzzlePos_AoE.position, Quaternion.identity);
        spellObject.GetComponent<NetworkObject>().Spawn();
        if (spellObject.TryGetComponent<AoESpell>(out var aoESpell))
        {
            aoESpell.SetOwner(OwnerClientId);
        }

        Destroy(spellObject, 4f);
        // 해당 SpellState 업데이트
        UpdatePlayerSpellState(2, SpellState.Cooltime);
        spellObject.transform.SetParent(GameManager.Instance.transform);

        // 발사 애니메이션 실행
        playerAnimator.UpdateWizardMaleAnimationOnServer(WizardMaleAnimState.ShootingMagic);
    }


    /// <summary>
    /// 공격 마법 생성해주기. 캐스팅 시작 ( NetworkObject는 Server에서만 생성 가능합니다 )
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void CastingSpellServerRPC(ushort spellIndex)
    {
        // 발사체 오브젝트 생성
        GameObject spellObject = Instantiate(GameAssetsManager.Instance.GetSpellPrefab(GetSpellInfo(spellIndex).spellName), muzzlePos_Normal.position, Quaternion.identity);
        spellObject.GetComponent<NetworkObject>().Spawn();

        // 발사체 스펙 초기화 해주기
        SpellInfo spellInfo = new SpellInfo(GetSpellInfo(spellIndex));
        spellObject.GetComponent<AttackSpell>().InitSpellInfoDetail(spellInfo);
        // 호밍 마법이라면 호밍 마법에 소유자 등록 & 속도 설정
        if (spellObject.TryGetComponent<HomingMissile>(out var ex))
        {
            ex.SetOwner(OwnerClientId);
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
        float moveSpeed = spellObject.GetComponent<AttackSpell>().GetSpellInfo().moveSpeed;

        // 호밍 마법이라면 호밍 시작 처리
        if (spellObject.TryGetComponent<HomingMissile>(out var ex)) ex.StartHoming();
        // 설치 마법
        else if (moveSpeed == 0) {
            
        }
        // 마법 발사 (기본 직선 비행 마법)
        else
        {           
            spellObject.GetComponent<AttackSpell>().Shoot(spellObject.transform.forward * moveSpeed, ForceMode.Impulse);
        }

        // 발사 SFX 실행 
        SpellInfo spellInfo = new SpellInfo(GetSpellInfo(spellIndex));
        SoundManager.Instance?.PlayWizardSpellSFX(spellInfo.spellName, SFX_Type.Shooting, transform);

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
