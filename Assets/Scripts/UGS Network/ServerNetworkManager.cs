using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using Random = System.Random;

/// <summary>
/// ���� �� ������ ����մϴ�. �÷��̾� ������ ����, ���� �� ���� ���� ó�� ���� �����մϴ�.
/// </summary>
public class ServerNetworkManager : NetworkBehaviour
{
    public static ServerNetworkManager Instance { get; private set; }

    // ������ �������� �÷��̾���� �����Ͱ� ��� ����Ʈ <<--- �����ִ� HP�����͵�. �������� �������� �������̽��� �߰��ϴ� ������ ���� ĳ������ ��ũ��Ʈ�� �پ��ֽ��ϴ�.  �� PlayerInGameData�� �ʿ䰡 ������ �ϴ°� ����غ��Դϴ�.
    [Header("������ �������� �÷��̾���� �����Ͱ� ��� ����Ʈ")]
    public NetworkList<PlayerInGameData> currentPlayers;
    [Header("�÷��̾���� ������ ��� ��Ȳ")]
    [SerializeField] private Dictionary<ulong, Dictionary<ItemName, ushort>> playerItemDictionaryOnServer;

    public event EventHandler OnCurrentPlayerListOnServerChanged;
    public event EventHandler OnPlayerMoveAnimStateChanged;

    [SerializeField] private bool isAIPlayerAdded;
    private ulong lastClientId;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitGameMultiplayer();
    }

    private void InitGameMultiplayer()
    {
        currentPlayers = new NetworkList<PlayerInGameData>();

        playerItemDictionaryOnServer = new Dictionary<ulong, Dictionary<ItemName, ushort>>();

        isAIPlayerAdded = false;
    }

    private void Update()
    {
        if (!IsServer) return;
        // ���� ���� ������ Ŭ���̾�Ʈ�� ���� �ð� Ȯ��
        CheckFirstPlayerConnectionTime();
    }

    public override void OnNetworkSpawn()
    {
        currentPlayers.OnListChanged += OnCurrentPlayerListChanged;
    }

    public override void OnNetworkDespawn()
    {
        currentPlayers.OnListChanged -= OnCurrentPlayerListChanged;

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= Server_OnClientConnectedCallback;
            NetworkManager.Singleton.OnClientDisconnectCallback -= Server_OnClientDisconnectCallback;
        }
    }

    private void OnCurrentPlayerListChanged(NetworkListEvent<PlayerInGameData> changeEvent)
    {
        OnCurrentPlayerListOnServerChanged?.Invoke(this, EventArgs.Empty);
    }

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
        foreach (var player in currentPlayers)
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
            currentPlayers.RemoveAt(GetPlayerDataIndexFromClientId(clientId));

        // ��� �÷��̾� ������� �ʱ�ȭ
        if (GameMatchReadyManagerServer.Instance)
            GameMatchReadyManagerServer.Instance.SetEveryPlayerUnReady();

        // ���� �÷��̾���� ���� AI�� ���, ���� ���� ó��
        bool isEveryPlayerisAI = true;
        foreach (PlayerInGameData player in currentPlayers)
        {
            if (!player.isAI)
            {
                isEveryPlayerisAI = false;
            }
        }
        if (isEveryPlayerisAI)
        {
            Debug.Log("AI�� ���ҽ��ϴ�. ��� AI�� �����ŵ�ϴ�.");
            currentPlayers.Clear();
            isAIPlayerAdded = false;
        }

        Debug.Log($"�÷��̾� {clientId}��Ż. ���� �÷��̾� {currentPlayers.Count}��");

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

        Debug.Log($"Server_OnClientDisconnectCallback, Player Count :{currentPlayers.Count}");

        // Ȥ�� ��� �÷��̾ ��������, ������ �ٽ� �κ������ ���ư���
        if (currentPlayers.Count == 0)
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
        currentPlayers.Clear();
