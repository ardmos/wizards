using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 이거 서버/클라 나눠야함./
/// UGS Start Server, Start Client
/// NetworkList 관리
/// </summary>
public class GameMultiplayer : NetworkBehaviour
{
    // 서버에 접속중인 플레이어들의 데이터가 담긴 리스트 <<--- 여기있는 HP같은것들. 컴포넌츠 패턴으로 인터페이스를 추가하는 식으로 각자 캐릭터의 스크립트에 붙어있습니다.  이 PlayerInGameData가 필요가 없도록 하는게 깔끔해보입니다.
    private NetworkList<PlayerInGameData> playerDataNetworkList;
    // 플레이어들이 보유한 장비 현황
    [SerializeField] private Dictionary<ulong, Dictionary<ItemName, ushort>> playerItemDictionaryOnServer;    

    public static GameMultiplayer Instance { get; private set; }

    public event EventHandler OnSucceededToJoinMatch;
    public event EventHandler OnFailedToJoinMatch;
    public event EventHandler OnPlayerListOnServerChanged;
    public event EventHandler OnPlayerMoveAnimStateChanged;
    //public event EventHandler OnPlayerAttackAnimStateChanged;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
        playerDataNetworkList = new NetworkList<PlayerInGameData>();

        playerItemDictionaryOnServer = new Dictionary<ulong, Dictionary<ItemName, ushort>>();
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log("OnNetworkSpawn()");
        playerDataNetworkList.OnListChanged += OnServerListChanged;
    }

    public override void OnNetworkDespawn()
    {
        Debug.Log("OnNetworkDespawn()");
        playerDataNetworkList.OnListChanged -= OnServerListChanged;

        if (IsServer)   NetworkManager.Singleton.OnClientDisconnectCallback -= Server_OnClientDisconnectCallback;
    }

    private void OnServerListChanged(NetworkListEvent<PlayerInGameData> changeEvent)
    {
        //Debug.Log($"OnServerListChanged changed index: ");
        OnPlayerListOnServerChanged?.Invoke(this, EventArgs.Empty);
    }

    // --- 서버
    // UGS Dedicated Server
    public void StartServer()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback += Server_OnClientDisconnectCallback;
        NetworkManager.Singleton.StartServer();
    }

    /// <summary>
    /// 게임도중 플레이어가 나갔을 경우에대한 처리입니다.
    /// </summary>
    /// <param name="clientId"></param>
    private void Server_OnClientDisconnectCallback(ulong clientId)
    {
        // 게임중인지 확인.
        if (GameManager.Instance == null) { 
            Debug.Log("유저가 나갔지만 게임씬이 아닙니다.");
            if(GetPlayerDataIndexFromClientId(clientId) != -1) 
                playerDataNetworkList.RemoveAt(GetPlayerDataIndexFromClientId(clientId));
            return; 
        }

        // Game중이라면 GameManager에서 죽은걸로 처리. 어차피 재접속 안되게끔 구현할거니까 재접속시 처리는 안해도 된다. 해당 리스트 아이템을 삭제할 필요도 없다.
        if (GetPlayerDataIndexFromClientId(clientId) != -1)
            GameManager.Instance.UpdatePlayerGameOverOnServer(clientId);


        // 플레이어 이탈 처리
        if (GetPlayerDataIndexFromClientId(clientId) != -1)
            playerDataNetworkList.RemoveAt(GetPlayerDataIndexFromClientId(clientId));


        Debug.Log($"Server_OnClientDisconnectCallback, Player Count :{playerDataNetworkList.Count}");
        // 혹시 모든 플레이어가 나갔으면, 서버도 다시 로비씬으로 돌아간다
        if (playerDataNetworkList.Count == 0)
        {
            Debug.Log($"Server_OnClientDisconnectCallback, Go to Lobby");
            CleanUp();
            // 로비씬으로 이동
            LoadSceneManager.Load(LoadSceneManager.Scene.LobbyScene);
        }

    }
    // 로비씬으로 돌아기 전 초기화
    private void CleanUp()
    {
        #if UNITY_SERVER
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
            Destroy(NetworkManager.Singleton.gameObject);
        }
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        if(ServerStartUp.Instance != null)
        {
            Destroy(ServerStartUp.Instance.gameObject);
        }
        #endif
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdatePlayerInGameDataServerRPC(PlayerInGameData playerData, ServerRpcParams serverRpcParams = default)
    {
        // 새로운 유저
        playerDataNetworkList.Add(new PlayerInGameData
        {
            clientId = serverRpcParams.Receive.SenderClientId,
            characterClass = playerData.characterClass,
            playerMoveAnimState = PlayerMoveAnimState.Idle,
            playerGameState = PlayerGameState.Playing,
            playerName = playerData.playerName
            // HP는 게임 시작되면 OnNetworkSpawn때 각자가 SetPlayerHP로 보고함.
        });
        Debug.Log($"ChangePlayerClassServerRPC PlayerDataList Add complete. " +
            $"player{serverRpcParams.Receive.SenderClientId} Name: {playerData.playerName} Class: {playerData.characterClass} PlayerDataList.Count:{playerDataNetworkList.Count}");
    }

    // GameRoomPlayerCharacter에서 해당 인덱스의 플레이어가 접속 되었나 확인할 때 사용
    public bool IsPlayerIndexConnected(int playerIndex)
    {
        //Debug.Log($"IsPlayerIndexConnected playerIndex: {playerIndex}, playerDataNetworkList.Count: {playerDataNetworkList.Count}");
        return playerIndex < playerDataNetworkList.Count;
    }

    // 플레이어 clientID를 단서로 player Index를 찾는 메소드
    public int GetPlayerDataIndexFromClientId(ulong clientId)
    {
        //Debug.Log("GetPlayerDataIndexFromClientId");
        for (int i = 0; i < playerDataNetworkList.Count; i++)
        {
            if (playerDataNetworkList[i].clientId == clientId)
            {
                return i;
            }
        }
        return -1;
    }

    // 플레이어 client를 단서로 PlayerData(ClientId 포함 여러 플레이어 데이터)를 찾는 메소드
    public PlayerInGameData GetPlayerDataFromClientId(ulong clientId)
    {
        foreach (PlayerInGameData playerData in playerDataNetworkList)
        {
            if (playerData.clientId == clientId)
            {
                return playerData;
            }
        }
        return default;
    }

    // 플레이어 Index를 단서로 PlayerData(ClientId 포함 여러 플레이어 데이터)를 찾는 메소드
    public PlayerInGameData GetPlayerDataFromPlayerIndex(int playerIndex)
    {
        if (playerIndex >= playerDataNetworkList.Count)
        {
            Debug.Log($"playerIndex is wrong. playerIndex:{playerIndex}, listCount: {playerDataNetworkList.Count}");       
        }
        return playerDataNetworkList[playerIndex];
    }
    
    public NetworkList<PlayerInGameData> GetPlayerDataNetworkList()
    {
        return playerDataNetworkList;
    }

    public byte GetPlayerCount()
    {
        return (byte)playerDataNetworkList.Count;
    }

    /// <summary>
    /// 서버에서 호출해야하는 메소드
    /// </summary>
    public void SetPlayerDataFromClientId(ulong clientId, PlayerInGameData newPlayerData)
    {
        playerDataNetworkList[GetPlayerDataIndexFromClientId(clientId)] = newPlayerData;
        //Debug.Log($"SetPlayerDataFromClientId. player.clientId:{clientId}. playerGameState:{playerDataNetworkList[GetPlayerDataIndexFromClientId(clientId)].playerGameState}");
    }

    /// <summary>
    /// 서버에서 호출해야하는 메소드. 플레이어의 이동 처리부분을 담당하는 스크립트를 따로 만드는게 좋아보인다!! 나중에 코드 정리할 때 참고해서 진행하자.
    /// 특정 플레이어에게 이동 및 특정 자세(비 공격적) 애니메이션을 실행시켜줄 수 있는 메소드 입니다.
    /// </summary>
    /// <param name="clientId">플레이어 캐릭터 특정</param>
    /// <param name="playerAnimState">실행시키고싶은 애니메이션 state</param>
    public void UpdatePlayerMoveAnimStateOnServer(ulong clientId, PlayerMoveAnimState playerMoveAnimState)
    {
        PlayerInGameData playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(clientId);
        playerData.playerMoveAnimState = playerMoveAnimState;
        SetPlayerDataFromClientId(clientId, playerData);
        // 변경내용을 서버 내의 Player들에 붙어있는 PlayerAnimator에게 알림.
        OnPlayerMoveAnimStateChanged?.Invoke(this, new PlayerMoveAnimStateEventData(clientId, playerData.playerMoveAnimState));
    }

    /// <summary>
    /// 플레이어 보유 아이템 추가. 전부 서버에서 동작하는 메소드 입니다.
    /// </summary>
    [ServerRpc (RequireOwnership = false)]
    public void AddPlayerItemServerRPC(ItemName[] itemNameArray, ushort[] itemCountArray, ServerRpcParams serverRpcParams = default)
    {
        Dictionary<ItemName, ushort> playerItemDictionary = Enumerable.Range(0, itemNameArray.Length).ToDictionary(i => itemNameArray[i], i => itemCountArray[i]);
        Debug.Log($"AddPlayerItemServerRPC. player{serverRpcParams.Receive.SenderClientId}'s playerItemDictionary.Count: {playerItemDictionary.Count} ");
        foreach (var item in playerItemDictionary)
        {
            Debug.Log($"{item.Key}, {item.Value}");
        }

        if (playerItemDictionaryOnServer.ContainsKey(serverRpcParams.Receive.SenderClientId))
            playerItemDictionaryOnServer[serverRpcParams.Receive.SenderClientId] = playerItemDictionary;
        else
            playerItemDictionaryOnServer.Add(serverRpcParams.Receive.SenderClientId, playerItemDictionary);
    }
    [ServerRpc(RequireOwnership = false)]
    public void DeletePlayerItemServerRPC(ItemName itemName, ServerRpcParams serverRpcParams = default)
    {
        if (!playerItemDictionaryOnServer.ContainsKey(serverRpcParams.Receive.SenderClientId)) return;

        if(playerItemDictionaryOnServer[serverRpcParams.Receive.SenderClientId][itemName]>0)
            playerItemDictionaryOnServer[serverRpcParams.Receive.SenderClientId][itemName]--;
        else 
            playerItemDictionaryOnServer[serverRpcParams.Receive.SenderClientId][itemName] = 0;
    }
    [ServerRpc(RequireOwnership = false)]
    public void GetPlayerItemDictionaryServerRPC(ServerRpcParams serverRpcParams = default)
    {
        NetworkClient networkClient = NetworkManager.ConnectedClients[serverRpcParams.Receive.SenderClientId];
        networkClient.PlayerObject.GetComponent<PlayerClient>().SetPlayerItemsDictionaryOnClient(playerItemDictionaryOnServer[serverRpcParams.Receive.SenderClientId].Keys.ToArray(), playerItemDictionaryOnServer[serverRpcParams.Receive.SenderClientId].Values.ToArray());      
    }

    /// <summary>
    /// 플레이어 스코어 추가.
    /// </summary>
    /// <param name="clientID"></param>
    /// <param name="score"></param>
    public void AddPlayerScore(ulong clientID , int score)
    {
        PlayerInGameData playerData = GetPlayerDataFromClientId(clientID);
        Debug.Log($"1.player:{playerData.clientId}'s score:{playerData.score}");
        playerData.score += score;

        SetPlayerDataFromClientId(clientID, playerData);
        Debug.Log($"2.player:{playerData.clientId}'s score:{playerData.score}, getScore:{GetPlayerScore(clientID)}");
    }

    /// <summary>
    /// 플레이어 스코어 얻기.
    /// </summary>
    /// <param name="clientID"></param>
    /// <returns></returns>
    public int GetPlayerScore(ulong clientID)
    {
        Debug.Log($"GetPlayerScore player{clientID} requested.");
        return GetPlayerDataFromClientId(clientID).score;
    }

    // 클라이언트 ---

    public void StartClient()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += Client_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += Client_OnClientDisconnectCallback;
        NetworkManager.Singleton.StartClient();
    }

    public void StopClient()
    {
        Debug.Log("StopClient()");

        NetworkManager.Singleton.OnClientConnectedCallback -= Client_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback -= Client_OnClientDisconnectCallback;
        NetworkManager.Singleton.Shutdown();
    }

    /// <summary>
    ///  클라이언트 측에서. 접속 성공시 할 일들
    ///  1. NetworkList인 PlayerDataList에 현재 선택중인 클래스 정보를 등록한다.
    /// </summary>
    private void Client_OnClientConnectedCallback(ulong clientId)
    {
        // 매칭 UI 실행을 위한 이벤트 핸들러 호출
        OnSucceededToJoinMatch?.Invoke(this, EventArgs.Empty);
        // 서버RPC를 통해 서버에 저장
        Debug.Log($"Client_OnClientConnectedCallback. clientId: {clientId}, class: {PlayerDataManager.Instance.GetCurrentPlayerClass()}");
        //ChangePlayerClass(PlayerProfileData.Instance.GetCurrentSelectedClass());
        SavePlayerInGameDataOnServer(PlayerDataManager.Instance.GetPlayerInGameData());
    }
    private void Client_OnClientDisconnectCallback(ulong obj)
    {
        Debug.Log($"OnClientDisconnectCallback : {obj}");
        // 매칭 UI 숨김을 위한 이벤트 핸들러 호출. 
        OnFailedToJoinMatch?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// 로컬에 저장되어있는 Player 정보를 생성된 서버에 저장하는 스크립트 입니다.
    /// </summary>
    /// <param name="playerData"></param>
    private void SavePlayerInGameDataOnServer(PlayerInGameData playerData)
    {
        UpdatePlayerInGameDataServerRPC(playerData);
    }
}
