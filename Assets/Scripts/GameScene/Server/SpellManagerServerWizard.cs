using System.Collections;
using Unity.Netcode;
using UnityEngine;
/// <summary>
/// ������ Server Auth ������� ������ �� �ֵ��� �����ִ� ��ũ��Ʈ �Դϴ�.
/// Scroll�� ȹ��� ������ ������ �����մϴ�.
/// �������� �����ϴ� ��ũ��Ʈ���� ���־�� �մϴ�. ���� �ڵ� �����ϸ鼭 Ȯ�� �ʿ�.
/// </summary>
public class SpellManagerServerWizard : SkillSpellManagerServer
{
    private GameObject playerCastingSpell;

    #region Defence Spell Cast
    /// <summary>
    /// ��� ���� ����
    /// </summary>
    /// <param name="player"></param>
    [ServerRpc(RequireOwnership = false)]
    public void StartActivateDefenceSpellServerRPC()
    {
        // ���� ����
        GameObject spellObject = Instantiate(GameAssetsManager.Instance.GetSpellPrefab(GetSpellInfo(DEFENCE_SPELL_INDEX_DEFAULT).spellName), transform.position, Quaternion.identity);
        spellObject.GetComponent<NetworkObject>().Spawn();
        spellObject.GetComponent<DefenceSpell>().InitSpellInfoDetail(GetSpellInfo(DEFENCE_SPELL_INDEX_DEFAULT));
        spellObject.transform.SetParent(transform);
        spellObject.transform.localPosition = Vector3.zero;
        spellObject.GetComponent<DefenceSpell>().Activate();

        // ���� ���� ���� ���
        spellObject.GetComponent<DefenceSpell>().PlaySFX(SFX_Type.Aiming);

        // �ش� SpellState ������Ʈ
        UpdatePlayerSpellState(DEFENCE_SPELL_INDEX_DEFAULT, SpellState.Cooltime);

        // �ִϸ��̼� ����
        StartCoroutine(StartAndResetAnimState(spellObject.GetComponent<DefenceSpell>().GetSpellInfo().lifeTime));

        // ��� ���� ó��
        tag = "Invincible";
    }

    IEnumerator StartAndResetAnimState(float lifeTime)
    {
        playerAnimator.UpdateWizardMaleAnimationOnServer(WizardMaleAnimState.CastingDefensiveMagic);
        yield return new WaitForSeconds(lifeTime);

        // ���� ����
        tag = "Player";

        // �÷��̾� ĳ���Ͱ� Casting �ִϸ��̼����� �ƴ� ��쿡�� Idle�� ����
        if (!playerAnimator.playerAttackAnimState.Equals(WizardMaleAnimState.CastingAttackMagic))
        {
            playerAnimator.UpdateWizardMaleAnimationOnServer(WizardMaleAnimState.Idle);
        }
    }
    #endregion

    #region Attack Spell Cast&Fire
    /// <summary>
    /// ���� ���� �������ֱ�. ĳ���� ���� ( NetworkObject�� Server������ ���� �����մϴ� )
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void CastingSpellServerRPC(ushort spellIndex)
    {
        // ���� ��ġ ã��(Local posittion)
        Transform muzzleTransform = GetComponentInChildren<MuzzlePos>().transform;

        // �߻�ü ������Ʈ ����
        GameObject spellObject = Instantiate(GameAssetsManager.Instance.GetSpellPrefab(GetSpellInfo(spellIndex).spellName), muzzleTransform.position, Quaternion.identity);
        spellObject.GetComponent<NetworkObject>().Spawn();

        // �߻�ü ���� �ʱ�ȭ ���ֱ�
        SpellInfo spellInfo = new SpellInfo(GetSpellInfo(spellIndex));
        spellObject.GetComponent<AttackSpell>().InitSpellInfoDetail(spellInfo);
        spellObject.transform.SetParent(transform);
        // ������ �߻�ü ��ġ��Ű��
        spellObject.transform.localPosition = muzzleTransform.localPosition;

        // ���� ���� ���� ���
        spellObject.GetComponent<AttackSpell>().PlaySFX(SFX_Type.Aiming);

        // �÷��̾ �����ִ� ����� �߻�ü�� �ٶ󺸴� ���� ��ġ��Ű��
        spellObject.transform.forward = transform.forward;

        // �÷��̾ �������� ������ �����ϱ�
        playerCastingSpell = spellObject;

        // �ش� �÷��̾��� ���� SpellState ������Ʈ
        UpdatePlayerSpellState(spellIndex, SpellState.Aiming);

        // ĳ���� �ִϸ��̼� ����
        playerAnimator.UpdateWizardMaleAnimationOnServer(WizardMaleAnimState.CastingAttackMagic);
    }

    /// <summary>
    /// �÷��̾��� ��û���� ���� ĳ�������� ������ �߻��ϴ� �޼ҵ� �Դϴ�.
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

        // �ش� SpellState ������Ʈ
        UpdatePlayerSpellState(spellIndex, SpellState.Cooltime);

        //Debug.Log($"{nameof(ShootSpellServerRPC)} ownerClientId {clientId}");

        spellObject.transform.SetParent(GameManager.Instance.transform);

        // ���� �߻� (�⺻ ���� ���� ����)
        float moveSpeed = spellObject.GetComponent<AttackSpell>().GetSpellInfo().moveSpeed;
        spellObject.GetComponent<AttackSpell>().Shoot(spellObject.transform.forward * moveSpeed, ForceMode.Impulse);

        // ���� VFX
        MuzzleVFX(spellObject.GetComponent<AttackSpell>().GetMuzzleVFXPrefab(), GetComponentInChildren<MuzzlePos>().transform);

        // �߻� �ִϸ��̼� ����
        playerAnimator.UpdateWizardMaleAnimationOnServer(WizardMaleAnimState.ShootingMagic);
    }
    #endregion

    #region Spell VFX
    // ���� VFX 
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
