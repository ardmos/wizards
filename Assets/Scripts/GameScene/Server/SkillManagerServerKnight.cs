using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SkillManagerServerKnight : SkillSpellManagerServer
{
    // �뽬 ��ų ������ �ϴ� �ϵ��ڵ����� �־�׽��ϴ�. ���� ���۽�Ʈ�� JSON�� Ȱ���� ������ �� �� �����ϸ� �˴ϴ�.
    public float dashDistance = 10f; // �뽬 �Ÿ�
    public float dashDuration = 0.3f; // �뽬 ���� �ð�

    // ��ų ���� ��ġ
    public Transform attack1Muzzle;
    public Transform attack2Muzzle;

    // ������ƮUI ��ų ��ư �巡�׵��� ������ �����ϵ��� �ϱ� ���� �޼���
    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerSpellStateToCastingServerRPC(ushort spellIndex)
    {
        UpdatePlayerSpellState(spellIndex, SpellState.Casting);
    }

    [ServerRpc(RequireOwnership = false)]
    public void StartAttackSkillServerRPC(ushort skillIndex)
    {
        // Knight_male�� �� ��° ��ų�� ù ��° ��ų�� �����մϴ�. (���⺣��1)
        // ���� ����
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
        // ��ų ����Ʈ ����
        SpellInfo skillInfo = new SpellInfo(GetSpellInfo((ushort)0));
        GameObject spellObject = Instantiate(GameAssets.instantiate.GetSpellPrefab(skillInfo.spellName), attack1Muzzle.position, Quaternion.identity);
        spellObject.GetComponent<NetworkObject>().Spawn();
        skillInfo.ownerPlayerClientId = OwnerClientId;
        spellObject.GetComponent<SlashSkill>().InitSkillInfoDetail(skillInfo);
        spellObject.GetComponent<SlashSkill>().SetSelfDestroy();
        spellObject.transform.SetParent(transform);
        // ��ġ ����
        spellObject.transform.localPosition = attack1Muzzle.localPosition;
        spellObject.transform.localRotation = attack1Muzzle.localRotation;

        // State ������Ʈ
        UpdatePlayerSpellState(0, SpellState.Cooltime);

        // Anim ����
        playerAnimator.UpdateKnightMaleAnimationOnServer(KnightMaleAnimState.Attack1);

        // SFX ����
        spellObject.GetComponent<SlashSkill>().PlaySFX(SlashSkill.SFX_SHOOTING);
    }

    private void Attack2()
    {
        // ��ų ����Ʈ ����
        SpellInfo skillInfo = new SpellInfo(GetSpellInfo((ushort)1));
        GameObject spellObject = Instantiate(GameAssets.instantiate.GetSpellPrefab(skillInfo.spellName), attack2Muzzle.position, Quaternion.identity);
        spellObject.GetComponent<NetworkObject>().Spawn();
        skillInfo.ownerPlayerClientId = OwnerClientId;
        spellObject.GetComponent<SlashSkill>().InitSkillInfoDetail(skillInfo);
        spellObject.GetComponent<SlashSkill>().SetSelfDestroy();
        spellObject.transform.SetParent(transform);
        // ��ġ ����
        spellObject.transform.localPosition = attack2Muzzle.localPosition;
        spellObject.transform.localRotation = attack2Muzzle.localRotation;

        // State ������Ʈ
        UpdatePlayerSpellState(1, SpellState.Cooltime);

        // Anim ����
        playerAnimator.UpdateKnightMaleAnimationOnServer(KnightMaleAnimState.Attack2);

        // SFX ����
        spellObject.GetComponent<SlashSkill>().PlaySFX(SlashSkill.SFX_SHOOTING);
    }

    private void Attack3()
    {
        // ��ų ����Ʈ ����
        SpellInfo skillInfo = new SpellInfo(GetSpellInfo((ushort)1));
        GameObject spellObject = Instantiate(GameAssets.instantiate.GetSpellPrefab(skillInfo.spellName), attack1Muzzle.position, Quaternion.identity);
        spellObject.GetComponent<NetworkObject>().Spawn();
        skillInfo.ownerPlayerClientId = OwnerClientId;
        spellObject.GetComponent<SlashSkill>().InitSkillInfoDetail(skillInfo);
        spellObject.GetComponent<SlashSkill>().SetSelfDestroy();
        spellObject.transform.SetParent(transform);
        // ��ġ ����
        spellObject.transform.localPosition = attack1Muzzle.localPosition;
        spellObject.transform.localRotation = attack1Muzzle.localRotation;

        // State ������Ʈ ��Ÿ���� attack1�� �ٸ��� �����ϱ� ������ �ε����� �Ű���ݴϴ�
        UpdatePlayerSpellState(2, SpellState.Cooltime);

        // Anim ����
        playerAnimator.UpdateKnightMaleAnimationOnServer(KnightMaleAnimState.Attack1);

        // SFX ����
        spellObject.GetComponent<SlashSkill>().PlaySFX(SlashSkill.SFX_SHOOTING);
    }

    /// <summary>
    /// ��� ���� ����
    /// </summary>
    /// <param name="player"></param>
    [ServerRpc(RequireOwnership = false)]
    public void StartDefenceSkillServerRPC()
    {
        // ���� ����
        Dash();

        // State ������Ʈ
        UpdatePlayerSpellState(DEFENCE_SPELL_INDEX_DEFAULT, SpellState.Cooltime);

        // Anim ����
        playerAnimator.UpdateKnightMaleAnimationOnServer(KnightMaleAnimState.Dash);

        // SFX ����
    }

    private void Dash()
    {
        // �뽬 ���� ���� (���÷� ������ �뽬)
        Vector3 dashDirection = transform.forward;

        // �뽬 ����
        StartCoroutine(PerformDash(dashDirection));
    }

    private IEnumerator PerformDash(Vector3 dashDirection)
    {
        float startTime = Time.time;
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + dashDirection * dashDistance;

        // �뽬 ����
        while (Time.time < startTime + dashDuration)
        {
            float t = (Time.time - startTime) / dashDuration;
            transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        // �뽬 ������ �ٽ� ĳ���� �ִϸ��̼� Idle�� 
        playerAnimator.UpdateKnightMaleAnimationOnServer(KnightMaleAnimState.Idle);
    }
}
