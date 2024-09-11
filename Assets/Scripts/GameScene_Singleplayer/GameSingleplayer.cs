using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using Random = System.Random;

/// <summary>
/// �̰� ����/Ŭ�� ��������./
/// UGS Start Server, Start Client
/// NetworkList ����
/// </summary>
public class GameSingleplayer : NetworkBehaviour
{
    public static GameSingleplayer Instance { get; private set; }

    // ������ �������� �÷��̾���� �����Ͱ� ��� ����Ʈ <<--- �����ִ� HP�����͵�. �������� �������� �������̽��� �߰��ϴ� ������ ���� ĳ������ ��ũ��Ʈ�� �پ��ֽ��ϴ�.  �� PlayerInGameData�� �ʿ䰡 ������ �ϴ°� ����غ��Դϴ�.
    [Header("������ �������� �÷��̾���� �����Ͱ� ��� ����Ʈ")]
    private NetworkList<PlayerInGameData> playerDataNetworkList;
    [Header("�÷��̾���� ������ ��� ��Ȳ")]
    [SerializeField] private Dictionary<ulong, Dictionary<ItemName, ushort>> playerItemDictionaryOnServer;

    public event EventHandler OnSucceededToJoinMatch;
    public event EventHandler OnFailedToJoinMatch;
    public event EventHandler OnPlayerListOnServerChanged;
    public event EventHandler OnPlayerMoveAnimStateChanged;
    //public event EventHandler OnPlayerAttackAnimStateChanged;

    public SingleplayerGameManager singlePlayerGameManager;

    [SerializeField] private bool isAIPlayerAdded;
    private ulong lastClientId;

    private void Awake()
    {
        Debug.Log("GameSingleplayer.Awake()");
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitGameMultiplayer();
    }

    private void InitGameMultiplayer()
    {
        Debug.Log($"1. GameSingleplayer Awake() playerDataNetworkList : {playerDataNetworkList}");
        playerDataNetworkList = new NetworkList<PlayerInGameData>();
        Debug.Log($"2. GameSingleplayer Awake() playerDataNetworkList : {playerDataNetworkList}");

        playerItemDictionaryOnServer = new Dictionary<ulong, Dictionary<ItemName, ushort>>();

        isAIPlayerAdded = false;
    }

    public override void OnNetworkSpawn()
    {
        //Debug.Log($"GameSingleplayer OnNetworkSpawn() IsServer:{IsServer}, IsHost:{IsHost}, IsClient:{IsClient}");
        playerDataNetworkList.OnListChanged += OnServerListChanged;
    }

