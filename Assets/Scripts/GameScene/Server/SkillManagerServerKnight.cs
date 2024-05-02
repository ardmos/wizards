using System.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class SkillManagerServerKnight : SkillSpellManagerServer
{
    // �뽬 ��ų ������ �ϴ� �ϵ��ڵ����� �־�׽��ϴ�. ���� ���۽�Ʈ�� JSON�� Ȱ���� ������ �� �� �����ϸ� �˴ϴ�.
    public float dashDistance = 25f; // �뽬 �Ÿ�
    public float dashDuration = 0.05f; // �뽬 ���� �ð�

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
        Debug.Log("1.StartAttackSkillServerRPC");
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
        GameObject spellObject = Instantiate(GameAssetsManager.Instance.GetSpellPrefab(skillInfo.spellName), attackVerticalMuzzle.position, attackVerticalMuzzle.localRotation);
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
        SoundManager.Instance?.PlayKnightSkillSFX(SkillName.ElectricSlashAttackVertical_Lv1, SFX_Type.Shooting, transform);
    }

    private void AttackWhirlwind()
    {
        Debug.Log("2.AttackWhirlwind");
        // ��ų ����Ʈ ����
        SpellInfo skillInfo = new SpellInfo(GetSpellInfo((ushort)1));
        GameObject spellObject = Instantiate(GameAssetsManager.Instance.GetSpellPrefab(skillInfo.spellName), attackWhirlwindMuzzle.position, Quaternion.identity);
        spellObject.GetComponent<NetworkObject>().Spawn();
        skillInfo.ownerPlayerClientId = OwnerClientId;
        spellObject.GetComponent<SlashSkill>().InitSkillInfoDetail(skillInfo);
        spellObject.GetComponent<SlashSkill>().SetSelfDestroy();

        // ��ġ ����
        spellObject.transform.SetParent(transform);
        spellObject.transform.localPosition = attackWhirlwindMuzzle.localPosition;

        // State ������Ʈ
        UpdatePlayerSpellState(1, SpellState.Cooltime);

        // Anim ����
        playerAnimator.UpdateKnightMaleAnimationOnServer(KnightMaleAnimState.AttackWhirlwind);

        // SFX ����
        SoundManager.Instance?.PlayKnightSkillSFX(SkillName.ElectricSlashAttackWhirlwind_Lv1, SFX_Type.Shooting, transform);
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

        // SFX ����
        SoundManager.Instance?.PlayKnightSkillSFX(SkillName.ElectricSlashAttackChargeSlash_Lv1, SFX_Type.Aiming, transform);
    }

    private void AttackChargeShoot()
    {
        // ���� ����Ʈ ������Ʈ ����
        Destroy(chargeEffectObject);

        // ��ų ����Ʈ ����
        SpellInfo skillInfo = new SpellInfo(GetSpellInfo((ushort)2));
        GameObject spellObject = Instantiate(GameAssetsManager.Instance.GetSpellPrefab(skillInfo.spellName), attackChargeShootMuzzle.position, attackChargeShootMuzzle.rotation);
        spellObject.GetComponent<NetworkObject>().Spawn();
        skillInfo.ownerPlayerClientId = OwnerClientId;
        spellObject.GetComponent<SlashSkill>().InitSkillInfoDetail(skillInfo);
        spellObject.GetComponent<SlashSkill>().SetSelfDestroy();
        // ��ġ ����
        spellObject.transform.SetParent(GameManager.Instance.transform);
        spellObject.transform.position = attackChargeShootMuzzle.position;
        // �÷��̾ �����ִ� ����� �߻�ü�� �ٶ󺸴� ���� ��ġ��Ű��
        spellObject.transform.forward = transform.forward;
        float moveSpeed = 10f;
        spellObject.GetComponent<Rigidbody>().AddForce(spellObject.transform.forward * moveSpeed, ForceMode.Impulse);

        // State ������Ʈ ��Ÿ���� attack1�� �ٸ��� �����ϱ� ������ �ε����� �Ű���ݴϴ�
        UpdatePlayerSpellState(2, SpellState.Cooltime);

        // Anim ����
        playerAnimator.UpdateKnightMaleAnimationOnServer(KnightMaleAnimState.AttackVertical);

        // SFX ����
        SoundManager.Instance?.PlayKnightSkillSFX(SkillName.ElectricSlashAttackChargeSlash_Lv1, SFX_Type.Shooting, transform);
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
        SoundManager.Instance?.PlayKnightSkillSFX(SkillName.Dash_Lv1, SFX_Type.Shooting, transform);

        // ��ų ����Ʈ ����
        /*        SpellInfo skillInfo = new SpellInfo(GetSpellInfo((ushort)3));
                GameObject spellObject = Instantiate(GameAssetsManager.Instance.GetSpellPrefab(skillInfo.spellName), transform.position, Quaternion.identity);
                spellObject.GetComponent<NetworkObject>().Spawn();

                Destroy(spellObject, 3f);
                // ��ġ ����
                spellObject.transform.SetParent(transform);
                spellObject.transform.localPosition = Vector3.zero;*/
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
