using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 마법 쿨타임 관리. 쿨타임 관리는 클라이언트에서 해주고있습니다.
/// </summary>
public class SkillSpellManagerClient : NetworkBehaviour
{
    public SkillSpellManagerServer skillSpellManagerServer;

    protected const byte totalSpellCount = 4;
    [SerializeField] protected List<SpellInfo> skillInfoListOnClient;
    [SerializeField] protected float[] restTimeCurrentSpellArrayOnClient;

    public override void OnNetworkSpawn()
    {
        skillInfoListOnClient = new List<SpellInfo>();
        restTimeCurrentSpellArrayOnClient = new float[totalSpellCount];
    }

    private void Update()
    {
        // 쿨타임 관리
        for (ushort i = 0; i < skillInfoListOnClient.Count; i++)
        {
            Cooltime(i);
        }
    }

    /// <summary>
    /// 마법 쿨타임 관리. 쿨타임 관리는 클라이언트에서 해주고있습니다.
    /// </summary>
    private void Cooltime(ushort spellIndex)
    {
        //Debug.Log($"spellNumber : {spellIndex}, currentSpellPrefabArray.Length : {currentSpellPrefabArray.Length}");
        if (skillInfoListOnClient[spellIndex] == null) return;
        // 쿨타임 관리
        if (skillInfoListOnClient[spellIndex].spellState == SpellState.Cooltime)
        {
            restTimeCurrentSpellArrayOnClient[spellIndex] += Time.deltaTime;
            if (restTimeCurrentSpellArrayOnClient[spellIndex] >= skillInfoListOnClient[spellIndex].coolTime)
            {
                restTimeCurrentSpellArrayOnClient[spellIndex] = 0f;
                // 여기서 서버에 Ready로 바뀐 State를 보고 하면 서버에서 다시 콜백해서 현 클라이언트 오브젝트의 State가 Ready로 바뀌긴 하는데, 그 사이에 딜레이가 있어서
                // 여기서 한 번 클라이언트의 State를 바꿔주고 서버에 보고 해준다.
                skillInfoListOnClient[spellIndex].spellState = SpellState.Ready;
                skillSpellManagerServer.UpdatePlayerSpellStateServerRPC(spellIndex, SpellState.Ready);
            }
            //Debug.Log($"쿨타임 관리 메소드. spellState:{spellInfoListOnClient[spellIndex].spellState}, restTime:{restTimeCurrentSpellArrayOnClient[spellIndex]}, coolTime:{spellInfoListOnClient[spellIndex].coolTime}");            
        }
    }

    /// <summary>
    /// Spell state를 얻을 수 있는 메소드 입니다
    /// </summary>
    /// <param name="spellIndex"></param>
    /// <returns></returns>
    public SpellState GetSpellStateFromSpellIndexOnClient(ushort spellIndex)
    {
        return skillInfoListOnClient[spellIndex].spellState;
    }

    [ClientRpc]
    public void UpdatePlayerSpellInfoArrayClientRPC(SpellInfo[] spellInfoArray)
    {
        // ServerRPC를 요청한 클라이언트에게만 업데이트 되도록 필터링
        if (!IsOwner) return;

        skillInfoListOnClient = new List<SpellInfo>(spellInfoArray.ToList<SpellInfo>());
    }

    /// <summary>
    /// Player Server에서 Player Init시 InitPlayerSpellInfoArrayOnServer와 함께 호출되는 메서드 입니다. 
    /// 플레이어의 스킬 리스트 정보를 저장합니다
    /// </summary>
    /// <param name="skills"></param>
    public void InitPlayerSpellInfoListClient(SkillName[] skills)
    {
        List<SpellInfo> playerSpellInfoList = new List<SpellInfo>();
        foreach (SkillName skill in skills)
        {
            SpellInfo spellInfo = new SpellInfo(SpellSpecifications.Instance.GetSpellDefaultSpec(skill));
            spellInfo.ownerPlayerClientId = OwnerClientId;
            playerSpellInfoList.Add(spellInfo);
        }

        skillInfoListOnClient = playerSpellInfoList;
        Debug.Log("2. SkillSpellManagerClient.InitPlayerSpellInfoListClient");
    }

    /// <summary>
    /// 특정 client가 현재 보유중인 마법의 정보를 알려주는 메소드 입니다.  
    /// </summary>
    /// <param name="clientId">알고싶은 Client의 ID</param>
    /// <returns></returns>
    public List<SpellInfo> GetSpellInfoList()
    {
        return skillInfoListOnClient;
    }

    #region 현재 마법 restTime/coolTime 얻기
    /// <summary>
    /// 클라이언트에서 동작하는 메소드
    /// </summary>
    /// <param name="spellIndex"></param>
    /// <returns></returns>
    public float GetCurrentSpellCoolTimeRatio(ushort spellIndex)
    {
        if (skillInfoListOnClient.Count <= spellIndex) return 0f;
        if (skillInfoListOnClient[spellIndex] == null) return 0f;
        float coolTimeRatio = restTimeCurrentSpellArrayOnClient[spellIndex] / skillInfoListOnClient[spellIndex].coolTime;
        return coolTimeRatio;
    }
    #endregion
}
