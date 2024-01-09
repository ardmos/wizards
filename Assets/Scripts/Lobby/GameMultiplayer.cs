using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// UGS Start Server, Start Client
/// NetworkList ����
/// 
/// // ���� ���� ��ũ��Ʈ�� ���� �ʿ䰡 �ִ�. ���� Ŭ��/���� ��ũ��Ʈ �и� �� ����.
/// </summary>

public class GameMultiplayer : NetworkBehaviour
{
    // ConnectionApprovalHandler���� ��
    // private const int MAX_PLAYER_AMOUNT = 4;

    // ������ �������� �÷��̾���� �����Ͱ� ��� ����Ʈ
    private NetworkList<PlayerData> playerDataNetworkList;
    // �÷��̾���� ������ ��� ��Ȳ
    [SerializeField] private Dictionary<ulong, Dictionary<Item.ItemName, ushort>> playerItemDictionaryOnServer;    

    public static GameMultiplayer Instance { get; private set; }

    public event EventHandler OnTryingToJoinGame;
    public event EventHandler OnFailedToJoinGame;
    public event EventHandler OnPlayerListOnServerChanged;
    public event EventHandler OnPlayerMoveAnimStateChanged;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
        playerDataNetworkList = new NetworkList<PlayerData>();

        playerItemDictionaryOnServer = new Dictionary<ulong, Dictionary<Item.ItemName, ushort>>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        playerDataNetworkList.OnListChanged += OnServerListChanged;
    }

    private void OnServerListChanged(NetworkListEvent<PlayerData> changeEvent)
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

    // GameRoom���� Client�� ������ �� �÷��̾ �����ִ� �κ�.
    private void Server_OnClientDisconnectCallback(ulong clientId)
    {

        for (int i = 0; i<playerDataNetworkList.Count; i++)
        {
            PlayerData playerData = playerDataNetworkList[i];
            if (playerData.clientId == clientId)
            {              
                // Game���̶�� GameManager���� �����ɷ� ó��. ������ ������ �ȵǰԲ� �����ҰŴϱ� �����ӽ� ó���� ���ص� �ȴ�. �ش� ����Ʈ �������� ������ �ʿ䵵 ����.
                GameManager.Instance.UpdatePlayerGameOverOnServer(clientId);

                Debug.Log($"Server_OnClientDisconnectCallback");
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
    ///  Ŭ���̾�Ʈ ������. ���� ������ �� �ϵ�
    ///  1. NetworkList�� PlayerDataList�� ���� �������� Ŭ���� ������ ����Ѵ�.
    /// </summary>
    private void Client_OnClientConnectedCallback(ulong clientId)
    {
        // ����RPC�� ���� ������ ����
        Debug.Log($"Client_OnClientConnectedCallback. clientId: {clientId}, class: {PlayerProfileData.Instance.GetCurrentSelectedClass()}");
        ChangePlayerClass(PlayerProfileData.Instance.GetCurrentSelectedClass());
    }
    private void Client_OnClientDisconnectCallback(ulong obj)
    {
        Debug.Log($"OnClientDisconnectCallback : {obj}");
        OnFailedToJoinGame?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Ŭ���� ������ �������� ������ �� �ֽ��ϴ�. �޼ҵ�� Change ��� Update�� ���� ����ϱ�.
    /// ���� ������ Server�� Allocate �� ����! 
    /// �� GameRoom�� ���鼭 �Դϴ�.
    /// </summary>
    /// <param name="playerClass"></param>    
    private void ChangePlayerClass(CharacterClass playerClass)
    {
        //Debug.Log($"ChangePlayerClass. clientId: {clientId}, class: {playerClass}");
        ChangePlayerClassServerRPC(playerClass);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangePlayerClassServerRPC(CharacterClass playerClass, ServerRpcParams serverRpcParams = default)
    {
        // ���ο� ����
        playerDataNetworkList.Add(new PlayerData
        {
            clientId = serverRpcParams.Receive.SenderClientId,
            playerClass = playerClass,
            playerAnimState = PlayerMoveAnimState.Idle,
            playerGameState = PlayerGameState.Playing,
            playerName = "Default Player Name"
            // HP�� ���� ���۵Ǹ� OnNetworkSpawn�� ���ڰ� SetPlayerHP�� ������.
        });
        Debug.Log($"ChangePlayerClassServerRPC PlayerDataList Add complete. " +
            $"player{serverRpcParams.Receive.SenderClientId} Class: {playerClass} PlayerDataList.Count:{playerDataNetworkList.Count}");
    }

    // GameRoomPlayerCharacter���� �ش� �ε����� �÷��̾ ���� �Ǿ��� Ȯ���� �� ���
    public bool IsPlayerIndexConnected(int playerIndex)
    {
        //Debug.Log($"IsPlayerIndexConnected playerIndex: {playerIndex}, playerDataNetworkList.Count: {playerDataNetworkList.Count}");
        return playerIndex < playerDataNetworkList.Count;
    }

    // �÷��̾� clientID�� �ܼ��� player Index�� ã�� �޼ҵ�
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

    // �÷��̾� client�� �ܼ��� PlayerData(ClientId ���� ���� �÷��̾� ������)�� ã�� �޼ҵ�
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

    // �÷��̾� Index�� �ܼ��� PlayerData(ClientId ���� ���� �÷��̾� ������)�� ã�� �޼ҵ�
    public PlayerData GetPlayerDataFromPlayerIndex(int playerIndex)
    {
        if (playerIndex >= playerDataNetworkList.Count)
        {
            Debug.Log($"playerIndex is wrong. playerIndex:{playerIndex}, listCount: {playerDataNetworkList.Count}");       
        }
        return playerDataNetworkList[playerIndex];
    }
    
    public NetworkList<PlayerData> GetPlayerDataNetworkList()
    {
        return playerDataNetworkList;
    }

    /// <summary>
    /// �������� ȣ���ؾ��ϴ� �޼ҵ�
    /// </summary>
    public void SetPlayerDataFromClientId(ulong clientId, PlayerData newPlayerData)
    {
        //Debug.Log("SetPlayerDataFromClientId");
        playerDataNetworkList[GetPlayerDataIndexFromClientId(clientId)] = newPlayerData;
    }

    /// <summary>
    /// �������� ȣ���ؾ��ϴ� �޼ҵ�
    /// </summary>
    /// <param name="clientId"></param>
    /// <param name="playerAnimState"></param>
    public void UpdatePlayerAnimStateOnServer(ulong clientId, PlayerMoveAnimState playerAnimState)
    {
        PlayerData playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(clientId);
        playerData.playerAnimState = playerAnimState;
        SetPlayerDataFromClientId(clientId, playerData);
        // ���泻���� ���� ���� Player�鿡 �پ��ִ� PlayerAnimator���� �˸�.
        OnPlayerMoveAnimStateChanged?.Invoke(this, new PlayerAnimStateEventData(clientId, playerData.playerAnimState));
    }



    /// <summary>
    /// �÷��̾� ���� ������ �߰�. ���� �������� �����ϴ� �޼ҵ� �Դϴ�.
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

    // ��� ���ϴ� �޼���. ServerStartUp���� ó�����̴�.
    /*    public bool HasAvailablePlayerSlots()
        {
            //return NetworkManager.Singleton.ConnectedClientsIds.Count < MAX_PLAYER_AMOUNT;
            return NetworkManager.Singleton.ConnectedClientsIds.Count < ConnectionApprovalHandler.MaxPlayers;
        }*/

    /*    private void NetworkManager_ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            // GameRoom ������ Ȯ��
            if (SceneManager.GetActiveScene().name != LoadingSceneManager.Scene.GameRoomScene.ToString()) { 
                response.Approved = false;
                response.Reason = "Game has already started";
                return;
            }

            // Maximum Player Ȯ��
            if (NetworkManager.Singleton.ConnectedClientsIds.Count >= MAX_PLAYER_AMOUNT)
            {
                response.Approved = false;
                response.Reason = "Game is full";
                return;
            }

            response.Approved = true;
        }*/

    // �÷��̾� �̸� ����ִ� �κ� () ������ ��� NetworkManager_OnclientConnectedCallback���� ȣ�����ְ� Ŭ���̾�Ʈ�� ���StartClient���� ȣ���������
    // ���� �̸� ����ϴºκ��� ???? 
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
