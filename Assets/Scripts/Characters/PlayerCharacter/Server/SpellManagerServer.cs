using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 서버 측에서 스킬과 스펠을 관리하는 클래스입니다.
/// </summary>
public class SpellManagerServer : NetworkBehaviour
{
    #region Constants
    // SpellInfoList의 인덱스 0~2까지는 공격마법, 3은 방어마법 입니다.
    public const byte DEFENCE_SPELL_INDEX_DEFAULT = 3;
    #endregion

    #region Fields & Components
    public SpellManagerClient spellManagerClient;
    public PlayerAnimator playerAnimator;
    private List<SpellInfo> playerOwnedSpellInfoListOnServer = new List<SpellInfo>();
    #endregion

    #region SpellInfo Management
    /// <summary>
    /// 클라이언트측에서 서버측에 저장중인 플레이어의 스펠 상태를 업데이트할 수 있도록하는 ServerRpc 메서드입니다.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void UpdatePlayerSpellStateServerRPC(ushort spellIndex, SpellState spellState, ServerRpcParams serverRpcParams = default)
    {
        UpdatePlayerSpellState(spellIndex, spellState);
    }

    /// <summary>
    /// 플레이어의 스펠 상태를 업데이트하는 메서드입니다.
    /// </summary>
    public void UpdatePlayerSpellState(ushort spellIndex, SpellState spellState)
    {
        // 메서드 실행 조건 확인
        if (!ValidateUpdateSpellStateConditions(spellIndex)) return;

        // 스펠 상태 업데이트
        playerOwnedSpellInfoListOnServer[spellIndex].spellState = spellState;

        // 업데이트된 플레이어 스펠 상태 정보를 요청한 클라이언트와 동기화
        spellManagerClient.UpdatePlayerSpellInfoArrayClientRPC(playerOwnedSpellInfoListOnServer.ToArray());
    }

    /// <summary>
    /// 게임 시작시 생성된 플레이어의 보유 마법 리스트(SpellInfoList)를 초기화하는 메서드입니다.
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
    /// 현재 서버가 보유중인 SpellInfoList를 반환합니다.
    /// </summary>
    public List<SpellInfo> GetSpellInfoList()
    {
        return playerOwnedSpellInfoListOnServer;
    }

    /// <summary>
    /// spellName으로 SpellInfo를 찾는 메서드입니다.
    /// </summary>
    /// <param name="spellName">알고싶은 마법의 이름</param>
    /// <returns>SpellInfo 객체, 없으면 null</returns>
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
    /// 인덱스로 SpellInfo를 찾는 메서드입니다.
    /// </summary>
    /// <param name="spellIndex">스펠의 인덱스</param>
    /// <returns>SpellInfo 객체, 없으면 null</returns>
    public SpellInfo GetSpellInfo(ushort spellIndex)
    {
        if (playerOwnedSpellInfoListOnServer.Count <= spellIndex) return null;
        return playerOwnedSpellInfoListOnServer[spellIndex];
    }

    /// <summary>
    /// spellName으로 스펠 인덱스를 찾는 메서드입니다.
    /// </summary>
    /// <param name="spellName">찾고자 하는 마법 이름</param>
    /// <returns>스펠 인덱스, 못찾으면 -1</returns>
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
