using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// ���� ������ ��ų�� ������ �����ϴ� Ŭ�����Դϴ�.
/// </summary>
public class SkillSpellManagerServer : NetworkBehaviour
{
    #region Constants
    // SpellInfoList�� �ε��� 0~2������ ���ݸ���, 3�� ���� �Դϴ�.
    public const byte DEFENCE_SPELL_INDEX_DEFAULT = 3;
    #endregion

    #region Fields
    public SkillSpellManagerClient skillSpellManagerClient;
    public PlayerAnimator playerAnimator;
    private List<SpellInfo> playerOwnedSpellInfoListOnServer = new List<SpellInfo>();
    #endregion

    #region SpellInfo Management
    /// <summary>
    /// �÷��̾��� ���� ���¸� ������Ʈ�ϴ� ServerRpc �޼����Դϴ�.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void UpdatePlayerSpellStateServerRPC(ushort spellIndex, SpellState spellState, ServerRpcParams serverRpcParams = default)
    {
        playerOwnedSpellInfoListOnServer[spellIndex].spellState = spellState;

        // ��û�� Ŭ���̾�Ʈ�� playerOwnedSpellInfoList�� ����ȭ
        skillSpellManagerClient.UpdatePlayerSpellInfoArrayClientRPC(playerOwnedSpellInfoListOnServer.ToArray());
    }

    /// <summary>
    /// �÷��̾��� ���� ���¸� ������Ʈ�ϴ� �޼����Դϴ�.
    /// </summary>
    public void UpdatePlayerSpellState(ushort spellIndex, SpellState spellState)
    {
        playerOwnedSpellInfoListOnServer[spellIndex].spellState = spellState;

        // ��û�� Ŭ���̾�Ʈ�� playerOwnedSpellInfoList�� ����ȭ
        skillSpellManagerClient.UpdatePlayerSpellInfoArrayClientRPC(playerOwnedSpellInfoListOnServer.ToArray());
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
    /// ���� ������ �������� Ư�� ������ ������ �˷��ִ� �޼��� �Դϴ�.  
    /// </summary>
    /// <param name="spellName">�˰���� ������ �̸�</param>
    /// <returns>SpellInfo ��ü, ������ null</returns>
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
    /// ��ų �̸����� ���� �ε����� ã�� �޼����Դϴ�.
    /// </summary>
    /// <param name="skillName">ã���� �ϴ� ��ų �̸�</param>
    /// <returns>���� �ε���, ��ã���� -1</returns>
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
