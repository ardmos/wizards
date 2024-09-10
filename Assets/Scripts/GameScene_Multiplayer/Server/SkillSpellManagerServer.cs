using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 서버 측에서 스킬과 스펠을 관리하는 클래스입니다.
/// </summary>
public class SkillSpellManagerServer : NetworkBehaviour
{
    #region Constants
    // SpellInfoList의 인덱스 0~2까지는 공격마법, 3은 방어마법 입니다.
    public const byte DEFENCE_SPELL_INDEX_DEFAULT = 3;
    #endregion

    #region Fields
    public SkillSpellManagerClient skillSpellManagerClient;
    public PlayerAnimator playerAnimator;
    private List<SpellInfo> playerOwnedSpellInfoListOnServer = new List<SpellInfo>();
    #endregion

    #region SpellInfo Management
    /// <summary>
    /// 플레이어의 스펠 상태를 업데이트하는 ServerRpc 메서드입니다.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void UpdatePlayerSpellStateServerRPC(ushort spellIndex, SpellState spellState, ServerRpcParams serverRpcParams = default)
    {
        playerOwnedSpellInfoListOnServer[spellIndex].spellState = spellState;

        // 요청한 클라이언트의 playerOwnedSpellInfoList와 동기화
        skillSpellManagerClient.UpdatePlayerSpellInfoArrayClientRPC(playerOwnedSpellInfoListOnServer.ToArray());
    }

    /// <summary>
    /// 플레이어의 스펠 상태를 업데이트하는 메서드입니다.
    /// </summary>
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

    /// <summary>
    /// 현재 서버가 보유중인 SpellInfo 리스트를 반환합니다.
    /// </summary>
    public List<SpellInfo> GetSpellInfoList()
    {
        return playerOwnedSpellInfoListOnServer;
    }

    /// <summary>
    /// 현재 서버가 보유중인 특정 마법의 정보를 알려주는 메서드 입니다.  
    /// </summary>
    /// <param name="spellName">알고싶은 마법의 이름</param>
    /// <returns>SpellInfo 객체, 없으면 null</returns>
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
    /// 인덱스로 SpellInfo를 가져오는 메서드입니다.
    /// </summary>
    /// <param name="spellIndex">스펠의 인덱스</param>
    /// <returns>SpellInfo 객체, 없으면 null</returns>
    public SpellInfo GetSpellInfo(ushort spellIndex)
    {
        if (playerOwnedSpellInfoListOnServer.Count <= spellIndex) return null;
        return playerOwnedSpellInfoListOnServer[spellIndex];
    }

    /// <summary>
    /// 스킬 이름으로 스펠 인덱스를 찾는 메서드입니다.
    /// </summary>
    /// <param name="skillName">찾고자 하는 스킬 이름</param>
    /// <returns>스펠 인덱스, 못찾으면 -1</returns>
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
