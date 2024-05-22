using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class WizardRukeAISpellManagerServer : NetworkBehaviour
{
    // SpellInfoList�� �ε��� 0~2������ ���ݸ���, 3�� ���� �Դϴ�.
    public const byte DEFENCE_SPELL_INDEX_DEFAULT = 3;

    public PlayerAnimator playerAnimator;

    [Header("���� �߻� ��ġ ����")]
    public Transform muzzlePos_Normal;
    public Transform muzzlePos_AoE;

    private List<SpellInfo> playerOwnedSpellInfoListOnServer = new List<SpellInfo>();
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
    /// Blizzard ��ų ���� ĳ���� �޼���
    /// </summary>
    /// <param name="spellIndex"></param>
    [ServerRpc(RequireOwnership = false)]
    public void CastingBlizzardServerRPC()
    {
        // ���� ǥ�� ������Ʈ ����
        GameObject spellObject = Instantiate(GameAssetsManager.Instance.GetSpellPrefab(SkillName.BlizzardLv1_Ready), muzzlePos_AoE.position, Quaternion.identity);
        spellObject.GetComponent<NetworkObject>().Spawn();

        spellObject.transform.SetParent(transform);
        // ������ �߻�ü ��ġ��Ű��
        spellObject.transform.localPosition = muzzlePos_AoE.localPosition;

        // �÷��̾ �����ִ� ����� �߻�ü�� �ٶ󺸴� ���� ��ġ��Ű��
        spellObject.transform.forward = transform.forward;

        // �÷��̾ �������� ������ �����ϱ�
        playerCastingSpell = spellObject;

        // �ش� �÷��̾��� ���� SpellState ������Ʈ
        UpdatePlayerSpellState(2, SpellState.Aiming);

        // ĳ���� �ִϸ��̼� ����
        playerAnimator.UpdateWizardMaleAnimationOnServer(WizardMaleAnimState.CastingAttackMagic);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetBlizzardServerRPC()
    {
        // 1. �������� ����ǥ�� ������Ʈ ����
        Destroy(playerCastingSpell);
        // 2. ���ڵ� ��ų ����Ʈ������Ʈ ����
        GameObject spellObject = Instantiate(GameAssetsManager.Instance.GetSpellPrefab(SkillName.BlizzardLv1), muzzlePos_AoE.position, Quaternion.identity);
        spellObject.GetComponent<NetworkObject>().Spawn();
        if (spellObject.TryGetComponent<AoESpell>(out var aoESpell))
        {
            aoESpell.SetOwner(OwnerClientId);
        }

        Destroy(spellObject, 4f);
        // �ش� SpellState ������Ʈ
        UpdatePlayerSpellState(2, SpellState.Cooltime);
        spellObject.transform.SetParent(GameManager.Instance.transform);

        // �߻� �ִϸ��̼� ����
        playerAnimator.UpdateWizardMaleAnimationOnServer(WizardMaleAnimState.ShootingMagic);
    }


    /// <summary>
    /// ���� ���� �������ֱ�. ĳ���� ���� ( NetworkObject�� Server������ ���� �����մϴ� )
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void CastingSpellServerRPC(ushort spellIndex)
    {
        // �߻�ü ������Ʈ ����
        GameObject spellObject = Instantiate(GameAssetsManager.Instance.GetSpellPrefab(GetSpellInfo(spellIndex).spellName), muzzlePos_Normal.position, Quaternion.identity);
        spellObject.GetComponent<NetworkObject>().Spawn();

        // �߻�ü ���� �ʱ�ȭ ���ֱ�
        SpellInfo spellInfo = new SpellInfo(GetSpellInfo(spellIndex));
        spellObject.GetComponent<AttackSpell>().InitSpellInfoDetail(spellInfo);
        // ȣ�� �����̶�� ȣ�� ������ ������ ��� & �ӵ� ����
        if (spellObject.TryGetComponent<HomingMissile>(out var ex))
        {
            ex.SetOwner(OwnerClientId);
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
        float moveSpeed = spellObject.GetComponent<AttackSpell>().GetSpellInfo().moveSpeed;

        // ȣ�� �����̶�� ȣ�� ���� ó��
        if (spellObject.TryGetComponent<HomingMissile>(out var ex)) ex.StartHoming();
        // ��ġ ����
        else if (moveSpeed == 0)
        {

        }
        // ���� �߻� (�⺻ ���� ���� ����)
        else
        {
            spellObject.GetComponent<AttackSpell>().Shoot(spellObject.transform.forward * moveSpeed, ForceMode.Impulse);
        }

        // �߻� SFX ���� 
        SpellInfo spellInfo = new SpellInfo(GetSpellInfo(spellIndex));
        SoundManager.Instance?.PlayWizardSpellSFX(spellInfo.spellName, SFX_Type.Shooting, transform);

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

    #region SpellInfo
    [ServerRpc(RequireOwnership = false)]
    public void UpdatePlayerSpellStateServerRPC(ushort spellIndex, SpellState spellState, ServerRpcParams serverRpcParams = default)
    {
        playerOwnedSpellInfoListOnServer[spellIndex].spellState = spellState;
    }

    public void UpdatePlayerSpellState(ushort spellIndex, SpellState spellState)
    {
        playerOwnedSpellInfoListOnServer[spellIndex].spellState = spellState;    
    }

    /// <summary>
    /// Server������ ������ SpellInfo ����Ʈ �ʱ�ȭ �޼ҵ� �Դϴ�.
    /// �÷��̾� ���� ������ ȣ��˴ϴ�.
    /// </summary>
    public void InitPlayerSpellInfoArrayOnServer(SkillName[] skillNames)
    {
        List<SpellInfo> playerSpellInfoList = new List<SpellInfo>();
        foreach (SkillName spellName in skillNames)
        {
            SpellInfo spellInfo = new SpellInfo(SpellSpecifications.Instance.GetSpellDefaultSpec(spellName));
            spellInfo.ownerPlayerClientId = OwnerClientId;
            playerSpellInfoList.Add(spellInfo);
        }

        playerOwnedSpellInfoListOnServer = playerSpellInfoList;
    }

    public List<SpellInfo> GetSpellInfoList()
    {
        return playerOwnedSpellInfoListOnServer;
    }

    /// <summary>
    /// ���� client�� �������� Ư�� ������ ������ �˷��ִ� �޼ҵ� �Դϴ�.  
    /// </summary>
    /// <param name="spellName">�˰���� ������ �̸�</param>
    /// <returns></returns>
    public SpellInfo GetSpellInfo(SkillName spellName)
    {
        foreach (SpellInfo spellInfo in playerOwnedSpellInfoListOnServer)
        {
            if (spellInfo.spellName == spellName)
            {
                return spellInfo;
            }
        }

        return null;
    }

    public SpellInfo GetSpellInfo(ushort spellIndex)
    {
        if (playerOwnedSpellInfoListOnServer.Count <= spellIndex) return null;
        return playerOwnedSpellInfoListOnServer[spellIndex];
    }

    public void SetSpellInfo(ushort spellIndex, SpellInfo newSpellInfo)
    {
        if (playerOwnedSpellInfoListOnServer.Count <= spellIndex) return;
        playerOwnedSpellInfoListOnServer[spellIndex] = newSpellInfo;
    }

    /// <summary>
    /// -1�� ��ȯ�ϸ� ��ã�Ҵٴ� ��
    /// </summary>
    /// <param name="skillName"></param>
    /// <returns></returns>
    public int GetSpellIndexBySpellName(SkillName skillName)
    {
        int index = -1;

        for (int i = 0; i < playerOwnedSpellInfoListOnServer.Count; i++)
        {
            if (playerOwnedSpellInfoListOnServer[i].spellName.Equals(skillName))
            {
                index = i;
                break;
            }
        }
        return index;
    }
    #endregion
}
