using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SkillSpellManagerServer : NetworkBehaviour
{
    // SpellInfoList의 인덱스 0~2까지는 공격마법, 3은 방어마법 입니다.
    public const byte DEFENCE_SPELL_INDEX_DEFAULT = 3;
    public SkillSpellManagerClient skillSpellManagerClient;
    public PlayerAnimator playerAnimator;
    private List<SpellInfo> playerOwnedSpellInfoListOnServer = new List<SpellInfo>();

    #region SpellInfo
    [ServerRpc (RequireOwnership = false)]
    public void UpdatePlayerSpellStateServerRPC(ushort spellIndex, SpellState spellState, ServerRpcParams serverRpcParams = default)
    {
        playerOwnedSpellInfoListOnServer[spellIndex].spellState = spellState;

        // 요청한 클라이언트의 playerOwnedSpellInfoList와 동기화
        skillSpellManagerClient.UpdatePlayerSpellInfoArrayClientRPC(playerOwnedSpellInfoListOnServer.ToArray());
    }

    public void UpdatePlayerSpellState(ushort spellIndex, SpellState spellState)
    {
        playerOwnedSpellInfoListOnServer[spellIndex].spellState = spellState;

        // 요청한 클라이언트의 playerOwnedSpellInfoList와 동기화
        skillSpellManagerClient.UpdatePlayerSpellInfoArrayClientRPC(playerOwnedSpellInfoListOnServer.ToArray());
    }

    /// <summary>
    /// Server측에서 보유한 SpellInfo 리스트 초기화 메소드 입니다.
    /// 플레이어 최초 생성시 호출됩니다.
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
    /// 현재 client가 보유중인 특정 마법의 정보를 알려주는 메소드 입니다.  
    /// </summary>
    /// <param name="spellName">알고싶은 마법의 이름</param>
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
    /// -1을 반환하면 못찾았다는 뜻
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
