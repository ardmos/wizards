using System;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Spawn network objects
/// NetworkList 包府
/// </summary>

public class GameMultiplayer : NetworkBehaviour
{
    // ConnectionApprovalHandler俊辑 窃
    // private const int MAX_PLAYER_AMOUNT = 4;

    // 辑滚俊 立加吝牢 敲饭捞绢甸狼 单捞磐啊 淬变 府胶飘
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
        // GameRoomReadyManager 俊辑 瘤难焊绰 EventHandler
        playerDataNetworkList.OnListChanged += PlayerDataNetworkList_OnListChanged;
        // GameRoomCharacterManager俊辑 瘤难焊绰 Eventhandler
        //GameRoomCharacterManager.OnInstanceCreated += GameRoomCharacterManager_OnInstanceCreated;
    }

    //private void GameRoomCharacterManager_OnInstanceCreated(object sender, EventArgs e)
    //{
    //    GameRoomCharacterManager.instance.SetPlayerCharacter();
    //}

    private void PlayerDataNetworkList_OnListChanged(NetworkListEvent<PlayerData> changeEvent)
    {
        OnPlayerDataNetworkListChanged?.Invoke(this, EventArgs.Empty);
    }

    private void NetworkManager_OnClientConnectedCallback(ulong clientId)
    {
        playerDataNetworkList.Add(new PlayerData
        {
            clientId = clientId,
        });
        GameRoomCharacterManager.instance.UpdatePlayerCharacter();
    }

/*    private void NetworkManager_ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        // GameRoom 纠牢瘤 犬牢
        if (SceneManager.GetActiveScene().name != LoadingSceneManager.Scene.GameRoomScene.ToString()) { 
            response.Approved = false;
            response.Reason = "Game has already started";
            return;
        }

        // Maximum Player 犬牢
        if (NetworkManager.Singleton.ConnectedClientsIds.Count >= MAX_PLAYER_AMOUNT)
        {
            response.Approved = false;
            response.Reason = "Game is full";
            return;
        }

        response.Approved = true;
    }*/

    private void NetworkManager_Client_OnClientDisconnectCallback(ulong obj)
    {
        Debug.Log($"OnClientDisconnectCallback : {obj}");
        OnFailedToJoinGame?.Invoke(this, EventArgs.Empty);
    }

    // GameRoom俊辑 Client啊 唱艾阑 锭 敲饭捞绢甫 绝局林绰 何盒.
    private void NetworkManager_Server_OnClientDisconnectCallback(ulong clientId)
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

                // 敲饭捞绢 Disconnected. 秦寸 牢郸胶 单捞磐 昏力
                playerDataNetworkList.RemoveAt(i);

                // 한 번 더 GameRoom 캐릭터 Visual 업데이트. 
                GameRoomCharacterManager.instance.UpdatePlayerCharacter();
            }
        }
    }

    // UGS Dedicated Server 
    public void StartServer()
    {
        //NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManager_ConnectionApprovalCallback;
        NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_Server_OnClientDisconnectCallback;
        NetworkManager.Singleton.StartServer();
    }

    public void StartClient()
    {
        OnTryingToJoinGame?.Invoke(this, EventArgs.Empty);
        NetworkManager.Singleton.OnClientConnectedCallback += Client_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_Client_OnClientDisconnectCallback;
        NetworkManager.Singleton.StartClient();
    }
    /// <summary>
    ///  努扼捞攫飘 螟俊辑. 立加 己傍矫 且 老甸
    ///  1. NetworkList牢 PlayerDataList俊 泅犁 急琶吝牢 努贰胶 沥焊甫 殿废茄促.
    /// </summary>
    private void Client_OnClientConnectedCallback(ulong clientId)
    {
        // 辑滚RPC甫 烹秦 辑滚俊 历厘
        ChangePlayerClass(PlayerProfileData.Instance.GetCurrentSelectedClass());
    }

    // CharacterSelectPlayer俊辑 秦寸 牢郸胶狼 敲饭捞绢啊 立加 登菌唱 犬牢且 锭 荤侩
    public bool IsPlayerIndexConnected(int playerIndex)
    {
        return playerIndex < playerDataNetworkList.Count;
    }

    // 敲饭捞绢 clientID甫 窜辑肺 player Index甫 茫绰 皋家靛
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

    // 敲饭捞绢 client甫 窜辑肺 PlayerData(ClientId 器窃 咯矾 敲饭捞绢 单捞磐)甫 茫绰 皋家靛
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

    // 敲饭捞绢 Index甫 窜辑肺 PlayerData(ClientId 器窃 咯矾 敲饭捞绢 单捞磐)甫 茫绰 皋家靛
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
    /// 努贰胶 函版阑 辑滚俊霸 焊绊且 荐 乐嚼聪促.
    /// 焊绊 矫痢篮 Server啊 Allocate 等 捞饶! 
    /// 溜 GameRoom俊 甸绢啊搁辑 涝聪促.
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
    /// PlayerIndex肺 泅犁 敲饭捞绢啊 急琶吝牢 Class狼 橇府普 坷宏璃飘甫 掘阑 荐 乐嚼聪促.
    /// 
    /// 盔贰绰 咯扁辑 汗厘惑怕 馆康秦辑 馆券秦拎具窃. 瘤陛篮 努贰胶父 馆康秦辑 馆券秦淋
    /// 
    /// <<< 辟单 捞芭 咯扁 嘎唱???? 努贰胶 盒府 绊妨秦杭 鞘夸 乐促.   
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

    // 敲饭捞绢 捞抚 剁况林绰 何盒 () 辑滚老 版快 NetworkManager_OnclientConnectedCallback俊辑 龋免秦林绊 努扼捞攫飘老 版快StartClient俊辑 龋免秦拎具窃
    // 弥檬 捞抚 殿废窍绰何盒篮 ???? 
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
