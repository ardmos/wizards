using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// UGS Start Server, Start Client
/// NetworkList ����
/// 
/// // ���� ���� ��ũ��Ʈ�� ���� �ʿ䰡 �ִ�. ���� Ŭ��/���� ��ũ��Ʈ �и� �� ����.  �� Ŭ���� �̸��� ������ �ȵ��.
/// </summary>

public class GameMultiplayer : NetworkBehaviour
{
    // ConnectionApprovalHandler���� ��
    // private const int MAX_PLAYER_AMOUNT = 4;

    // ������ �������� �÷��̾���� �����Ͱ� ��� ����Ʈ
    private NetworkList<PlayerInGameData> playerDataNetworkList;
    // �÷��̾���� ������ ��� ��Ȳ
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
    /// ������ �������� �÷��̾ ���� ��� ó�����ִ� �κ�.
    /// ����� ���ӵ��� ������ ��쿡���� ó���� ���ְ� �ֽ��ϴ�.
    /// </summary>
    /// <param name="clientId"></param>
    private void Server_OnClientDisconnectCallback(ulong clientId)
    {
        // ���������� Ȯ��.
        if (GameManager.Instance == null) { 
            Debug.Log("������ �������� ���Ӿ��� �ƴմϴ�.");
            if(GetPlayerDataIndexFromClientId(clientId) != -1) 
                playerDataNetworkList.RemoveAt(GetPlayerDataIndexFromClientId(clientId));
            return; 
        }

        // Game���̶�� GameManager���� �����ɷ� ó��. ������ ������ �ȵǰԲ� �����ҰŴϱ� �����ӽ� ó���� ���ص� �ȴ�. �ش� ����Ʈ �������� ������ �ʿ䵵 ����.
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

        // Ŭ���̾�Ʈ�� ��Ʈ��ũ�Ŵ����� ���ٰ� �ٽ� �� ��, OnNetworkSpawn�� ȣ��˴ϴ�. �� �� �Ʒ� �̺�Ʈ�� �Ǵٽ� ��ϵǱ⶧���� ������Ҹ� ���ְ��ֽ��ϴ�.
        playerDataNetworkList.OnListChanged -= OnServerListChanged;
        NetworkManager.Singleton.Shutdown();
    }

    /// <summary>
    ///  Ŭ���̾�Ʈ ������. ���� ������ �� �ϵ�
    ///  1. NetworkList�� PlayerDataList�� ���� �������� Ŭ���� ������ ����Ѵ�.
    /// </summary>
    private void Client_OnClientConnectedCallback(ulong clientId)
    {
        // ��Ī UI ������ ���� �̺�Ʈ �ڵ鷯 ȣ��
        OnSucceededToJoinMatch?.Invoke(this, EventArgs.Empty);
        // ����RPC�� ���� ������ ����
        Debug.Log($"Client_OnClientConnectedCallback. clientId: {clientId}, class: {PlayerDataManager.Instance.GetCurrentPlayerClass()}");
        //ChangePlayerClass(PlayerProfileData.Instance.GetCurrentSelectedClass());
        SavePlayerInGameDataOnServer(PlayerDataManager.Instance.GetPlayerInGameData());
    }
    private void Client_OnClientDisconnectCallback(ulong obj)
    {
        Debug.Log($"OnClientDisconnectCallback : {obj}");
        // ��Ī UI ������ ���� �̺�Ʈ �ڵ鷯 ȣ��. 
        OnFailedToJoinMatch?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// ���ÿ� ����Ǿ��ִ� Player ������ ������ ������ �����ϴ� ��ũ��Ʈ �Դϴ�.
    /// </summary>
    /// <param name="playerData"></param>
    private void SavePlayerInGameDataOnServer(PlayerInGameData playerData)
    {
        UpdatePlayerInGameDataServerRPC(playerData);
    }

    /// <summary>
    /// Ŭ���� ������ �������� ������ �� �ֽ��ϴ�. �޼ҵ�� Change ��� Update�� ���� ����ϱ�.
    /// ���� ������ Server�� Allocate �� ����! 
    /// �� GameRoom�� ���鼭 �Դϴ�.
    /// 
    /// �� �޼ҵ带 ������� �ʴ� ������� �����Ǿ����ϴ�.  �������θ� ����ؾ��մϴ�.
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
        // ���ο� ����
        playerDataNetworkList.Add(new PlayerInGameData
        {
            clientId = serverRpcParams.Receive.SenderClientId,
            characterClass = playerData.characterClass,
            playerMoveAnimState = PlayerMoveAnimState.Idle,
            playerGameState = PlayerGameState.Playing,
            playerName = playerData.playerName
            // HP�� ���� ���۵Ǹ� OnNetworkSpawn�� ���ڰ� SetPlayerHP�� ������.
        });
        Debug.Log($"ChangePlayerClassServerRPC PlayerDataList Add complete. " +
            $"player{serverRpcParams.Receive.SenderClientId} Name: {playerData.playerName} Class: {playerData.characterClass} PlayerDataList.Count:{playerDataNetworkList.Count}");
    }