    public override void OnNetworkDespawn()
    {
        //Debug.Log("OnNetworkDespawn()");
        playerDataNetworkList.OnListChanged -= OnServerListChanged;

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= Server_OnClientConnectedCallback;
            NetworkManager.Singleton.OnClientDisconnectCallback -= Server_OnClientDisconnectCallback;
        }
    }

    private void OnServerListChanged(NetworkListEvent<PlayerInGameData> changeEvent)
    {
        OnPlayerListOnServerChanged?.Invoke(this, EventArgs.Empty);
    }

    // --- ����
    // UGS Dedicated Server
    public void StartServer()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += Server_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += Server_OnClientDisconnectCallback;
        NetworkManager.Singleton.StartServer();
    }

    private void Server_OnClientConnectedCallback(ulong obj)
    {
        // Ŭ�� ���ӽ� AI�������� �����Ѵٸ�, ��� ���� ��ŵ�ϴ�. 
        foreach (var player in playerDataNetworkList)
        {
            if (player.isAI) GameMatchReadyManagerServer.Instance.SetAIPlayerReady(player.clientId);
        }
    }

    /// <summary>
    /// �÷��̾ ������ ��쿡���� ó���Դϴ�.
    /// </summary>
    private void Server_OnClientDisconnectCallback(ulong clientId)
    {
        // �÷��̾� ��Ż ó��
        if (GetPlayerDataIndexFromClientId(clientId) != -1)
            playerDataNetworkList.RemoveAt(GetPlayerDataIndexFromClientId(clientId));

        // ��� �÷��̾� ������� �ʱ�ȭ
        if (GameMatchReadyManagerServer.Instance)
            GameMatchReadyManagerServer.Instance.SetEveryPlayerUnReady();

        // ���� �÷��̾���� ���� AI�� ���, ���� ���� ó��
        bool isEveryPlayerisAI = true;
        foreach (PlayerInGameData player in playerDataNetworkList)
        {
            if (!player.isAI)
            {
                isEveryPlayerisAI = false;
            }
        }
        if (isEveryPlayerisAI)
        {
            Debug.Log("AI�� ���ҽ��ϴ�. ��� AI�� �����ŵ�ϴ�.");
            playerDataNetworkList.Clear();
            isAIPlayerAdded = false;
        }

        Debug.Log($"�÷��̾� {clientId}��Ż. ���� �÷��̾� {playerDataNetworkList.Count}��");

        // ���Ӿ��� �ƴ��� Ȯ��.
        if (MultiplayerGameManager.Instance == null)
        {
            Debug.Log("������ �������� ���Ӿ��� �ƴմϴ�.");
            return;
        }

        // ���� ���̶��
        // ���� clientId GameOver ó��
        if (GetPlayerDataIndexFromClientId(clientId) != -1)
            MultiplayerGameManager.Instance.UpdatePlayerGameOverOnServer(clientId);

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
        // Ŭ���̾�Ʈ ����� if �ɼ�.
#if UNITY_SERVER || UNITY_EDITOR
        if (MultiplayerGameManager.Instance != null)
        {
            MultiplayerGameManager.Instance.CleanUpObjects();
        }
        if (NetworkManager.Singleton != null)
        {
            //Debug.Log("CleanUp NetworkManager!");
            NetworkManager.Singleton.Shutdown();
            //NetworkManager.Shutdown();
            Destroy(NetworkManager.Singleton.gameObject);
        }
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        if (ServerStartUp.Instance != null)
        {
            Destroy(ServerStartUp.Instance.gameObject);
        }
        playerDataNetworkList.Clear();
#endif
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdatePlayerInGameDataServerRPC(PlayerInGameData playerData, ServerRpcParams serverRpcParams = default)
    {
        // ������ �õ��ϴ� �ο��� ClientID�� �̹� �����մϴ�. 
        if (GetPlayerDataFromClientId(serverRpcParams.Receive.SenderClientId).hp != 0)
        {
            Debug.Log($"�÷��̾�{serverRpcParams.Receive.SenderClientId}�� �̹� �߰��� �����Դϴ�!");
            return;
        }

        AddPlayer(playerData);

        Debug.Log($"GameSingleplayer.PlayerDataList Add complete. " +
            $"player{serverRpcParams.Receive.SenderClientId} Name: {playerData.playerName} Class: {playerData.characterClass} PlayerDataList.Count:{playerDataNetworkList.Count}");
    }

    public void AddPlayer(PlayerInGameData playerData)
    {
        playerDataNetworkList.Add(playerData);
    }

    // GameRoomPlayerCharacter���� �ش� �ε����� �÷��̾ ���� �Ǿ��� Ȯ���� �� ���
    public bool IsPlayerIndexConnected(int playerIndex)
    {
        //Debug.Log($"IsPlayerIndexConnected?:{playerIndex < playerDataNetworkList.Count} playerIndex: {playerIndex}, playerDataNetworkList.Count: {playerDataNetworkList.Count}");
        return playerIndex < playerDataNetworkList.Count;
    }

    // �÷��̾� clientID�� �ܼ��� player Index�� ã�� �޼ҵ�
    public int GetPlayerDataIndexFromClientId(ulong clientId)
    {
        //Debug.Log($"GetPlayerDataIndexFromClientId, requested {clientId}");
        for (int i = 0; i < playerDataNetworkList.Count; i++)
        {
            //Debug.Log($"playerDataNetworkList[{i}].playerName : {playerDataNetworkList[i].playerName}");
            //Debug.Log($"playerDataNetworkList[{i}].clientId : {playerDataNetworkList[i].clientId}");
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
        Debug.Log($"SetPlayerDataFromClientId. player.clientId: {clientId}.");
        Debug.Log($"SetPlayerDataFromClientId. playerDataNetworkList.Count: {playerDataNetworkList.Count}");
        playerDataNetworkList[GetPlayerDataIndexFromClientId(clientId)] = newPlayerData;
        //Debug.Log($"SetPlayerDataFromClientId. player.clientId:{clientId}. playerGameState:{playerDataNetworkList[GetPlayerDataIndexFromClientId(clientId)].playerGameState}");
    }

    /// <summary>
    /// �÷��̾� ���� ������ �߰�. ���� �������� �����ϴ� �޼ҵ� �Դϴ�.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
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

        if (playerItemDictionaryOnServer[serverRpcParams.Receive.SenderClientId][itemName] > 0)
            playerItemDictionaryOnServer[serverRpcParams.Receive.SenderClientId][itemName]--;
        else
            playerItemDictionaryOnServer[serverRpcParams.Receive.SenderClientId][itemName] = 0;
    }

    /// <summary>
    /// �÷��̾� ���ھ� �߰�.
    /// </summary>
    /// <param name="clientID"></param>
    /// <param name="score"></param>
    public void AddPlayerScore(ulong clientID, int score)
    {
        //Debug.Log($"AddPlayerScore. requested clientID : {clientID}");

        PlayerInGameData playerData = GetPlayerDataFromClientId(clientID);
        //Debug.Log($"1.player:{playerData.clientId}'s score:{playerData.score}");
        playerData.score += score;

        SetPlayerDataFromClientId(clientID, playerData);
        //Debug.Log($"2.player:{playerData.clientId}'s score:{playerData.score}, getScore:{GetPlayerScore(clientID)}");
    }

    /// <summary>
    /// �÷��̾� ���ھ� ���.
    /// </summary>
    /// <param name="clientID"></param>
    /// <returns></returns>
    public int GetPlayerScore(ulong clientID)
    {
        Debug.Log($"GetPlayerScore player{clientID} requested. {GetPlayerDataFromClientId(clientID).score}");
        return GetPlayerDataFromClientId(clientID).score;
    }

    private static T GetRandomEnumValue<T>()
    {
        Array values = Enum.GetValues(typeof(T));
        Random random = new Random();
        return (T)values.GetValue(random.Next(values.Length));
    }


    // Ŭ���̾�Ʈ ---
/*
    public void StartClient()
    {
        Debug.Log("StartClient()");
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
        // ���ÿ� ����Ǿ��ִ� Player ������ ������ ������ ����. 
        UpdatePlayerInGameDataServerRPC(PlayerDataManager.Instance.GetPlayerInGameData());
    }
    private void Client_OnClientDisconnectCallback(ulong clientId)
    {
        Debug.Log($"OnClientDisconnectCallback : {clientId}");
        // ��Ī UI ������ ���� �̺�Ʈ �ڵ鷯 ȣ��. 
        OnFailedToJoinMatch?.Invoke(this, EventArgs.Empty);
    }
*/



    // ----------------  �̱� ��� ( Host )
    public void StartHost()
    {
        // Host �÷��̾��� Player ������ ������ ������ ����. 
        //UpdatePlayerInGameData(PlayerDataManager.Instance.GetPlayerInGameData());

        NetworkManager.Singleton.StartHost();
        Debug.Log("StartHost() Host��� ������ ����Ǿ����ϴ�.");

        // �̱۸��� ���� �Ŵ��� ����
        singlePlayerGameManager?.StartSinglePlayerGameManager();
    }
}
