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
    public Transform attackVerticalMuzzle;
    public Transform attackWhirlwindMuzzle;
    public Transform attackChargeReadyMuzzle;
    public Transform attackChargeShootMuzzle;

    private GameObject chargeEffectObject;

    [ServerRpc(RequireOwnership = false)]
    public void ReadyAttackSkillServerRPC(ushort skillIndex)
    {
        // ������ƮUI ��ų ��ư �巡�׵��� ������ �����ϵ��� �ϱ� ���� ó��
        UpdatePlayerSpellState(skillIndex, SpellState.Aiming);

        // �غ��ڼ� �ִϸ��̼� ����
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
        // Knight_male�� �� ��° ��ų�� ù ��° ��ų�� �����մϴ�. (���⺣��1)
        // ���� ����
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
        // ��ų ����Ʈ ����
        SpellInfo skillInfo = new SpellInfo(GetSpellInfo((ushort)0));
        GameObject spellObject = Instantiate(GameAssetsManager.Instance.GetSpellPrefab(skillInfo.spellName), attackVerticalMuzzle.position, Quaternion.identity);
        spellObject.GetComponent<NetworkObject>().Spawn();
        skillInfo.ownerPlayerClientId = OwnerClientId;
        spellObject.GetComponent<SlashSkill>().InitSkillInfoDetail(skillInfo);
        spellObject.GetComponent<SlashSkill>().SetSelfDestroy();

        // ��ġ ����
        spellObject.transform.SetParent(transform);
        spellObject.transform.localPosition = attackVerticalMuzzle.localPosition;
        spellObject.transform.localRotation = attackVerticalMuzzle.localRotation;

        // State ������Ʈ
        UpdatePlayerSpellState(0, SpellState.Cooltime);

        // Anim ����
        playerAnimator.UpdateKnightMaleAnimationOnServer(KnightMaleAnimState.AttackVertical);

        // SFX ����
        spellObject.GetComponent<SlashSkill>().PlaySFX(SlashSkill.SFX_SHOOTING);
    }

    private void AttackWhirlwind()
    {
        // ��ų ����Ʈ ����
        SpellInfo skillInfo = new SpellInfo(GetSpellInfo((ushort)1));
        GameObject spellObject = Instantiate(GameAssetsManager.Instance.GetSpellPrefab(skillInfo.spellName), attackWhirlwindMuzzle.position, Quaternion.identity);
        spellObject.GetComponent<NetworkObject>().Spawn();
        skillInfo.ownerPlayerClientId = OwnerClientId;
/*        spellObject.GetComponent<SlashSkill>().InitSkillInfoDetail(skillInfo);
        spellObject.GetComponent<SlashSkill>().SetSelfDestroy();*/

        // ��ġ ����
        spellObject.transform.SetParent(transform);
        spellObject.transform.localPosition = attackWhirlwindMuzzle.localPosition;

        // State ������Ʈ
        UpdatePlayerSpellState(1, SpellState.Cooltime);

        // Anim ����
        playerAnimator.UpdateKnightMaleAnimationOnServer(KnightMaleAnimState.AttackWhirlwind);

        // SFX ����
        //spellObject.GetComponent<SlashSkill>().PlaySFX(SlashSkill.SFX_SHOOTING);
    }

    private void AttackChargeReady()
    {
        // ��ų ����Ʈ ����
        chargeEffectObject = Instantiate(GameAssetsManager.Instance.GetSpellPrefab(SkillName.ElectricSlashAttackCharge_Lv1), attackChargeReadyMuzzle.position, Quaternion.identity);
        chargeEffectObject.GetComponent<NetworkObject>().Spawn();
        // ��ġ ����
        chargeEffectObject.transform.SetParent(transform);
        chargeEffectObject.transform.localPosition = attackChargeReadyMuzzle.localPosition;

        // ��¡ �����ε�, �ϴ� �ִϸ��̼� ���ΰ����̶� ���� ���� ����.
        playerAnimator.UpdateKnightMaleAnimationOnServer(KnightMaleAnimState.AttackVerticalReady);

        // SFX ���� (���� ȿ���� �ʿ�)
        //spellObject.GetComponent<SlashSkill>().PlaySFX(SlashSkill.SFX_SHOOTING);
    }

    private void AttackChargeShoot()
    {
        // ���� ����Ʈ ������Ʈ ����
        Destroy(chargeEffectObject);

        // ��ų ����Ʈ ����
        SpellInfo skillInfo = new SpellInfo(GetSpellInfo((ushort)2));
        GameObject spellObject = Instantiate(GameAssetsManager.Instance.GetSpellPrefab(skillInfo.spellName), attackChargeShootMuzzle.position, Quaternion.identity);
        spellObject.GetComponent<NetworkObject>().Spawn();
        skillInfo.ownerPlayerClientId = OwnerClientId;
/*        spellObject.GetComponent<SlashSkill>().InitSkillInfoDetail(skillInfo);
        spellObject.GetComponent<SlashSkill>().SetSelfDestroy();*/
        // ��ġ ����
        spellObject.transform.position = attackChargeShootMuzzle.position;
/*        spellObject.transform.localPosition = attackChargeShootMuzzle.localPosition;
        spellObject.transform.localRotation = attackChargeShootMuzzle.localRotation;*/

        // State ������Ʈ ��Ÿ���� attack1�� �ٸ��� �����ϱ� ������ �ε����� �Ű���ݴϴ�
        UpdatePlayerSpellState(2, SpellState.Cooltime);

        // Anim ����
        playerAnimator.UpdateKnightMaleAnimationOnServer(KnightMaleAnimState.AttackVertical);

        // SFX ����
        //spellObject.GetComponent<SlashSkill>().PlaySFX(SlashSkill.SFX_SHOOTING);
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
        // ��ų ����Ʈ ����
        SpellInfo skillInfo = new SpellInfo(GetSpellInfo((ushort)3));
        GameObject spellObject = Instantiate(GameAssetsManager.Instance.GetSpellPrefab(skillInfo.spellName), transform.position, Quaternion.identity);
        spellObject.GetComponent<NetworkObject>().Spawn();

        Destroy(spellObject, 3f);
        // ��ġ ����
        spellObject.transform.SetParent(transform);
        spellObject.transform.localPosition = Vector3.zero;
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
