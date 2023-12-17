using System;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Spawn network objects
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
    public event EventHandler OnPlayerDataNetworkListChanged;

    private void Awake()
    {
        Instance = this;

        DontDestroyOnLoad(gameObject);

        playerDataNetworkList = new NetworkList<PlayerData>();
        // GameRoomReadyManager 에서 지켜보는 EventHandler
        playerDataNetworkList.OnListChanged += PlayerDataNetworkList_OnListChanged;
        // GameRoomCharacterManager에서 지켜보는 Eventhandler
        //GameRoomCharacterManager.OnInstanceCreated += GameRoomCharacterManager_OnInstanceCreated;
    }

    //private void GameRoomCharacterManager_OnInstanceCreated(object sender, EventArgs e)
    //{
    //    GameRoomCharacterManager.instance.SetPlayerCharacter();
    //}

    private void PlayerDataNetworkList_OnListChanged(NetworkListEvent<PlayerData> changeEvent)
    {
        Debug.Log($"PlayerDataNetworkList_OnListChanged changed index: {changeEvent.Index}");
        OnPlayerDataNetworkListChanged?.Invoke(this, EventArgs.Empty);
    }

    private void Server_OnClientConnectedCallback(ulong clientId)
    {
        playerDataNetworkList.Add(new PlayerData
        {
            clientId = clientId,
        });
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

    private void Client_OnClientDisconnectCallback(ulong obj)
    {
        Debug.Log($"OnClientDisconnectCallback : {obj}");
        OnFailedToJoinGame?.Invoke(this, EventArgs.Empty);
    }

    // GameRoom에서 Client가 나갔을 때 플레이어를 없애주는 부분.
    private void Server_OnClientDisconnectCallback(ulong clientId)
    {
        for (int i = 0; i<playerDataNetworkList.Count; i++)
        {
            PlayerData playerData = playerDataNetworkList[i];
            if (playerData.clientId == clientId)
            {
                Debug.Log($"Disconnected Player clientID: {clientId},");
                Debug.Log($"playerDataNetworkList Index: {i},");
                Debug.Log($"playerDataNetworkList.Count: {playerDataNetworkList.Count},");
                Debug.Log($"playerDataNetworkList[index]: {playerDataNetworkList[i]}");

                // 플레이어 Disconnected. 해당 인덱스 데이터 삭제
                playerDataNetworkList.RemoveAt(i);               
            }
        }
    }

    // UGS Dedicated Server 
    public void StartServer()
    {
        //NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManager_ConnectionApprovalCallback;
        NetworkManager.Singleton.OnClientConnectedCallback += Server_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += Server_OnClientDisconnectCallback;
        NetworkManager.Singleton.StartServer();
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
        ChangePlayerClass(PlayerProfileData.Instance.GetCurrentSelectedClass());
    }

    // CharacterSelectPlayer에서 해당 인덱스의 플레이어가 접속 되었나 확인할 때 사용
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

    /// <summary>
    /// 클래스 변경을 서버에게 보고할 수 있습니다.
    /// 보고 시점은 Server가 Allocate 된 이후! 
    /// 즉 GameRoom에 들어가면서 입니다.
    /// </summary>
    /// <param name="playerClass"></param>    
    public void ChangePlayerClass(CharacterClasses.Class playerClass)
    {
        ChangePlayerClassServerRPC(playerClass);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangePlayerClassServerRPC(CharacterClasses.Class playerClass, ServerRpcParams serverRpcParams = default)
    {
        int playerDataIndex = GetPlayerDataIndexFromClientId(serverRpcParams.Receive.SenderClientId);
        PlayerData playerData = playerDataNetworkList[playerDataIndex];
        playerData.playerClass = playerClass;
        playerDataNetworkList[playerDataIndex] = playerData;      
    }

    /// <summary>
    /// PlayerIndex로 현재 플레이어가 선택중인 Class의 프리팹 오브젝트를 얻을 수 있습니다.
    /// 
    /// 원래는 여기서 복장상태 반영해서 반환해줘야함. 지금은 클래스만 반영해서 반환해줌
    /// 
    /// <<< 근데 이거 여기 맞나???? 클래스 분리 고려해볼 필요 있다.   
    /// </summary>
    /// <returns></returns>
    public GameObject GetPlayerClassPrefabByPlayerIndex_NotForGameSceneObject(int playerIndex)
    {       
        GameObject resultObejct = null;
        switch (GetPlayerDataFromPlayerIndex(playerIndex).playerClass)
        {
            case CharacterClasses.Class.Wizard:
                resultObejct = GameAssets.instantiate.wizard_Male_ForLobby;
                break;
            case CharacterClasses.Class.Knight:
                resultObejct = GameAssets.instantiate.knight_Male_ForLobby;
                break;
            default:
                break;
        }     
        return resultObejct;
    }
    public GameObject GetPlayerClassPrefabByPlayerIndex_ForGameSceneObject(int playerIndex)
    {
        GameObject resultObejct = null;
        switch (GetPlayerDataFromPlayerIndex(playerIndex).playerClass)
        {
            case CharacterClasses.Class.Wizard:
                resultObejct = GameAssets.instantiate.wizard_Male;
                break;
            case CharacterClasses.Class.Knight:
                resultObejct = GameAssets.instantiate.knight_Male;
                break;
            default:
                break;
        }

        return resultObejct;
    }


    //

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
