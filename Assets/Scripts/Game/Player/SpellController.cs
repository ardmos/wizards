using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
/// <summary>
/// 플레이어 캐릭터 오브젝트에 붙이는 스크립트
/// 현재 기능
///   1. 현재 캐릭터 마법 보유 현황 관리
///   2. 캐릭터 보유 마법 발동 
///   3. 현재 보유 마법에 대한 정보를 공유
///   4. 현재 캐스팅중인 마법 오브젝트 관리
/// </summary>
public class SpellController : NetworkBehaviour
{
    public event EventHandler OnSpellStateChanged;

    // 서버에 저장되는 내용
    [SerializeField] private Dictionary<ulong, List<SpellInfo>> spellInfoListOnServer = new Dictionary<ulong, List<SpellInfo>>();

    // 클라이언트에 저장되는 내용
    [SerializeField] protected List<GameObject> ownedSpellPrefabListOnClient;
    [SerializeField] private List<SpellInfo> spellInfoListOnClient;
    [SerializeField] private float[] restTimeCurrentSpellArrayOnClient = new float[3];

    private void Awake()
    {
        spellInfoListOnClient = new List<SpellInfo>(3) {};
    }

    private void Update()
    {
        // 쿨타임 관리
        for (ushort i = 0; i < ownedSpellPrefabListOnClient.Count; i++)
        {
            Cooltime(i);
        }        
    }

    /// <summary>
    /// 마법 쿨타임 관리. 클라이언트에서 동작하는 메소드
    /// </summary>
    private void Cooltime(ushort spellIndex)
    {
        //Debug.Log($"spellNumber : {spellIndex}, currentSpellPrefabArray.Length : {currentSpellPrefabArray.Length}");
        if (ownedSpellPrefabListOnClient[spellIndex] == null) return;
        // 쿨타임 관리
        if (spellInfoListOnClient[spellIndex].spellState == SpellState.Cooltime)
        {
            restTimeCurrentSpellArrayOnClient[spellIndex] += Time.deltaTime;
            if (restTimeCurrentSpellArrayOnClient[spellIndex] >= spellInfoListOnClient[spellIndex].coolTime)
            {                
                restTimeCurrentSpellArrayOnClient[spellIndex] = 0f;
                // 여기서 서버에 Ready로 바뀐 State를 보고 하면 서버에서 다시 콜백해서 현 클라이언트 오브젝트의 State가 Ready로 바뀌긴 하는데, 그 사이에 딜레이가 있어서
                // 여기서 한 번 클라이언트의 State를 바꿔주고 서버에 보고 해준다.
                spellInfoListOnClient[spellIndex].spellState = SpellState.Ready;
                UpdatePlayerSpellStateServerRPC(spellIndex, SpellState.Ready);
            }
            //Debug.Log($"쿨타임 관리 메소드. spellState:{spellInfoListOnClient[spellIndex].spellState}, restTime:{restTimeCurrentSpellArrayOnClient[spellIndex]}, coolTime:{spellInfoListOnClient[spellIndex].coolTime}");            
        }
    }

    #region 현재 설정된 마법 시전
    /// <summary>
    /// 마법 캐스팅 시작. 클라이언트에서 동작하는 메소드
    /// </summary>
    /// <param name="spellIndex"></param>
    public void StartCastingSpellOnClient(ushort spellIndex)
    {
        if (spellInfoListOnClient[spellIndex].spellState == SpellState.Cooltime || spellInfoListOnClient[spellIndex].spellState == SpellState.Casting)
        {
            //Debug.Log($"마법 {spellInfoListOnClient[spellIndex].spellName}은 현재 시전불가상태입니다.");
            return;
        }

        // 캐스팅 시작 Server에게 요청.
        ownedSpellPrefabListOnClient[spellIndex].GetComponent<Spell>().CastSpell(spellInfoListOnClient[spellIndex], GetComponent<NetworkObject>());
        // 캐스팅 시작으로 변경된 SpellState 정보 Server에 보고.
        UpdatePlayerSpellStateServerRPC(spellIndex, SpellState.Casting);
    }

    /// <summary>
    /// 캐스팅중인 마법 발사. 클라이언트에서 동작하는 메소드
    /// </summary>
    public void ShootCurrentCastingSpellOnClient(ushort spellIndex)
    {
        if (spellInfoListOnClient[spellIndex].spellState != SpellState.Casting) return;

        // 서버에 발사 요청
        SpellManager.Instance.ShootSpellObject() ; 
        // 발사로 변경된 SpellState 상태 서버에 보고
        UpdatePlayerSpellStateServerRPC(spellIndex, SpellState.Cooltime);
    }
    #endregion

    #region 현재 마법 restTime/coolTime 얻기
    /// <summary>
    /// 클라이언트에서 동작하는 메소드
    /// </summary>
    /// <param name="spellIndex"></param>
    /// <returns></returns>
    public float GetCurrentSpellCoolTimeRatio(ushort spellIndex)
    {
        if (spellInfoListOnClient[spellIndex] == null) return 0f;
        float coolTimeRatio = restTimeCurrentSpellArrayOnClient[spellIndex] / spellInfoListOnClient[spellIndex].coolTime;
        return coolTimeRatio;
    }
    #endregion

