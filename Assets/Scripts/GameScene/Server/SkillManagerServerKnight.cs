using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SkillManagerServerKnight : SkillSpellManagerServer
{
    // 대쉬 스킬 정보를 일단 하드코딩으로 넣어뒀습니다. 추후 구글시트와 JSON을 활용한 연결을 할 때 수정하면 됩니다.
    public float dashDistance = 10f; // 대쉬 거리
    public float dashDuration = 0.3f; // 대쉬 지속 시간

    // 스킬 시전 위치
    public Transform attack1Muzzle;
    public Transform attack2Muzzle;

    // 게임패트UI 스킬 버튼 드래그도중 조준이 가능하도록 하기 위한 메서드
    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerSpellStateToCastingServerRPC(ushort spellIndex)
    {
        UpdatePlayerSpellState(spellIndex, SpellState.Casting);
    }

    [ServerRpc(RequireOwnership = false)]
    public void StartAttackSkillServerRPC(ushort skillIndex)
    {
        // Knight_male의 세 번째 스킬은 첫 번째 스킬과 동일합니다. (전기베기1)
        // 마법 시전
        switch (skillIndex)
        {
            case 0:
                Attack1();
                break;
            case 1:
                Attack2();
                break;
            case 2:
                Attack3();
                break;
            default: 
                break;
        }
    }

    private void Attack1()
    {
        // 스킬 이펙트 생성
        SpellInfo skillInfo = new SpellInfo(GetSpellInfo((ushort)0));
        GameObject spellObject = Instantiate(GameAssets.instantiate.GetSpellPrefab(skillInfo.spellName), attack1Muzzle.position, Quaternion.identity);
        spellObject.GetComponent<NetworkObject>().Spawn();
        skillInfo.ownerPlayerClientId = OwnerClientId;
        spellObject.GetComponent<SlashSkill>().InitSkillInfoDetail(skillInfo);
        spellObject.GetComponent<SlashSkill>().SetSelfDestroy();
        spellObject.transform.SetParent(transform);
        // 위치 설정
        spellObject.transform.localPosition = attack1Muzzle.localPosition;
        spellObject.transform.localRotation = attack1Muzzle.localRotation;

        // State 업데이트
        UpdatePlayerSpellState(0, SpellState.Cooltime);

        // Anim 실행
        playerAnimator.UpdateKnightMaleAnimationOnServer(KnightMaleAnimState.Attack1);

        // SFX 실행
        spellObject.GetComponent<SlashSkill>().PlaySFX(SlashSkill.SFX_SHOOTING);
    }

    private void Attack2()
    {
        // 스킬 이펙트 생성
        SpellInfo skillInfo = new SpellInfo(GetSpellInfo((ushort)1));
        GameObject spellObject = Instantiate(GameAssets.instantiate.GetSpellPrefab(skillInfo.spellName), attack2Muzzle.position, Quaternion.identity);
        spellObject.GetComponent<NetworkObject>().Spawn();
        skillInfo.ownerPlayerClientId = OwnerClientId;
        spellObject.GetComponent<SlashSkill>().InitSkillInfoDetail(skillInfo);
        spellObject.GetComponent<SlashSkill>().SetSelfDestroy();
        spellObject.transform.SetParent(transform);
        // 위치 설정
        spellObject.transform.localPosition = attack2Muzzle.localPosition;
        spellObject.transform.localRotation = attack2Muzzle.localRotation;

        // State 업데이트
        UpdatePlayerSpellState(1, SpellState.Cooltime);

        // Anim 실행
        playerAnimator.UpdateKnightMaleAnimationOnServer(KnightMaleAnimState.Attack2);

        // SFX 실행
        spellObject.GetComponent<SlashSkill>().PlaySFX(SlashSkill.SFX_SHOOTING);
    }

    private void Attack3()
    {
        // 스킬 이펙트 생성
        SpellInfo skillInfo = new SpellInfo(GetSpellInfo((ushort)1));
        GameObject spellObject = Instantiate(GameAssets.instantiate.GetSpellPrefab(skillInfo.spellName), attack1Muzzle.position, Quaternion.identity);
        spellObject.GetComponent<NetworkObject>().Spawn();
        skillInfo.ownerPlayerClientId = OwnerClientId;
        spellObject.GetComponent<SlashSkill>().InitSkillInfoDetail(skillInfo);
        spellObject.GetComponent<SlashSkill>().SetSelfDestroy();
        spellObject.transform.SetParent(transform);
        // 위치 설정
        spellObject.transform.localPosition = attack1Muzzle.localPosition;
        spellObject.transform.localRotation = attack1Muzzle.localRotation;

        // State 업데이트 쿨타임은 attack1과 다르게 가야하기 때문에 인덱스를 신경써줍니다
        UpdatePlayerSpellState(2, SpellState.Cooltime);

        // Anim 실행
        playerAnimator.UpdateKnightMaleAnimationOnServer(KnightMaleAnimState.Attack1);

        // SFX 실행
        spellObject.GetComponent<SlashSkill>().PlaySFX(SlashSkill.SFX_SHOOTING);
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
