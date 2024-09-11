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
/// 이거 서버/클라 나눠야함./
/// UGS Start Server, Start Client
/// NetworkList 관리
/// </summary>
public class GameSingleplayer : NetworkBehaviour
{
    public static GameSingleplayer Instance { get; private set; }

    // 서버에 접속중인 플레이어들의 데이터가 담긴 리스트 <<--- 여기있는 HP같은것들. 컴포넌츠 패턴으로 인터페이스를 추가하는 식으로 각자 캐릭터의 스크립트에 붙어있습니다.  이 PlayerInGameData가 필요가 없도록 하는게 깔끔해보입니다.
    [Header("서버에 접속중인 플레이어들의 데이터가 담긴 리스트")]
    private NetworkList<PlayerInGameData> playerDataNetworkList;
    [Header("플레이어들이 보유한 장비 현황")]
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

    // --- 서버
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
        foreach (var player in playerDataNetworkList)
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
            playerDataNetworkList.RemoveAt(GetPlayerDataIndexFromClientId(clientId));

        // 모든 플레이어 레디상태 초기화
        if (GameMatchReadyManagerServer.Instance)
            GameMatchReadyManagerServer.Instance.SetEveryPlayerUnReady();

        // 남은 플레이어들이 전부 AI일 경우, 전부 강퇴 처리
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
            Debug.Log("AI만 남았습니다. 모든 AI를 퇴장시킵니다.");
            playerDataNetworkList.Clear();
            isAIPlayerAdded = false;
        }

        Debug.Log($"플레이어 {clientId}이탈. 남은 플레이어 {playerDataNetworkList.Count}명");

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

        Debug.Log($"Server_OnClientDisconnectCallback, Player Count :{playerDataNetworkList.Count}");

        // 혹시 모든 플레이어가 나갔으면, 서버도 다시 로비씬으로 돌아간다
        if (playerDataNetworkList.Count == 0)
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
        playerDataNetworkList.Clear();
#endif
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdatePlayerInGameDataServerRPC(PlayerInGameData playerData, ServerRpcParams serverRpcParams = default)
    {
        // 접속을 시도하는 인원의 ClientID가 이미 존재합니다. 
        if (GetPlayerDataFromClientId(serverRpcParams.Receive.SenderClientId).hp != 0)
        {
            Debug.Log($"플레이어{serverRpcParams.Receive.SenderClientId}는 이미 추가된 유저입니다!");
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

    // GameRoomPlayerCharacter에서 해당 인덱스의 플레이어가 접속 되었나 확인할 때 사용
    public bool IsPlayerIndexConnected(int playerIndex)
    {
        //Debug.Log($"IsPlayerIndexConnected?:{playerIndex < playerDataNetworkList.Count} playerIndex: {playerIndex}, playerDataNetworkList.Count: {playerDataNetworkList.Count}");
        return playerIndex < playerDataNetworkList.Count;
    }

    // 플레이어 clientID를 단서로 player Index를 찾는 메소드
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
        Debug.Log($"SetPlayerDataFromClientId. player.clientId: {clientId}.");
        Debug.Log($"SetPlayerDataFromClientId. playerDataNetworkList.Count: {playerDataNetworkList.Count}");
        playerDataNetworkList[GetPlayerDataIndexFromClientId(clientId)] = newPlayerData;
        //Debug.Log($"SetPlayerDataFromClientId. player.clientId:{clientId}. playerGameState:{playerDataNetworkList[GetPlayerDataIndexFromClientId(clientId)].playerGameState}");
    }

    /// <summary>
    /// 플레이어 보유 아이템 추가. 전부 서버에서 동작하는 메소드 입니다.
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

    private static T GetRandomEnumValue<T>()
    {
        Array values = Enum.GetValues(typeof(T));
        Random random = new Random();
        return (T)values.GetValue(random.Next(values.Length));
    }


    // 클라이언트 ---
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
        // 로컬에 저장되어있는 Player 정보를 생성된 서버에 저장. 
        UpdatePlayerInGameDataServerRPC(PlayerDataManager.Instance.GetPlayerInGameData());
    }
    private void Client_OnClientDisconnectCallback(ulong clientId)
    {
        Debug.Log($"OnClientDisconnectCallback : {clientId}");
        // 매칭 UI 숨김을 위한 이벤트 핸들러 호출. 
        OnFailedToJoinMatch?.Invoke(this, EventArgs.Empty);
    }
*/



    // ----------------  싱글 모드 ( Host )
    public void StartHost()
    {
        // Host 플레이어의 Player 정보를 생성된 서버에 저장. 
        //UpdatePlayerInGameData(PlayerDataManager.Instance.GetPlayerInGameData());

        NetworkManager.Singleton.StartHost();
        Debug.Log("StartHost() Host모드 서버가 실행되었습니다.");

        // 싱글모드용 게임 매니저 시작
        singlePlayerGameManager?.StartSinglePlayerGameManager();
    }
}
