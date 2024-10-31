using Unity.Netcode;

/// <summary>
/// 서버 측 로직을 담당합니다. 플레이어 데이터 관리, 연결 및 연결 해제 처리 등을 수행합니다.
/// </summary>
public class ServerNetworkConnectionManager : NetworkBehaviour
{
    public static ServerNetworkConnectionManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null && IsServer)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= Server_OnClientConnectedCallback;
            NetworkManager.Singleton.OnClientDisconnectCallback -= Server_OnClientDisconnectCallback;
        }
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
        if (GameMatchReadyManagerServer.Instance == null) return;
        // 새로운 유저가 접속하면 접속중이던 모든 AI유저들을 레디 상태로 만듭니다
        GameMatchReadyManagerServer.Instance.SetEveryAIPlayersReady();
    }

    /// <summary>
    /// 플레이어가 나간 경우에대한 처리입니다.
    /// </summary>
    private void Server_OnClientDisconnectCallback(ulong clientId)
    {
        if (CurrentPlayerDataManager.Instance == null) return;
        if(GameMatchReadyManagerServer.Instance == null) return;
        if(AIPlayerGenerator.Instance == null) return;

        // 플레이어 데이터 제거
        CurrentPlayerDataManager.Instance.RemovePlayer(clientId);

        // 현 매치에 접속중인 모든 플레이어 레디상태 초기화
        GameMatchReadyManagerServer.Instance.SetEveryPlayerUnReady();

        // 남은 플레이어들이 전부 AI인지 확인 후 그렇다면 전부 강퇴 처리
        if (CheckIsEveryPlayerAnAI())
        {
            CurrentPlayerDataManager.Instance.RemoveAllPlayers();
            AIPlayerGenerator.Instance.ResetAIPlayerGenerator();
        }

        // 플레이어가 이탈한 현재 씬이 게임씬이라면 추가 처리 진행
        GameSceneConnectionHandler(clientId);
    }

    private bool CheckIsEveryPlayerAnAI()
    {
        bool areAllPlayersAI = true;
        foreach (PlayerInGameData player in CurrentPlayerDataManager.Instance.GetCurrentPlayers())
        {
            if (!player.isAI)
            {
                areAllPlayersAI = false;
            }
        }
        return areAllPlayersAI;
    }

    private void GameSceneConnectionHandler(ulong clientId)
    {
        if (MultiplayerGameManager.Instance == null) return;

        // 나간 clientId GameOver 처리
        if (CurrentPlayerDataManager.Instance.GetPlayerDataListIndexByClientId(clientId) != -1)
            MultiplayerGameManager.Instance.UpdatePlayerGameOverOnServer(clientId);

        // 혹시 모든 플레이어가 나갔으면, 서버도 다시 로비씬으로 돌아간다
        if (CurrentPlayerDataManager.Instance.GetCurrentPlayerCount() == 0)
        {
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
            MultiplayerGameManager.Instance.CleanUpChildObjects();
            Destroy(MultiplayerGameManager.Instance.gameObject);
        }
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
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
        if(CurrentPlayerDataManager.Instance != null)
        {
            Destroy(CurrentPlayerDataManager.Instance.gameObject);
        }
        if(AIPlayerGenerator.Instance != null)
            Destroy(AIPlayerGenerator.Instance.gameObject);
#endif
    }

    // GameRoomPlayerCharacter에서 해당 인덱스의 플레이어가 접속 되었나 확인할 때 사용
    public bool IsPlayerIndexConnected(int toggleArrayPlayerIndex)
    {
        return toggleArrayPlayerIndex < CurrentPlayerDataManager.Instance.GetCurrentPlayerCount();
    }
}