    // GameRoomPlayerCharacter���� �ش� �ε����� �÷��̾ ���� �Ǿ��� Ȯ���� �� ��� <<< ���ʿ� �����. ���� �Ʒ� �޼ҵ�� ��ü ����
    public bool IsPlayerIndexConnected(int playerIndex)
    {
        //Debug.Log($"IsPlayerIndexConnected playerIndex: {playerIndex}, playerDataNetworkList.Count: {playerDataNetworkList.Count}");
        return playerIndex < playerDataNetworkList.Count;
    }

    /// <summary>
    /// �÷��̾� ���� Ȯ�� �޼ҵ�. 
    /// </summary>
    public bool IsPlayerConnected(ulong clientId)
    {
        return NetworkManager.Singleton.ConnectedClientsIds.Contains(clientId);
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

    // �÷��̾� Index�� �ܼ��� PlayerData(ClientId ���� ���� �÷��̾� ������)�� ã�� �޼ҵ�
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
    /// �������� ȣ���ؾ��ϴ� �޼ҵ�
    /// </summary>
    public void SetPlayerDataFromClientId(ulong clientId, PlayerInGameData newPlayerData)
    {
        //Debug.Log("SetPlayerDataFromClientId");
        playerDataNetworkList[GetPlayerDataIndexFromClientId(clientId)] = newPlayerData;
    }

    /// <summary>
    /// �������� ȣ���ؾ��ϴ� �޼ҵ�
    /// Ư�� �÷��̾�� �̵� �� Ư�� �ڼ�(�� ������) �ִϸ��̼��� ��������� �� �ִ� �޼ҵ� �Դϴ�.
    /// </summary>
    /// <param name="clientId">�÷��̾� ĳ���� Ư��</param>
    /// <param name="playerAnimState">�����Ű����� �ִϸ��̼� state</param>
    public void UpdatePlayerMoveAnimStateOnServer(ulong clientId, PlayerMoveAnimState playerMoveAnimState)
    {
        PlayerInGameData playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(clientId);
        playerData.playerMoveAnimState = playerMoveAnimState;
        SetPlayerDataFromClientId(clientId, playerData);
        // ���泻���� ���� ���� Player�鿡 �پ��ִ� PlayerAnimator���� �˸�.
        OnPlayerMoveAnimStateChanged?.Invoke(this, new PlayerMoveAnimStateEventData(clientId, playerData.playerMoveAnimState));
    }


    public void UpdatePlayerAttackAnimStateOnServer(ulong clientId, PlayerAttackAnimState playerAttackAnimState)
    {
        PlayerInGameData playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(clientId);
        playerData.playerAttackAnimState = playerAttackAnimState;
        SetPlayerDataFromClientId(clientId, playerData);
        // ���泻���� ���� ���� Player�鿡 �پ��ִ� PlayerAnimator���� �˸�.
        OnPlayerAttackAnimStateChanged?.Invoke(this, new PlayerAttackAnimStateEventData(clientId, playerData.playerAttackAnimState));
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
