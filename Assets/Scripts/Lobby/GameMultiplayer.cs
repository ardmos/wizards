using System;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// UGS Start Server, Start Client
/// NetworkList 관리
/// </summary>

public class GameMultiplayer : NetworkBehaviour
{
    // ConnectionApprovalHandler에서 함
    // private const int MAX_PLAYER_AMOUNT = 4;

    // 서버에 접속중인 플레이어들의 데이터가 담긴 리스트
    private NetworkList<PlayerData> playerDataNetworkList;

    public static GameMultiplayer Instance { get; private set; }

    public event EventHandler OnTryingToJoinGame;
    public event EventHandler OnFailedToJoinGame;
    public event EventHandler OnServerPlayerListChanged;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
        playerDataNetworkList = new NetworkList<PlayerData>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        playerDataNetworkList.OnListChanged += OnServerListChanged;
    }

    private void OnServerListChanged(NetworkListEvent<PlayerData> changeEvent)
    {
        Debug.Log($"OnServerListChanged changed index: ");
        OnServerPlayerListChanged?.Invoke(this, EventArgs.Empty);
    }

    // UGS Dedicated Server 
    public void StartServer()
    {
        //NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManager_ConnectionApprovalCallback;
        //NetworkManager.Singleton.OnClientConnectedCallback += Server_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += Server_OnClientDisconnectCallback;
        NetworkManager.Singleton.StartServer();
    }

    // GameRoom에서 Client가 나갔을 때 플레이어를 없애주는 부분.
    private void Server_OnClientDisconnectCallback(ulong clientId)
    {
        for (int i = 0; i<playerDataNetworkList.Count; i++)
        {
            PlayerData playerData = playerDataNetworkList[i];
            if (playerData.clientId == clientId)
            {
                // 플레이어 Disconnected. 해당 인덱스 데이터 삭제
                playerDataNetworkList.RemoveAt(i);
            }
        }
    }

    public void StartClient()
    {
        OnTryingToJoinGame?.Invoke(this, EventArgs.Empty);
        NetworkManager.Singleton.OnClientConnectedCallback += Client_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += Client_OnClientDisconnectCallback;
        NetworkManager.Singleton.StartClient();
    }
    /// <summary>
    ///  클라이언트 측에서. 접속 성공시 할 일들
    ///  1. NetworkList인 PlayerDataList에 현재 선택중인 클래스 정보를 등록한다.
    /// </summary>
    private void Client_OnClientConnectedCallback(ulong clientId)
    {
        // 서버RPC를 통해 서버에 저장
        Debug.Log($"Client_OnClientConnectedCallback. clientId: {clientId}, class: {PlayerProfileData.Instance.GetCurrentSelectedClass()}");
        ChangePlayerClass(PlayerProfileData.Instance.GetCurrentSelectedClass());
    }
    private void Client_OnClientDisconnectCallback(ulong obj)
    {
        Debug.Log($"OnClientDisconnectCallback : {obj}");
        OnFailedToJoinGame?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// 클래스 변경을 서버에게 보고할 수 있습니다.
    /// 보고 시점은 Server가 Allocate 된 이후! 
    /// 즉 GameRoom에 들어가면서 입니다.
    /// </summary>
    /// <param name="playerClass"></param>    
    private void ChangePlayerClass(CharacterClasses.Class playerClass)
    {
        //Debug.Log($"ChangePlayerClass. clientId: {clientId}, class: {playerClass}");
        ChangePlayerClassServerRPC(playerClass);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangePlayerClassServerRPC(CharacterClasses.Class playerClass, ServerRpcParams serverRpcParams = default)
    {
        // 새로운 유저
        playerDataNetworkList.Add(new PlayerData
        {
            clientId = serverRpcParams.Receive.SenderClientId,
            playerClass = playerClass,
        });
        Debug.Log($"ChangePlayerClassServerRPC PlayerDataList Add complete. " +
            $"player{serverRpcParams.Receive.SenderClientId} Class: {playerClass} PlayerDataList.Count:{playerDataNetworkList.Count}");
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
    public PlayerData GetPlayerDataFromClientId(ulong clientId)
    {
        foreach (PlayerData playerData in playerDataNetworkList)
        {
            if (playerData.clientId == clientId)
            {
                return playerData;
            }
        }
        return default;
    }

    // 플레이어 Index를 단서로 PlayerData(ClientId 포함 여러 플레이어 데이터)를 찾는 메소드
    public PlayerData GetPlayerDataFromPlayerIndex(int playerIndex)
    {
        if (playerIndex >= playerDataNetworkList.Count)
        {
            Debug.Log($"playerIndex is wrong. playerIndex:{playerIndex}, listCount: {playerDataNetworkList.Count}");       
        }
        return playerDataNetworkList[playerIndex];
    }

    public bool HasAvailablePlayerSlots()
    {
        //return NetworkManager.Singleton.ConnectedClientsIds.Count < MAX_PLAYER_AMOUNT;
        return NetworkManager.Singleton.ConnectedClientsIds.Count < ConnectionApprovalHandler.MaxPlayers;
    }

    public NetworkList<PlayerData> GetPlayerDataNetworkList()
    {
        return playerDataNetworkList;
    }

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
