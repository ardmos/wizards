using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using Random = System.Random;

/// <summary>
/// 서버 측 로직을 담당합니다. 플레이어 데이터 관리, 연결 및 연결 해제 처리 등을 수행합니다.
/// </summary>
public class ServerNetworkManager : NetworkBehaviour
{
    public static ServerNetworkManager Instance { get; private set; }

    // 서버에 접속중인 플레이어들의 데이터가 담긴 리스트 <<--- 여기있는 HP같은것들. 컴포넌츠 패턴으로 인터페이스를 추가하는 식으로 각자 캐릭터의 스크립트에 붙어있습니다.  이 PlayerInGameData가 필요가 없도록 하는게 깔끔해보입니다.
    [Header("서버에 접속중인 플레이어들의 데이터가 담긴 리스트")]
    public NetworkList<PlayerInGameData> currentPlayers;
    [Header("플레이어들이 보유한 장비 현황")]
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
        // 가장 먼저 접속한 클라이언트의 접속 시간 확인
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
        // 클라 접속시 AI유저들이 존재한다면, 모두 레디 시킵니다. 
        foreach (var player in currentPlayers)
        {
            if (player.isAI) GameMatchReadyManagerServer.Instance.SetAIPlayerReady(player.clientId);
        }
    }

    /// <summary>
    /// 플레이어가 나갔을 경우에대한 처리입니다.
    /// </summary>
    private void Server_OnClientDisconnectCallback(ulong clientId)
    {
        // 플레이어 이탈 처리
        if (GetPlayerDataIndexFromClientId(clientId) != -1)
            currentPlayers.RemoveAt(GetPlayerDataIndexFromClientId(clientId));

        // 모든 플레이어 레디상태 초기화
        if (GameMatchReadyManagerServer.Instance)
            GameMatchReadyManagerServer.Instance.SetEveryPlayerUnReady();

        // 남은 플레이어들이 전부 AI일 경우, 전부 강퇴 처리
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
            Debug.Log("AI만 남았습니다. 모든 AI를 퇴장시킵니다.");
            currentPlayers.Clear();
            isAIPlayerAdded = false;
        }

        Debug.Log($"플레이어 {clientId}이탈. 남은 플레이어 {currentPlayers.Count}명");

        // 게임씬이 아닌지 확인.
        if (MultiplayerGameManager.Instance == null)
        {
            Debug.Log("유저가 나갔지만 게임씬이 아닙니다.");
            return;
        }

        // 게임 씬이라면
        // 나간 clientId GameOver 처리
        if (GetPlayerDataIndexFromClientId(clientId) != -1)
            MultiplayerGameManager.Instance.UpdatePlayerGameOverOnServer(clientId);

        Debug.Log($"Server_OnClientDisconnectCallback, Player Count :{currentPlayers.Count}");

        // 혹시 모든 플레이어가 나갔으면, 서버도 다시 로비씬으로 돌아간다
        if (currentPlayers.Count == 0)
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
        // 클라이언트 빌드용 if 옵션.
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
        // 접속을 시도하는 인원의 ClientID가 이미 존재합니다. 
        if (GetPlayerDataFromClientId(serverRpcParams.Receive.SenderClientId).hp != 0)
        {
            Debug.Log($"플레이어{serverRpcParams.Receive.SenderClientId}는 이미 추가된 유저입니다!");
            return;
        }

        currentPlayers.Add(new PlayerInGameData
        {
            clientId = serverRpcParams.Receive.SenderClientId,
            // 접속시간 기록
            connectionTime = DateTime.Now,
            characterClass = playerData.characterClass,
            playerGameState = PlayerGameState.Playing,
            playerName = playerData.playerName,
            isAI = false
            // HP는 게임 시작되면 OnNetworkSpawn때 각자가 SetPlayerHP로 보고함.
        });
        Debug.Log($"GameMultiplayer.PlayerDataList Add complete. " +
            $"player{serverRpcParams.Receive.SenderClientId} Name: {playerData.playerName} Class: {playerData.characterClass} PlayerDataList.Count:{currentPlayers.Count}");
    }

    /// <summary>
    /// AI 플레이어 추가 전용 메서드
    /// </summary>
    /// <param name="playerData"></param>
    private void AddAIPlayer(PlayerInGameData playerData)
    {
        currentPlayers.Add(playerData);
        Debug.Log($"AI 추가를 완료했습니다. " +
            $"AI{playerData.clientId} Name: {playerData.playerName} Class: {playerData.characterClass} PlayerDataList.Count:{currentPlayers.Count}");
    }

    // GameRoomPlayerCharacter에서 해당 인덱스의 플레이어가 접속 되었나 확인할 때 사용
    public bool IsPlayerIndexConnected(int playerIndex)
    {
        //Debug.Log($"IsPlayerIndexConnected?:{playerIndex < playerDataNetworkList.Count} playerIndex: {playerIndex}, playerDataNetworkList.Count: {playerDataNetworkList.Count}");
        return playerIndex < currentPlayers.Count;
    }

    // 플레이어 clientID를 단서로 player Index를 찾는 메소드
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

    // 플레이어 client를 단서로 PlayerData(ClientId 포함 여러 플레이어 데이터)를 찾는 메소드
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

    // 플레이어 Index를 단서로 PlayerData(ClientId 포함 여러 플레이어 데이터)를 찾는 메소드
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
    /// 서버에서 호출해야하는 메소드
    /// </summary>
    public void SetPlayerDataFromClientId(ulong clientId, PlayerInGameData newPlayerData)
    {
        Debug.Log($"SetPlayerDataFromClientId. player.clientId: {clientId}.");
        Debug.Log($"SetPlayerDataFromClientId. currentPlayers.Count: {currentPlayers.Count}");
        currentPlayers[GetPlayerDataIndexFromClientId(clientId)] = newPlayerData;
        //Debug.Log($"SetPlayerDataFromClientId. player.clientId:{clientId}. playerGameState:{playerDataNetworkList[GetPlayerDataIndexFromClientId(clientId)].playerGameState}");
    }

    /// <summary>
    /// 플레이어 스코어 추가.
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
    /// 플레이어 스코어 얻기.
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
            //Debug.Log($"최초 접속자player{firstPlayer.clientId}는 접속한지 {passedTime} 지났습니다.");
            // 지정 시간( 테스트에서는 10초.) 이후 자동 시작. 빈자리는 AI로 채워준다
            if (passedTime > TimeSpan.FromSeconds(10))
            {
                Debug.Log($"The first connected player{firstPlayer.clientId} has been connected for more than 10 seconds.");
                isAIPlayerAdded = true;

                // AI 유저 채우고 (지금은 전부 Wizard Ruke, wizard ruke ai 의 스펙 지금 여기서 설정해주고있음. 테스트 후 수정 필요)
                ulong availablePlayerSlots = (ulong)(ConnectionApprovalHandler.MaxPlayers - NetworkManager.Singleton.ConnectedClients.Count);
                Debug.Log($"현재 접속한 플레이어 {NetworkManager.Singleton.ConnectedClients.Count}명. 최대 {ConnectionApprovalHandler.MaxPlayers}명에서 {availablePlayerSlots}명이 모자랍니다. 모자란만큼 AI 플레이어를 생성합니다.");
                //lastClientId = NetworkManager.Singleton.ConnectedClientsIds[NetworkManager.Singleton.ConnectedClientsIds.Count - 1];
                lastClientId = 10000;
                Debug.Log($"마지막 플레이어의 ID: {lastClientId}");
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
                    // 추가한 AI 플레이어들 레디한걸로 등록 시키기
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
