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

    #region Fields
    public SpellManagerClient spellManagerClient;
    public PlayerAnimator playerAnimator;
    private List<SpellInfo> playerOwnedSpellInfoListOnServer = new List<SpellInfo>();
    #endregion

    #region SpellInfo Management
    /// <summary>
    /// �÷��̾��� ���� ���¸� ������Ʈ�ϴ� ServerRpc �޼����Դϴ�.
    /// �Ʒ� UpdatePlayerSpellState�޼���� ������ �۾��� ������ ���� ���ο����� ȣ��Ǵ°��� �ƴ϶� Client�������� ���� ���¸� ������Ʈ��ų �� �ֵ��� ������ �޼��� �Դϴ�. 
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void UpdatePlayerSpellStateServerRPC(ushort spellIndex, SpellState spellState, ServerRpcParams serverRpcParams = default)
    {
        playerOwnedSpellInfoListOnServer[spellIndex].spellState = spellState;

        // ������Ʈ�� �÷��̾� ���� ���� ������ ��û�� Ŭ���̾�Ʈ�� ����ȭ
        spellManagerClient.UpdatePlayerSpellInfoArrayClientRPC(playerOwnedSpellInfoListOnServer.ToArray());
    }

    /// <summary>
    /// �÷��̾��� ���� ���¸� ������Ʈ�ϴ� �޼����Դϴ�.
    /// </summary>
    public void UpdatePlayerSpellState(ushort spellIndex, SpellState spellState)
    {
        playerOwnedSpellInfoListOnServer[spellIndex].spellState = spellState;

        // ������Ʈ�� �÷��̾� ���� ���� ������ ��û�� Ŭ���̾�Ʈ�� ����ȭ
        spellManagerClient.UpdatePlayerSpellInfoArrayClientRPC(playerOwnedSpellInfoListOnServer.ToArray());
    }

    /// <summary>
    /// Server������ ������ SpellInfo ����Ʈ �ʱ�ȭ �޼ҵ� �Դϴ�.
    /// �÷��̾� ���� ������ ȣ��˴ϴ�.
    /// </summary>
    public void InitPlayerSpellInfoArrayOnServer(SpellName[] skillNames)
    {
        List<SpellInfo> playerSpellInfoList = new List<SpellInfo>();
        foreach (SpellName spellName in skillNames)
        {
            Debug.Log($"spellName({spellName}): {SpellSpecifications.Instance.GetSpellDefaultSpec(spellName)}");
            SpellInfo spellInfo = new SpellInfo(SpellSpecifications.Instance.GetSpellDefaultSpec(spellName));
            spellInfo.ownerPlayerClientId = OwnerClientId;
            playerSpellInfoList.Add(spellInfo);
        }

        playerOwnedSpellInfoListOnServer = playerSpellInfoList;
    }

    /// <summary>
    /// ���� ������ �������� SpellInfo ����Ʈ�� ��ȯ�մϴ�.
    /// </summary>
    public List<SpellInfo> GetSpellInfoList()
    {
        return playerOwnedSpellInfoListOnServer;
    }

    /// <summary>
    /// ���� �̸����� SpellInfo�� �������� �޼����Դϴ�.
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
    /// �ε����� SpellInfo�� �������� �޼����Դϴ�.
    /// </summary>
    /// <param name="spellIndex">������ �ε���</param>
    /// <returns>SpellInfo ��ü, ������ null</returns>
    public SpellInfo GetSpellInfo(ushort spellIndex)
    {
        if (playerOwnedSpellInfoListOnServer.Count <= spellIndex) return null;
        return playerOwnedSpellInfoListOnServer[spellIndex];
    }

    /// <summary>
    /// SpellName���� ���� �ε����� ã�� �޼����Դϴ�.
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
}
