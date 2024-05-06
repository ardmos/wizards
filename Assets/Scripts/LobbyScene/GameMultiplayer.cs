using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// �̰� ����/Ŭ�� ��������./
/// UGS Start Server, Start Client
/// NetworkList ����
/// </summary>
public class GameMultiplayer : NetworkBehaviour
{
    // ������ �������� �÷��̾���� �����Ͱ� ��� ����Ʈ <<--- �����ִ� HP�����͵�. �������� �������� �������̽��� �߰��ϴ� ������ ���� ĳ������ ��ũ��Ʈ�� �پ��ֽ��ϴ�.  �� PlayerInGameData�� �ʿ䰡 ������ �ϴ°� ����غ��Դϴ�.
    private NetworkList<PlayerInGameData> playerDataNetworkList;
    // �÷��̾���� ������ ��� ��Ȳ
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

    // --- ����
    // UGS Dedicated Server
    public void StartServer()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback += Server_OnClientDisconnectCallback;
        NetworkManager.Singleton.StartServer();
    }

    /// <summary>
    /// ���ӵ��� �÷��̾ ������ ��쿡���� ó���Դϴ�.
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


        // �÷��̾� ��Ż ó��
        if (GetPlayerDataIndexFromClientId(clientId) != -1)
            playerDataNetworkList.RemoveAt(GetPlayerDataIndexFromClientId(clientId));


        Debug.Log($"Server_OnClientDisconnectCallback, Player Count :{playerDataNetworkList.Count}");
        // Ȥ�� ��� �÷��̾ ��������, ������ �ٽ� �κ������ ���ư���
        if (playerDataNetworkList.Count == 0)
        {
            Debug.Log($"Server_OnClientDisconnectCallback, Go to Lobby");
            CleanUp();
            // �κ������ �̵�
            LoadSceneManager.Load(LoadSceneManager.Scene.LobbyScene);
        }

    }
    // �κ������ ���Ʊ� �� �ʱ�ȭ
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
        playerDataNetworkList[GetPlayerDataIndexFromClientId(clientId)] = newPlayerData;
        //Debug.Log($"SetPlayerDataFromClientId. player.clientId:{clientId}. playerGameState:{playerDataNetworkList[GetPlayerDataIndexFromClientId(clientId)].playerGameState}");
    }

    /// <summary>
    /// �������� ȣ���ؾ��ϴ� �޼ҵ�. �÷��̾��� �̵� ó���κ��� ����ϴ� ��ũ��Ʈ�� ���� ����°� ���ƺ��δ�!! ���߿� �ڵ� ������ �� �����ؼ� ��������.
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

    /// <summary>
    /// �÷��̾� ���� ������ �߰�. ���� �������� �����ϴ� �޼ҵ� �Դϴ�.
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
    /// �÷��̾� ���ھ� �߰�.
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
    /// �÷��̾� ���ھ� ���.
    /// </summary>
    /// <param name="clientID"></param>
    /// <returns></returns>
    public int GetPlayerScore(ulong clientID)
    {
        Debug.Log($"GetPlayerScore player{clientID} requested.");
        return GetPlayerDataFromClientId(clientID).score;
    }

    // Ŭ���̾�Ʈ ---

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
}