#endif
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdatePlayerInGameDataServerRPC(PlayerInGameData playerData, ServerRpcParams serverRpcParams = default)
    {
        // ������ �õ��ϴ� �ο��� ClientID�� �̹� �����մϴ�. 
        if (GetPlayerDataFromClientId(serverRpcParams.Receive.SenderClientId).hp != 0)
        {
            Debug.Log($"�÷��̾�{serverRpcParams.Receive.SenderClientId}�� �̹� �߰��� �����Դϴ�!");
            return;
        }

        currentPlayers.Add(new PlayerInGameData
        {
            clientId = serverRpcParams.Receive.SenderClientId,
            // ���ӽð� ���
            connectionTime = DateTime.Now,
            characterClass = playerData.characterClass,
            playerGameState = PlayerGameState.Playing,
            playerName = playerData.playerName,
            isAI = false
            // HP�� ���� ���۵Ǹ� OnNetworkSpawn�� ���ڰ� SetPlayerHP�� ������.
        });
        Debug.Log($"GameMultiplayer.PlayerDataList Add complete. " +
            $"player{serverRpcParams.Receive.SenderClientId} Name: {playerData.playerName} Class: {playerData.characterClass} PlayerDataList.Count:{currentPlayers.Count}");
    }

    /// <summary>
    /// AI �÷��̾� �߰� ���� �޼���
    /// </summary>
    /// <param name="playerData"></param>
    private void AddAIPlayer(PlayerInGameData playerData)
    {
        currentPlayers.Add(playerData);
        Debug.Log($"AI �߰��� �Ϸ��߽��ϴ�. " +
            $"AI{playerData.clientId} Name: {playerData.playerName} Class: {playerData.characterClass} PlayerDataList.Count:{currentPlayers.Count}");
    }

    // GameRoomPlayerCharacter���� �ش� �ε����� �÷��̾ ���� �Ǿ��� Ȯ���� �� ���
    public bool IsPlayerIndexConnected(int playerIndex)
    {
        //Debug.Log($"IsPlayerIndexConnected?:{playerIndex < playerDataNetworkList.Count} playerIndex: {playerIndex}, playerDataNetworkList.Count: {playerDataNetworkList.Count}");
        return playerIndex < currentPlayers.Count;
    }

    // �÷��̾� clientID�� �ܼ��� player Index�� ã�� �޼ҵ�
    public int GetPlayerDataIndexFromClientId(ulong clientId)
    {
        //Debug.Log($"GetPlayerDataIndexFromClientId, requested {clientId}");
        for (int i = 0; i < currentPlayers.Count; i++)
        {
            //Debug.Log($"playerDataNetworkList[{i}].playerName : {playerDataNetworkList[i].playerName}");
            //Debug.Log($"playerDataNetworkList[{i}].clientId : {playerDataNetworkList[i].clientId}");
            if (currentPlayers[i].clientId == clientId)
            {
                return i;
            }
        }
        return -1;
    }

    // �÷��̾� client�� �ܼ��� PlayerData(ClientId ���� ���� �÷��̾� ������)�� ã�� �޼ҵ�
    public PlayerInGameData GetPlayerDataFromClientId(ulong clientId)
    {
        foreach (PlayerInGameData playerData in currentPlayers)
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
        if (playerIndex >= currentPlayers.Count)
        {
            Debug.Log($"playerIndex is wrong. playerIndex:{playerIndex}, listCount: {currentPlayers.Count}");
        }
        return currentPlayers[playerIndex];
    }

    public NetworkList<PlayerInGameData> GetPlayerDataNetworkList()
    {
        return currentPlayers;
    }

    public byte GetPlayerCount()
    {
        return (byte)currentPlayers.Count;
    }

    /// <summary>
    /// �������� ȣ���ؾ��ϴ� �޼ҵ�
    /// </summary>
    public void SetPlayerDataFromClientId(ulong clientId, PlayerInGameData newPlayerData)
    {
        Debug.Log($"SetPlayerDataFromClientId. player.clientId: {clientId}.");
        Debug.Log($"SetPlayerDataFromClientId. currentPlayers.Count: {currentPlayers.Count}");
        currentPlayers[GetPlayerDataIndexFromClientId(clientId)] = newPlayerData;
        //Debug.Log($"SetPlayerDataFromClientId. player.clientId:{clientId}. playerGameState:{playerDataNetworkList[GetPlayerDataIndexFromClientId(clientId)].playerGameState}");
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

    private void CheckFirstPlayerConnectionTime()
    {
        if (currentPlayers.Count > 0 && !isAIPlayerAdded)
        {
            PlayerInGameData firstPlayer = currentPlayers[0];
            var passedTime = DateTime.Now - firstPlayer.connectionTime;
            //Debug.Log($"���� ������player{firstPlayer.clientId}�� �������� {passedTime} �������ϴ�.");
            // ���� �ð�( �׽�Ʈ������ 10��.) ���� �ڵ� ����. ���ڸ��� AI�� ä���ش�
            if (passedTime > TimeSpan.FromSeconds(10))
            {
                Debug.Log($"The first connected player{firstPlayer.clientId} has been connected for more than 10 seconds.");
                isAIPlayerAdded = true;

                // AI ���� ä��� (������ ���� Wizard Ruke, wizard ruke ai �� ���� ���� ���⼭ �������ְ�����. �׽�Ʈ �� ���� �ʿ�)
                ulong availablePlayerSlots = (ulong)(ConnectionApprovalHandler.MaxPlayers - NetworkManager.Singleton.ConnectedClients.Count);
                Debug.Log($"���� ������ �÷��̾� {NetworkManager.Singleton.ConnectedClients.Count}��. �ִ� {ConnectionApprovalHandler.MaxPlayers}���� {availablePlayerSlots}���� ���ڶ��ϴ�. ���ڶ���ŭ AI �÷��̾ �����մϴ�.");
                //lastClientId = NetworkManager.Singleton.ConnectedClientsIds[NetworkManager.Singleton.ConnectedClientsIds.Count - 1];
                lastClientId = 10000;
                Debug.Log($"������ �÷��̾��� ID: {lastClientId}");
                for (ulong aiClientId = lastClientId + 1; aiClientId <= lastClientId + availablePlayerSlots; aiClientId++)
                {
                    PlayerInGameData aiPlayerInGameData_WizardRuke = new PlayerInGameData()
                    {
                        clientId = aiClientId,
                        playerGameState = PlayerGameState.Playing,
                        playerName = GenerateRandomAIPlayerName(),
                        characterClass = Character.Wizard,
                        hp = 5,
                        maxHp = 5,
                        moveSpeed = 4,
                        isAI = true,
                    };

                    AddAIPlayer(aiPlayerInGameData_WizardRuke);
                    // �߰��� AI �÷��̾�� �����Ѱɷ� ��� ��Ű��
                    GameMatchReadyManagerServer.Instance.SetAIPlayerReady(aiClientId);
                }

            }
        }
    }

    public static string GenerateRandomAIPlayerName()
    {
        AIPlayerName randomName = GetRandomEnumValue<AIPlayerName>();
        AIPlayerTitle randomTitle = GetRandomEnumValue<AIPlayerTitle>();

        return $"{randomTitle} {randomName}";
    }

    private static T GetRandomEnumValue<T>()
    {
        Array values = Enum.GetValues(typeof(T));
        Random random = new Random();
        return (T)values.GetValue(random.Next(values.Length));
    }

    public ulong GetLastClientId()
    {
        return lastClientId;
    }

}
