using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SkillSpellManagerServer : NetworkBehaviour
{
    // SpellInfoList�� �ε��� 0~2������ ���ݸ���, 3�� ���� �Դϴ�.
    public const byte DEFENCE_SPELL_INDEX_DEFAULT = 3;
    public SkillSpellManagerClient skillSpellManagerClient;
    public PlayerAnimator playerAnimator;
    private List<SpellInfo> playerOwnedSpellInfoListOnServer = new List<SpellInfo>();

    #region SpellInfo
    [ServerRpc (RequireOwnership = false)]
    public void UpdatePlayerSpellStateServerRPC(ushort spellIndex, SpellState spellState, ServerRpcParams serverRpcParams = default)
    {
        playerOwnedSpellInfoListOnServer[spellIndex].spellState = spellState;

        // ��û�� Ŭ���̾�Ʈ�� playerOwnedSpellInfoList�� ����ȭ
        skillSpellManagerClient.UpdatePlayerSpellInfoArrayClientRPC(playerOwnedSpellInfoListOnServer.ToArray());
    }

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

/*    public void SetSpellInfo(ushort spellIndex, SpellInfo newSpellInfo)
    {
        if (playerOwnedSpellInfoListOnServer.Count <= spellIndex) return;
        playerOwnedSpellInfoListOnServer[spellIndex] = newSpellInfo;
    }*/

    /// <summary>
    /// -1�� ��ȯ�ϸ� ��ã�Ҵٴ� ��
    /// </summary>
    /// <param name="skillName"></param>
    /// <returns></returns>
    public int GetSpellIndexBySpellName(SkillName skillName)
    {
        int index = -1;

        for (int i = 0; i<playerOwnedSpellInfoListOnServer.Count; i++)
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
