using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
/// <summary>
/// ������ Server Auth ������� ������ �� �ֵ��� �����ִ� ��ũ��Ʈ �Դϴ�.
/// Scroll�� ȹ��� ������ ������ �����մϴ�.
/// �������� �����ϴ� ��ũ��Ʈ���� ���־�� �մϴ�. ���� �ڵ� �����ϸ鼭 Ȯ�� �ʿ�.
/// </summary>
public class SpellManagerServerWizard : NetworkBehaviour
{
    public SpellManagerClientWizard spellManagerClientWizard;
    public List<SpellInfo> playerOwnedSpellInfoListOnServer = new List<SpellInfo>();

    // Spell Dictionary that the player is casting.
    private GameObject playerCastingSpell;

    #region SpellInfo
    /// <summary>
    /// ����� SpellState�� Server�� �����մϴ�.
    /// </summary>
    /// <param name="spellIndex">������Ʈ�ϰ���� ������ Index</param>
    /// <param name="spellState">������ ����</param>
    [ServerRpc(RequireOwnership = false)]
    public void UpdatePlayerSpellStateServerRPC(ushort spellIndex, SpellState spellState, ServerRpcParams serverRpcParams = default)
    {

        playerOwnedSpellInfoListOnServer[spellIndex].spellState = spellState;

        // ��û�� Ŭ���̾�Ʈ�� playerOwnedSpellInfoList�� ����ȭ
        spellManagerClientWizard.UpdatePlayerSpellInfoArrayClientRPC(playerOwnedSpellInfoListOnServer.ToArray());
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
    #endregion

    #region Defence Spell Cast
    /// <summary>
    /// ��� ���� ����
    /// </summary>
    /// <param name="player"></param>
    [ServerRpc(RequireOwnership = false)]
    public void StartActivateDefenceSpellServerRPC(SkillName spellName, NetworkObjectReference player)
    {
        // GameObject ���� ���н� �α� ���
        if (!player.TryGet(out NetworkObject playerObject))
        {
            Debug.LogError("StartActivateDefenceSpellServerRPC Failed to Get NetworkObject from NetwrokObjectRefernce!");
            return;
        }
        ulong clientId = playerObject.OwnerClientId;

        // ���� ����
        GameObject spellObject = Instantiate(GameAssets.instantiate.GetSpellPrefab(spellName), playerObject.transform.position, Quaternion.identity);
        spellObject.GetComponent<NetworkObject>().Spawn();
        spellObject.GetComponent<DefenceSpell>().InitSpellInfoDetail(GetSpellInfo(spellName));
        spellObject.transform.SetParent(playerObject.transform);
        spellObject.transform.localPosition = Vector3.zero;
        spellObject.GetComponent<DefenceSpell>().Activate();
    }
    #endregion

    #region Attack Spell Cast&Fire
    /// <summary>
    /// ���� ���� �������ֱ�. ĳ���� ���� ( NetworkObject�� Server������ ���� �����մϴ� )
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void StartCastingAttackSpellServerRPC(SkillName spellName, NetworkObjectReference player)
    {
        // ���� ��ġ ã��(Local posittion)
        Transform muzzleTransform = GetComponentInChildren<MuzzlePos>().transform;

        // ������ �߻�ü ��ġ��Ű��
        GameObject spellObject = Instantiate(GameAssets.instantiate.GetSpellPrefab(spellName), muzzleTransform.position, Quaternion.identity);
        spellObject.GetComponent<NetworkObject>().Spawn();

        // �߻�ü ���� �ʱ�ȭ ���ֱ�
        SpellInfo spellInfo = new SpellInfo(GetSpellInfo(spellName));
        spellObject.GetComponent<AttackSpell>().InitSpellInfoDetail(spellInfo);
        spellObject.transform.SetParent(transform);
        spellObject.transform.localPosition = muzzleTransform.localPosition;

        // �÷��̾ �����ִ� ����� �߻�ü�� �ٶ󺸴� ���� ��ġ��Ű��
        spellObject.transform.forward = transform.forward;

        // �÷��̾ �������� ������ �����ϱ�
        playerCastingSpell = spellObject;
    }

    /// <summary>
    /// �÷��̾��� ��û���� ���� ĳ�������� ������ �߻��ϴ� �޼ҵ� �Դϴ�.
    /// </summary>
    /// <param name="serverRpcParams"></param>
    [ServerRpc(RequireOwnership = false)]
    public void ShootCastingSpellObjectServerRPC(ServerRpcParams serverRpcParams = default)
    {
        ulong clientId = serverRpcParams.Receive.SenderClientId;
        GameObject spellObject = playerCastingSpell;
        if (spellObject == null)
        {
            Debug.Log($"ShootCastingSpellObjectServerRPC : Wrong Request. Player{clientId} has no casting spell object.");
            return;
        }

        Debug.Log($"{nameof(ShootCastingSpellObjectServerRPC)} ownerClientId {clientId}");

        spellObject.transform.SetParent(GameManager.Instance.transform);

        // ���� �߻� (�⺻ ���� ���� ����)
        float moveSpeed = spellObject.GetComponent<AttackSpell>().GetSpellInfo().moveSpeed;
        spellObject.GetComponent<AttackSpell>().Shoot(spellObject.transform.forward * moveSpeed, ForceMode.Impulse);

        // ���� VFX
        MuzzleVFX(spellObject.GetComponent<AttackSpell>().GetMuzzleVFXPrefab(), GetComponentInChildren<MuzzlePos>().transform);
    }
    #endregion

    #region Spell VFX
    // ���� VFX 
    public void MuzzleVFX(GameObject muzzleVFXPrefab, Transform muzzleTransform)
    {
        if (muzzleVFXPrefab == null)
        {
            Debug.Log($"MuzzleVFX muzzleVFXPrefab is null");
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
