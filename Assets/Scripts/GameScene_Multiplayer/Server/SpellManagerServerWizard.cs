using System.Collections;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Wizard ĳ������ ������ ���� �Ŵ��� ��ũ��Ʈ �Դϴ�.
/// Server Auth ������� ���� �ߵ��� �����մϴ�. 
/// </summary>
public class SpellManagerServerWizard : SpellManagerServer
{
    public Transform muzzlePos_Normal;
    public Transform muzzlePos_AoE;

    private GameObject playerCastingSpell;

    #region Defence Spell Cast
    /// <summary>
    /// ��� ���� ����
    /// </summary>
    /// <param name="player"></param>
    [ServerRpc(RequireOwnership = false)]
    public void ActivateShieldServerRPC()
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
        StartCoroutine(StartAndResetAnimState(spellObject.GetComponent<DefenceSpell>().GetSpellInfo().lifetime));

        // ��� ���� ó��
        tag = "Invincible";
    }

    IEnumerator StartAndResetAnimState(float lifeTime)
    {
        if (playerAnimator == null) yield break;

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
    /// Blizzard ��ų ���� ĳ���� �޼���
    /// </summary>
    /// <param name="spellIndex"></param>
    [ServerRpc (RequireOwnership = false)]
    public void CastingBlizzardServerRPC()
    {
        if (playerAnimator == null) return;

        // ���� ǥ�� ������Ʈ ����
        GameObject spellObject = Instantiate(GameAssetsManager.Instance.GetSpellPrefab(SpellName.BlizzardLv1_Ready), muzzlePos_AoE.position, Quaternion.identity);
        spellObject.GetComponent<NetworkObject>().Spawn();

        spellObject.transform.SetParent(transform);
        // ������ �߻�ü ��ġ��Ű��
        spellObject.transform.localPosition = muzzlePos_AoE.localPosition;

        // �÷��̾ �����ִ� ����� �߻�ü�� �ٶ󺸴� ���� ��ġ��Ű��
        spellObject.transform.forward = transform.forward;

        // �÷��̾ �������� ������ �����ϱ�
        playerCastingSpell = spellObject;

        // �ش� �÷��̾��� ���� SpellState ������Ʈ
        UpdatePlayerSpellState(2, SpellState.Casting);

        // ĳ���� �ִϸ��̼� ����
        playerAnimator.UpdateWizardMaleAnimationOnServer(WizardMaleAnimState.CastingAttackMagic);
    }

    [ServerRpc (RequireOwnership = false)]
    public void ReleaseBlizzardServerRPC(ServerRpcParams serverRpcParams = default)
    {
        if (playerAnimator == null) return;
        if (GetSpellInfo(2) == null) return; // �ӽ÷� ���ڵ� ��ų�� �����ε���2������ �����մϴ�.

        // 1. �������� ����ǥ�� ������Ʈ ����
        Destroy(playerCastingSpell);
        // 2. ���ڵ� ��ų ����Ʈ������Ʈ ����
        GameObject spellObject = Instantiate(GameAssetsManager.Instance.GetSpellPrefab(SpellName.BlizzardLv1), muzzlePos_AoE.position, Quaternion.identity);
        spellObject.GetComponent<NetworkObject>().Spawn();
        if (spellObject.TryGetComponent<AoESpell>(out var aoESpell))
        {
            aoESpell.SetOwner(OwnerClientId, gameObject);
            aoESpell.InitAoESpell(GetSpellInfo(2));
        }

        // �ش� SpellState ������Ʈ
        UpdatePlayerSpellState(2, SpellState.Cooltime);
        spellObject.transform.SetParent(MultiplayerGameManager.Instance.transform);

        // �߻� �ִϸ��̼� ����
        playerAnimator.UpdateWizardMaleAnimationOnServer(WizardMaleAnimState.ShootingMagic);
    }


    /// <summary>
    /// ���� ���� �������ֱ�. ĳ���� ���� ( NetworkObject�� Server������ ���� �����մϴ� )
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void CastingNormalSpellServerRPC(ushort spellIndex)
    {
        if (playerAnimator == null) return;
        SpellInfo spellInfo = GetSpellInfo(spellIndex);
        if (spellInfo == null) return;  

        // �߻�ü ������Ʈ ����
        GameObject spellObject = Instantiate(GameAssetsManager.Instance.GetSpellPrefab(spellInfo.spellName), muzzlePos_Normal.position, Quaternion.identity);
        spellObject.GetComponent<NetworkObject>().Spawn();

        // �߻�ü ���� �ʱ�ȭ ���ֱ�
        spellObject.GetComponent<AttackSpell>().InitSpellInfoDetail(spellInfo, gameObject);
        // ȣ�� �����̶�� ȣ�� ������ ������ ��� & �ӵ� ����
        if (spellObject.TryGetComponent<HomingMissile>(out var ex))
        {
            ex.SetOwner(OwnerClientId, gameObject);
            ex.SetSpeed(spellInfo.moveSpeed);
        }
        spellObject.transform.SetParent(transform);

        // ������ �߻�ü ��ġ��Ű��
        spellObject.transform.localPosition = muzzlePos_Normal.localPosition;

        // ���� ���� SFX ����
        SoundManager.Instance?.PlayWizardSpellSFX(spellInfo.spellName, SFX_Type.Aiming, transform);

        // �÷��̾ �����ִ� ����� �߻�ü�� �ٶ󺸴� ���� ��ġ��Ű��
        spellObject.transform.forward = transform.forward;

        // �÷��̾ �������� ������ �����ϱ�
        playerCastingSpell = spellObject;

        // �ش� �÷��̾��� ���� SpellState ������Ʈ
        UpdatePlayerSpellState(spellIndex, SpellState.Casting);

        // ĳ���� �ִϸ��̼� ����
        playerAnimator.UpdateWizardMaleAnimationOnServer(WizardMaleAnimState.CastingAttackMagic);
    }

    /// <summary>
    /// �÷��̾��� ��û���� ���� ĳ�������� ��� Ÿ�� ������ �߻��ϴ� �޼ҵ� �Դϴ�.
    /// </summary>
    /// <param name="serverRpcParams"></param>
    [ServerRpc(RequireOwnership = false)]
    public void ReleaseNormalSpellServerRPC(ushort spellIndex, ServerRpcParams serverRpcParams = default)
    {
        if (playerCastingSpell==null) return;
        if (playerAnimator==null) return;

        // �ش� SpellState ������Ʈ
        UpdatePlayerSpellState(spellIndex, SpellState.Cooltime);

        playerCastingSpell.transform.SetParent(MultiplayerGameManager.Instance.transform);
        float moveSpeed = playerCastingSpell.GetComponent<AttackSpell>().GetSpellInfo().moveSpeed;

        // ȣ�� �����̶�� ȣ�� ���� ó��
        if (playerCastingSpell.TryGetComponent<HomingMissile>(out var ex)) ex.StartHoming();
        // ��ġ ����
        else if (moveSpeed == 0) {
            
        }
        // ���� �߻� (�⺻ ���� ���� ����)
        else
        {
            playerCastingSpell.GetComponent<AttackSpell>().Shoot(playerCastingSpell.transform.forward * moveSpeed, ForceMode.Impulse);
        }

        // �߻� SFX ���� 
        SpellInfo spellInfo = GetSpellInfo(spellIndex);
        SoundManager.Instance?.PlayWizardSpellSFX(spellInfo.spellName, SFX_Type.Shooting, transform);

        // ���� VFX
        MuzzleVFX(playerCastingSpell.GetComponent<AttackSpell>().GetMuzzleVFXPrefab(), GetComponentInChildren<MuzzlePos>().transform);

        // �߻� �ִϸ��̼� ����
        playerAnimator.UpdateWizardMaleAnimationOnServer(WizardMaleAnimState.ShootingMagic);
    }
    #endregion

    #region Spell VFX
    // ���� VFX 
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