    /// <summary>
    /// Spell state를 얻을 수 있는 메소드 입니다
    /// </summary>
    /// <param name="spellIndex"></param>
    /// <returns></returns>
    public SpellState GetSpellStateFromSpellIndexOnServer(ulong clientId, ushort spellIndex)
    {
        return spellInfoListOnServer[clientId][spellIndex].spellState;
    }
    /// <summary>
    /// Spell state를 얻을 수 있는 메소드 입니다
    /// </summary>
    /// <param name="spellIndex"></param>
    /// <returns></returns>
    public SpellState GetSpellStateFromSpellIndexOnClient(ushort spellIndex)
    {
        return spellInfoListOnClient[spellIndex].spellState;
    }

    #region 현재 보유 마법 변경
    // GameAsset으로부터 Spell Prefab 로딩. playerClass와 spellName으로 필요한 프리팹만 불러온다. 
    // 불러온 SpellInfoList는 Server측에 보고한다.
    public void SetCurrentSpellOnClient(SpellName[] ownedSpellNameList)
    {
        foreach (var spellName in ownedSpellNameList)
        {
            Debug.Log($"spellName: {spellName}");
            ownedSpellPrefabListOnClient.Add(GameAssets.instantiate.GetSpellPrefab(spellName));
        }
        for (ushort spellIndex = 0; spellIndex < ownedSpellPrefabListOnClient.Count; spellIndex++)
        {
            ownedSpellPrefabListOnClient[spellIndex].GetComponent<Spell>().InitSpellInfoDetail();
            // 최초 생성시
            if(spellInfoListOnClient.Count <= spellIndex)
            {
                spellInfoListOnClient.Add(ownedSpellPrefabListOnClient[spellIndex].GetComponent<Spell>().spellInfo);
            }
            // 스킬 목록 변경시
            else
            {
                spellInfoListOnClient[spellIndex] = ownedSpellPrefabListOnClient[spellIndex].GetComponent<Spell>().spellInfo;
            }           
            Sprite spellIconImage = GameAssets.instantiate.GetSpellIconImage(spellInfoListOnClient[spellIndex].spellName);
            FindObjectOfType<GamePadUI>().UpdateSpellUI(spellIconImage, spellIndex);
        }
            
        // 서버에게 플레이어의 '보유 마법 정보 리스트'를 보고.
        UpdatePlayerSpellInfoServerRPC(spellInfoListOnClient.ToArray());  
    }
    #endregion

    /// <summary>
    /// 변경된 SpellState를 Server에 보고합니다. 보고받은 Server는 Server의 PlayerAnimator에게 애니메이션 변경을 지시합니다.
    /// </summary>
    /// <param name="spellIndex"></param>
    [ServerRpc]
    private void UpdatePlayerSpellStateServerRPC(ushort spellIndex, SpellState spellState, ServerRpcParams serverRpcParams = default)
    {
        ulong clientId = serverRpcParams.Receive.SenderClientId;
        if (!spellInfoListOnServer.ContainsKey(clientId))
        {
            Debug.LogError($"UpdatePlayerSpellStateServerRPC. There is no SpellInfoList for this client. clientId:{clientId}");
            return;
        }

        spellInfoListOnServer[clientId][spellIndex].spellState = spellState;
        OnSpellStateChanged?.Invoke(this, new SpellStateEventData(clientId, spellState));
        Debug.Log($"SpellController UpdatePlayerSpellStateServerRPC: {spellIndex}");

        // 요청한 클라이언트의 currentSpellInfoList 동기화
        NetworkClient networkClient = NetworkManager.ConnectedClients[clientId];
        networkClient.PlayerObject.GetComponent<SpellController>().UpdatePlayerSpellInfoClientRPC(spellInfoListOnServer[clientId].ToArray());
    }

    /// <summary>
    /// Server측에서 보유한 ClientId별 SpellInfo 리스트 업데이트.
    /// 업데이트가 끝나면 클라이언트측과 SpellInfo 정보 동기화를 진행합니다.
    /// </summary>
    /// <param name="spellInfoArray"></param>
    /// <param name="serverRpcParams"></param>
    [ServerRpc]
    private void UpdatePlayerSpellInfoServerRPC(SpellInfo[] spellInfoArray, ServerRpcParams serverRpcParams = default)
    {
        ulong clientId = serverRpcParams.Receive.SenderClientId;
        if (!spellInfoListOnServer.ContainsKey(clientId))
        {
            spellInfoListOnServer.Add(clientId, spellInfoArray.ToList<SpellInfo>());
        }
        else
        {
            spellInfoListOnServer[clientId] = spellInfoArray.ToList<SpellInfo>();
        }

        // 요청한 클라이언트의 currentSpellInfoList 동기화
        NetworkClient networkClient = NetworkManager.ConnectedClients[clientId];
        networkClient.PlayerObject.GetComponent<SpellController>().UpdatePlayerSpellInfoClientRPC(spellInfoListOnServer[clientId].ToArray());
    }

    [ClientRpc]
    private void UpdatePlayerSpellInfoClientRPC(SpellInfo[] spellInfoArray)
    {
        spellInfoListOnClient = spellInfoArray.ToList<SpellInfo>();
    }

}
