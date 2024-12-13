using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// ���� ������ ��ų�� ������ �����ϴ� Ŭ�����Դϴ�.
/// </summary>
public class SpellManagerServer : NetworkBehaviour
{
    #region Constants
    // SpellInfoList�� �ε��� 0~2������ ���ݸ���, 3�� ���� �Դϴ�.
    public const byte DEFENCE_SPELL_INDEX_DEFAULT = 3;
    #endregion

    #region Fields & Components
    public SpellManagerClient spellManagerClient;
    public PlayerAnimator playerAnimator;
    private List<SpellInfo> playerOwnedSpellInfoListOnServer = new List<SpellInfo>();
    #endregion

    #region SpellInfo Management
    /// <summary>
    /// Ŭ���̾�Ʈ������ �������� �������� �÷��̾��� ���� ���¸� ������Ʈ�� �� �ֵ����ϴ� ServerRpc �޼����Դϴ�.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void UpdatePlayerSpellStateServerRPC(ushort spellIndex, SpellState spellState, ServerRpcParams serverRpcParams = default)
    {
        UpdatePlayerSpellState(spellIndex, spellState);
    }

    /// <summary>
    /// �÷��̾��� ���� ���¸� ������Ʈ�ϴ� �޼����Դϴ�.
    /// </summary>
    public void UpdatePlayerSpellState(ushort spellIndex, SpellState spellState)
    {
        // �޼��� ���� ���� Ȯ��
        if (!ValidateUpdateSpellStateConditions(spellIndex)) return;

        // ���� ���� ������Ʈ
        playerOwnedSpellInfoListOnServer[spellIndex].spellState = spellState;

        // ������Ʈ�� �÷��̾� ���� ���� ������ ��û�� Ŭ���̾�Ʈ�� ����ȭ
        spellManagerClient.UpdatePlayerSpellInfoArrayClientRPC(playerOwnedSpellInfoListOnServer.ToArray());
    }

    /// <summary>
    /// ���� ���۽� ������ �÷��̾��� ���� ���� ����Ʈ(SpellInfoList)�� �ʱ�ȭ�ϴ� �޼����Դϴ�.
    /// </summary>
    public void InitPlayerSpellInfoArrayOnServer(SpellName[] spellNames)
    {
        List<SpellInfo> playerSpellInfoList = new List<SpellInfo>();
        foreach (SpellName spellName in spellNames)
        {
            Debug.Log($"spellName({spellName}): {SpellSpecifications.Instance.GetSpellDefaultSpec(spellName)}");
            SpellInfo spellInfo = new SpellInfo(SpellSpecifications.Instance.GetSpellDefaultSpec(spellName));
            if (spellInfo == null) continue;
            spellInfo.ownerPlayerClientId = OwnerClientId;
            playerSpellInfoList.Add(spellInfo);
        }

        playerOwnedSpellInfoListOnServer = playerSpellInfoList;
    }

    /// <summary>
    /// ���� ������ �������� SpellInfoList�� ��ȯ�մϴ�.
    /// </summary>
    public List<SpellInfo> GetSpellInfoList()
    {
        return playerOwnedSpellInfoListOnServer;
    }

    /// <summary>
    /// spellName���� SpellInfo�� ã�� �޼����Դϴ�.
    /// </summary>
    /// <param name="spellName">�˰���� ������ �̸�</param>
    /// <returns>SpellInfo ��ü, ������ null</returns>
    public SpellInfo GetSpellInfo(SpellName spellName)
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

    /// <summary>
    /// �ε����� SpellInfo�� ã�� �޼����Դϴ�.
    /// </summary>
    /// <param name="spellIndex">������ �ε���</param>
    /// <returns>SpellInfo ��ü, ������ null</returns>
    public SpellInfo GetSpellInfo(ushort spellIndex)
    {
        if (playerOwnedSpellInfoListOnServer.Count <= spellIndex) return null;
        return playerOwnedSpellInfoListOnServer[spellIndex];
    }

    /// <summary>
    /// spellName���� ���� �ε����� ã�� �޼����Դϴ�.
    /// </summary>
    /// <param name="spellName">ã���� �ϴ� ���� �̸�</param>
    /// <returns>���� �ε���, ��ã���� -1</returns>
    public int GetSpellIndexBySpellName(SpellName spellName)
    {
        int index = -1;

        for (int i = 0; i < playerOwnedSpellInfoListOnServer.Count; i++)
        {
            if (playerOwnedSpellInfoListOnServer[i].spellName.Equals(spellName))
            {
                index = i;
                break;
            }
        }
        return index;
    }
    #endregion

    #region ValiateConditions
    private bool ValidateUpdateSpellStateConditions(ushort spellIndex)
    {
        if (playerOwnedSpellInfoListOnServer.Count <= spellIndex) return false;
        if (spellManagerClient == null) return false;
        return true;
    }
    #endregion
}
