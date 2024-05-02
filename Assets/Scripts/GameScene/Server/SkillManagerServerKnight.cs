using System.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class SkillManagerServerKnight : SkillSpellManagerServer
{
    // 대쉬 스킬 정보를 일단 하드코딩으로 넣어뒀습니다. 추후 구글시트와 JSON을 활용한 연결을 할 때 수정하면 됩니다.
    public float dashDistance = 25f; // 대쉬 거리
    public float dashDuration = 0.05f; // 대쉬 지속 시간

    // 스킬 시전 위치
    public Transform attackVerticalMuzzle;
    public Transform attackWhirlwindMuzzle;
    public Transform attackChargeReadyMuzzle;
    public Transform attackChargeShootMuzzle;

    private GameObject chargeEffectObject;

    [ServerRpc(RequireOwnership = false)]
    public void ReadyAttackSkillServerRPC(ushort skillIndex)
    {
        // 게임패트UI 스킬 버튼 드래그도중 조준이 가능하도록 하기 위한 처리
        UpdatePlayerSpellState(skillIndex, SpellState.Aiming);

        // 준비자세 애니메이션 실행
        switch (skillIndex)
        {
            case 0:
                playerAnimator.UpdateKnightMaleAnimationOnServer(KnightMaleAnimState.AttackVerticalReady);
                break;
            case 1:
                playerAnimator.UpdateKnightMaleAnimationOnServer(KnightMaleAnimState.AttackWhirlwindReady);
                break;
            case 2:
                AttackChargeReady();
                break;
            default:
                break;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void StartAttackSkillServerRPC(ushort skillIndex)
    {
        Debug.Log("1.StartAttackSkillServerRPC");
        // Knight_male의 세 번째 스킬은 첫 번째 스킬과 동일합니다. (전기베기1)
        // 마법 시전
        switch (skillIndex)
        {
            case 0:
                AttackVertical();
                break;
            case 1:
                AttackWhirlwind();
                break;
            case 2:
                AttackChargeShoot();
                break;
            default: 
                break;
        }
    }

    private void AttackVertical()
    {
        // 스킬 이펙트 생성
        SpellInfo skillInfo = new SpellInfo(GetSpellInfo((ushort)0));
        GameObject spellObject = Instantiate(GameAssetsManager.Instance.GetSpellPrefab(skillInfo.spellName), attackVerticalMuzzle.position, attackVerticalMuzzle.localRotation);
        spellObject.GetComponent<NetworkObject>().Spawn();
        skillInfo.ownerPlayerClientId = OwnerClientId;
        spellObject.GetComponent<SlashSkill>().InitSkillInfoDetail(skillInfo);
        spellObject.GetComponent<SlashSkill>().SetSelfDestroy();

        // 위치 설정
        spellObject.transform.SetParent(transform);
        spellObject.transform.localPosition = attackVerticalMuzzle.localPosition;
        spellObject.transform.localRotation = attackVerticalMuzzle.localRotation;

        // State 업데이트
        UpdatePlayerSpellState(0, SpellState.Cooltime);

        // Anim 실행
        playerAnimator.UpdateKnightMaleAnimationOnServer(KnightMaleAnimState.AttackVertical);

        // SFX 실행
        SoundManager.Instance?.PlayKnightSkillSFX(SkillName.ElectricSlashAttackVertical_Lv1, SFX_Type.Shooting, transform);
    }

    private void AttackWhirlwind()
    {
        Debug.Log("2.AttackWhirlwind");
        // 스킬 이펙트 생성
        SpellInfo skillInfo = new SpellInfo(GetSpellInfo((ushort)1));
        GameObject spellObject = Instantiate(GameAssetsManager.Instance.GetSpellPrefab(skillInfo.spellName), attackWhirlwindMuzzle.position, Quaternion.identity);
        spellObject.GetComponent<NetworkObject>().Spawn();
        skillInfo.ownerPlayerClientId = OwnerClientId;
        spellObject.GetComponent<SlashSkill>().InitSkillInfoDetail(skillInfo);
        spellObject.GetComponent<SlashSkill>().SetSelfDestroy();

        // 위치 설정
        spellObject.transform.SetParent(transform);
        spellObject.transform.localPosition = attackWhirlwindMuzzle.localPosition;

        // State 업데이트
        UpdatePlayerSpellState(1, SpellState.Cooltime);

        // Anim 실행
        playerAnimator.UpdateKnightMaleAnimationOnServer(KnightMaleAnimState.AttackWhirlwind);

        // SFX 실행
        SoundManager.Instance?.PlayKnightSkillSFX(SkillName.ElectricSlashAttackWhirlwind_Lv1, SFX_Type.Shooting, transform);
    }

    private void AttackChargeReady()
    {
        // 스킬 이펙트 생성
        chargeEffectObject = Instantiate(GameAssetsManager.Instance.GetSpellPrefab(SkillName.ElectricSlashAttackCharge_Lv1), attackChargeReadyMuzzle.position, Quaternion.identity);
        chargeEffectObject.GetComponent<NetworkObject>().Spawn();
        // 위치 설정
        chargeEffectObject.transform.SetParent(transform);
        chargeEffectObject.transform.localPosition = attackChargeReadyMuzzle.localPosition;

        // 차징 어택인데, 일단 애니메이션 세로공격이랑 같이 쓰게 설정.
        playerAnimator.UpdateKnightMaleAnimationOnServer(KnightMaleAnimState.AttackVerticalReady);

        // SFX 실행
        SoundManager.Instance?.PlayKnightSkillSFX(SkillName.ElectricSlashAttackChargeSlash_Lv1, SFX_Type.Aiming, transform);
    }

    private void AttackChargeShoot()
    {
        // 충전 이펙트 오브젝트 제거
        Destroy(chargeEffectObject);

        // 스킬 이펙트 생성
        SpellInfo skillInfo = new SpellInfo(GetSpellInfo((ushort)2));
        GameObject spellObject = Instantiate(GameAssetsManager.Instance.GetSpellPrefab(skillInfo.spellName), attackChargeShootMuzzle.position, attackChargeShootMuzzle.rotation);
        spellObject.GetComponent<NetworkObject>().Spawn();
        skillInfo.ownerPlayerClientId = OwnerClientId;
        spellObject.GetComponent<SlashSkill>().InitSkillInfoDetail(skillInfo);
        spellObject.GetComponent<SlashSkill>().SetSelfDestroy();
        // 위치 설정
        spellObject.transform.SetParent(GameManager.Instance.transform);
        spellObject.transform.position = attackChargeShootMuzzle.position;
        // 플레이어가 보고있는 방향과 발사체가 바라보는 방향 일치시키기
        spellObject.transform.forward = transform.forward;
        float moveSpeed = 10f;
        spellObject.GetComponent<Rigidbody>().AddForce(spellObject.transform.forward * moveSpeed, ForceMode.Impulse);

        // State 업데이트 쿨타임은 attack1과 다르게 가야하기 때문에 인덱스를 신경써줍니다
        UpdatePlayerSpellState(2, SpellState.Cooltime);

        // Anim 실행
        playerAnimator.UpdateKnightMaleAnimationOnServer(KnightMaleAnimState.AttackVertical);

        // SFX 실행
        SoundManager.Instance?.PlayKnightSkillSFX(SkillName.ElectricSlashAttackChargeSlash_Lv1, SFX_Type.Shooting, transform);
    }

    /// <summary>
    /// 방어 마법 시전
    /// </summary>
    /// <param name="player"></param>
    [ServerRpc(RequireOwnership = false)]
    public void StartDefenceSkillServerRPC()
    {
        // 마법 시전
        Dash();

        // State 업데이트
        UpdatePlayerSpellState(DEFENCE_SPELL_INDEX_DEFAULT, SpellState.Cooltime);

        // Anim 실행
        playerAnimator.UpdateKnightMaleAnimationOnServer(KnightMaleAnimState.Dash);

        // SFX 실행
        SoundManager.Instance?.PlayKnightSkillSFX(SkillName.Dash_Lv1, SFX_Type.Shooting, transform);

        // 스킬 이펙트 생성
        /*        SpellInfo skillInfo = new SpellInfo(GetSpellInfo((ushort)3));
                GameObject spellObject = Instantiate(GameAssetsManager.Instance.GetSpellPrefab(skillInfo.spellName), transform.position, Quaternion.identity);
                spellObject.GetComponent<NetworkObject>().Spawn();

                Destroy(spellObject, 3f);
                // 위치 설정
                spellObject.transform.SetParent(transform);
                spellObject.transform.localPosition = Vector3.zero;*/
    }

    private void Dash()
    {
        // 대쉬 방향 설정 (예시로 앞으로 대쉬)
        Vector3 dashDirection = transform.forward;

        // 대쉬 동작
        StartCoroutine(PerformDash(dashDirection));
    }

    private IEnumerator PerformDash(Vector3 dashDirection)
    {
        float startTime = Time.time;
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + dashDirection * dashDistance;

        // 대쉬 동작
        while (Time.time < startTime + dashDuration)
        {
            float t = (Time.time - startTime) / dashDuration;
            transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        // 대쉬 끝나면 다시 캐릭터 애니메이션 Idle로 
        playerAnimator.UpdateKnightMaleAnimationOnServer(KnightMaleAnimState.Idle);
    }
}
