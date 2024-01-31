using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// UGS Start Server, Start Client
/// NetworkList 관리
/// 
/// // 순수 서버 스크립트로 만들 필요가 있다. 추후 클라/서버 스크립트 분리 때 참고.  이 클래스 이름도 마음에 안든다.
/// </summary>

public class GameMultiplayer : NetworkBehaviour
{
    // ConnectionApprovalHandler에서 함
    // private const int MAX_PLAYER_AMOUNT = 4;

    // 서버에 접속중인 플레이어들의 데이터가 담긴 리스트
    private NetworkList<PlayerInGameData> playerDataNetworkList;
    // 플레이어들이 보유한 장비 현황
    [SerializeField] private Dictionary<ulong, Dictionary<Item.ItemName, ushort>> playerItemDictionaryOnServer;    

    public static GameMultiplayer Instance { get; private set; }

    public event EventHandler OnSucceededToJoinMatch;
    public event EventHandler OnFailedToJoinMatch;
    public event EventHandler OnPlayerListOnServerChanged;
    public event EventHandler OnPlayerMoveAnimStateChanged;
    public event EventHandler OnPlayerAttackAnimStateChanged;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
        playerDataNetworkList = new NetworkList<PlayerInGameData>();

        playerItemDictionaryOnServer = new Dictionary<ulong, Dictionary<Item.ItemName, ushort>>();
    }

    public override void OnNetworkSpawn()
    {
        //Debug.Log("OnNetworkSpawn()");
        playerDataNetworkList.OnListChanged += OnServerListChanged;
    }

    private void OnServerListChanged(NetworkListEvent<PlayerInGameData> changeEvent)
    {
        //Debug.Log($"OnServerListChanged changed index: ");
        OnPlayerListOnServerChanged?.Invoke(this, EventArgs.Empty);
    }

    // UGS Dedicated Server 
    public void StartServer()
    {
        //NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManager_ConnectionApprovalCallback;
        //NetworkManager.Singleton.OnClientConnectedCallback += Server_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += Server_OnClientDisconnectCallback;
        NetworkManager.Singleton.StartServer();
    }

    /// <summary>
    /// 서버에 참여중인 플레이어가 나간 경우 처리해주는 부분.
    /// 현재는 게임도중 나갔을 경우에대한 처리만 해주고 있습니다.
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
        Debug.Log($"Server_OnClientDisconnectCallback");
    }

    public void StartClient()
    {
        //OnTryingToJoinGame?.Invoke(this, EventArgs.Empty);
        NetworkManager.Singleton.OnClientConnectedCallback += Client_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += Client_OnClientDisconnectCallback;
        NetworkManager.Singleton.StartClient();
    }

    public void StopClient()
    {
        NetworkManager.Singleton.OnClientConnectedCallback -= Client_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback -= Client_OnClientDisconnectCallback;

        // 클라이언트측 네트워크매니저를 껐다가 다시 켤 때, OnNetworkSpawn도 호출됩니다. 그 때 아래 이벤트가 또다시 등록되기때문에 구독취소를 해주고있습니다.
        playerDataNetworkList.OnListChanged -= OnServerListChanged;
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

    /// <summary>
    /// 클래스 변경을 서버에게 보고할 수 있습니다. 메소드명 Change 대신 Update로 변경 고려하기.
    /// 보고 시점은 Server가 Allocate 된 이후! 
    /// 즉 GameRoom에 들어가면서 입니다.
    /// 
    /// 이 메소드를 사용하지 않는 방면으로 수정되었습니다.  삭제여부를 고려해야합니다.
    /// </summary>
    /// <param name="playerClass"></param>    
    private void ChangePlayerClassOnClient(CharacterClass playerClass)
    {
        //Debug.Log($"ChangePlayerClass. clientId: {clientId}, class: {playerClass}");
        //ChangePlayerClassServerRPC(playerClass);
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

    // GameRoomPlayerCharacter에서 해당 인덱스의 플레이어가 접속 되었나 확인할 때 사용 <<< 쓸필요 없어보임. 봐서 아래 메소드로 대체 가능
    public bool IsPlayerIndexConnected(int playerIndex)
    {
        //Debug.Log($"IsPlayerIndexConnected playerIndex: {playerIndex}, playerDataNetworkList.Count: {playerDataNetworkList.Count}");
        return playerIndex < playerDataNetworkList.Count;
    }

    /// <summary>
    /// 플레이어 접속 확인 메소드. 
    /// </summary>
    public bool IsPlayerConnected(ulong clientId)
    {
        return NetworkManager.Singleton.ConnectedClientsIds.Contains(clientId);
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
        //Debug.Log("SetPlayerDataFromClientId");
        playerDataNetworkList[GetPlayerDataIndexFromClientId(clientId)] = newPlayerData;
    }

    /// <summary>
    /// 서버에서 호출해야하는 메소드
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


    public void UpdatePlayerAttackAnimStateOnServer(ulong clientId, PlayerAttackAnimState playerAttackAnimState)
    {
        PlayerInGameData playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(clientId);
        playerData.playerAttackAnimState = playerAttackAnimState;
        SetPlayerDataFromClientId(clientId, playerData);
        // 변경내용을 서버 내의 Player들에 붙어있는 PlayerAnimator에게 알림.
        OnPlayerAttackAnimStateChanged?.Invoke(this, new PlayerAttackAnimStateEventData(clientId, playerData.playerAttackAnimState));
    }


    /// <summary>
    /// 플레이어 보유 아이템 추가. 전부 서버에서 동작하는 메소드 입니다.
    /// </summary>
    [ServerRpc (RequireOwnership = false)]
    public void AddPlayerItemServerRPC(Item.ItemName[] itemNameArray, ushort[] itemCountArray, ServerRpcParams serverRpcParams = default)
    {
        Dictionary<Item.ItemName, ushort> playerItemDictionary = Enumerable.Range(0, itemNameArray.Length).ToDictionary(i => itemNameArray[i], i => itemCountArray[i]);
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
    public void DeletePlayerItemServerRPC(Item.ItemName itemName, ServerRpcParams serverRpcParams = default)
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
        networkClient.PlayerObject.GetComponent<Player>().SetPlayerItemsDictionaryOnClient(playerItemDictionaryOnServer[serverRpcParams.Receive.SenderClientId].Keys.ToArray(), playerItemDictionaryOnServer[serverRpcParams.Receive.SenderClientId].Values.ToArray());      
    }

    // 사용 안하는 메서드. ServerStartUp에서 처리중이다.
    /*    public bool HasAvailablePlayerSlots()
        {
            //return NetworkManager.Singleton.ConnectedClientsIds.Count < MAX_PLAYER_AMOUNT;
            return NetworkManager.Singleton.ConnectedClientsIds.Count < ConnectionApprovalHandler.MaxPlayers;
        }*/

    /*    private void NetworkManager_ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            // GameRoom 씬인지 확인
            if (SceneManager.GetActiveScene().name != LoadingSceneManager.Scene.GameRoomScene.ToString()) { 
                response.Approved = false;
                response.Reason = "Game has already started";
                return;
            }

            // Maximum Player 확인
            if (NetworkManager.Singleton.ConnectedClientsIds.Count >= MAX_PLAYER_AMOUNT)
            {
                response.Approved = false;
                response.Reason = "Game is full";
                return;
            }

            response.Approved = true;
        }*/

    // 플레이어 이름 띄워주는 부분 () 서버일 경우 NetworkManager_OnclientConnectedCallback에서 호출해주고 클라이언트일 경우StartClient에서 호출해줘야함
    // 최초 이름 등록하는부분은 ???? 
    /*[ServerRpc(RequireOwnership = false)]
    private void SetPlayerNameServerRpc(string playerName, ServerRpcParams serverRpcParams = default)
    {
        int playerDataIndex = GetPlayerDataIndexFromClientId(serverRpcParams.Receive.SenderClientId);

        PlayerData playerData = playerDataNetworkList[playerDataIndex];

        playerData.playerName = playerName;

        playerDataNetworkList[playerDataIndex] = playerData;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerIdServerRpc(string playerId, ServerRpcParams serverRpcParams = default)
    {
        int playerDataIndex = GetPlayerDataIndexFromClientId(serverRpcParams.Receive.SenderClientId);

        PlayerData playerData = playerDataNetworkList[playerDataIndex];

        playerData.playerId = playerId;

        playerDataNetworkList[playerDataIndex] = playerData;
    }*/
}
