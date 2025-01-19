using Unity.Netcode;
using static ComponentValidator;

/// <summary>
/// 서버 측 네트워크 연결을 관리하는 클래스입니다.
/// 플레이어의 연결, 연결 해제 및 관련 이벤트를 처리합니다.
/// </summary>
public class ServerNetworkConnectionManager : NetworkBehaviour, ICleanable
{
    #region Singleton
    // ServerNetworkConnectionManager의 싱글톤 인스턴스입니다.
    public static ServerNetworkConnectionManager Instance { get; private set; }
    #endregion

    #region Constants
    // 에러 메시지 상수들
    private const string ERROR_NETWORK_MANAGER_NOT_SET = "ServerNetworkConnectionManager NetworkManager.Singleton 객체가 설정되지 않았습니다.";
    private const string ERROR_GAME_MATCH_READY_MANAGER_NOT_SET = "ServerNetworkConnectionManager GameMatchReadyManagerServer.Instance 객체가 설정되지 않았습니다.";
    private const string ERROR_CURRENT_PLAYER_DATA_MANAGER_NOT_SET = "ServerNetworkConnectionManager CurrentPlayerDataManager.Instance 객체가 설정되지 않았습니다.";
    private const string ERROR_AI_PLAYER_GENERATOR_NOT_SET = "ServerNetworkConnectionManager AIPlayerGenerator.Instance 객체가 설정되지 않았습니다.";
    private const string ERROR_BACKFILL_MANAGER_NOT_SET = "ServerNetworkConnectionManager BackfillManager.Instance 객체가 설정되지 않았습니다.";
    private const string ERROR_MULTIPLAYER_GAME_MANAGER_NOT_SET = "ServerNetworkConnectionManager MultiplayerGameManager.Instance 객체가 설정되지 않았습니다.";
    #endregion

    #region Unity Lifecycle
    /// <summary>
    /// 싱글톤 인스턴스를 초기화하고 씬 전환 시 파괴되지 않도록 설정합니다.
    /// 동시에 씬 정리 매니저에 정리 대상으로써 등록합니다.
    /// </summary>
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneCleanupManager.RegisterCleanableObject(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 네트워크 객체가 디스폰될 때 호출되는 메서드입니다.
    /// 등록된 클라이언트 연결 및 연결 해제 이벤트 콜백을 제거합니다.
    /// </summary>
    public override void OnNetworkDespawn()
    {
        if (!ValidateComponent(NetworkManager.Singleton, ERROR_NETWORK_MANAGER_NOT_SET)) return;

        NetworkManager.Singleton.OnClientConnectedCallback -= Server_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback -= Server_OnClientDisconnectCallback;
    }

    /// <summary>
    /// 오브젝트 파괴시 씬 정리 매니저에 해뒀던 등록을 해제합니다.
    /// </summary>
    public override void OnDestroy()
    {
        SceneCleanupManager.UnregisterCleanableObject(this);
    }
    #endregion

    #region Initialization
    /// <summary>
    /// 서버가 시작될 때 호출되어 커넥션 매니저를 초기화시켜주는 메서드입니다.
    /// 클라이언트 연결 및 연결 해제 이벤트에 대한 콜백을 등록합니다.
    /// </summary>
    public void InitalizeConnectionManager()
    {
        if (!ValidateComponent(NetworkManager.Singleton, ERROR_NETWORK_MANAGER_NOT_SET)) return;

        NetworkManager.Singleton.OnClientConnectedCallback += Server_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += Server_OnClientDisconnectCallback;
    }
    #endregion

    #region Player Management
    /// <summary>
    /// 특정 인덱스의 플레이어가 연결되어 있는지 확인합니다.
    /// </summary>
    /// <param name="toggleArrayPlayerIndex">확인할 플레이어의 인덱스</param>
    /// <returns>플레이어가 연결되어 있으면 true, 그렇지 않으면 false</returns>
    public bool IsPlayerIndexConnected(int toggleArrayPlayerIndex)
    {
        if (!ValidateComponent(CurrentPlayerDataManager.Instance, ERROR_CURRENT_PLAYER_DATA_MANAGER_NOT_SET)) return false;

        return toggleArrayPlayerIndex < CurrentPlayerDataManager.Instance.GetCurrentPlayerCount();
    }

    /// <summary>
    /// 게임 씬에서의 플레이어 연결 해제를 처리합니다.
    /// 연결 해제된 플레이어를 게임 오버 처리하고, 남은 플레이어가 없는 경우 로비로 돌아갑니다.
    /// </summary>
    /// <param name="clientId">연결 해제된 클라이언트의 ID</param>
    private void HandlePlayerDisconnectInGameScene(ulong clientId)
    {
        if (!ValidateComponent(MultiplayerGameManager.Instance, ERROR_MULTIPLAYER_GAME_MANAGER_NOT_SET)) return;
        if (!ValidateComponent(CurrentPlayerDataManager.Instance, ERROR_CURRENT_PLAYER_DATA_MANAGER_NOT_SET)) return;
        if (!ValidateClientID(clientId)) return;

        SetDisconnectedPlayerGameOver(clientId);
        ReturnToLobbyWhenNoPlayersRemain();
    }

    /// <summary>
    /// 남아있는 플레이어가 없을 경우 로비로 돌아갑니다.
    /// </summary>
    private void ReturnToLobbyWhenNoPlayersRemain()
    {
        if (CurrentPlayerDataManager.Instance.GetCurrentPlayerCount() <= 0)
        {
            Cleanup();
            LoadSceneManager.Load(LoadSceneManager.Scene.LobbyScene);
        }
    }

    /// <summary>
    /// 클라이언트 데이터를 제거합니다.
    /// </summary>
    /// <param name="clientId">제거할 클라이언트의 ID</param>
    private void RemoveClientData(ulong clientId) => CurrentPlayerDataManager.Instance.RemovePlayer(clientId);

    /// <summary>
    /// 모든 플레이어의 준비 상태를 초기화합니다.
    /// </summary>
    private void ResetAllPlayersReadyState() => GameMatchReadyManagerServer.Instance.SetEveryPlayerUnReady();

    /// <summary>
    /// 연결이 끊긴 플레이어를 게임 오버 상태로 설정합니다.
    /// </summary>
    /// <param name="clientId">게임 오버 처리할 클라이언트의 ID</param>
    private void SetDisconnectedPlayerGameOver(ulong clientId) => MultiplayerGameManager.Instance.UpdatePlayerGameOverOnServer(clientId);
    #endregion

    #region AI Player Management
    /// <summary>
    /// 모든 플레이어가 AI일 경우의 시나리오를 처리합니다.
    /// AI만 남았을 경우 모든 플레이어를 제거하고 AI 생성기를 초기화합니다.
    /// </summary>
    private void HandleAllAIScenario()
    {
        if (CheckIsEveryPlayerAnAI())
        {
            CurrentPlayerDataManager.Instance.RemoveAllPlayers();
            AIPlayerGenerator.Instance.ResetAIPlayerGenerator();
        }
    }

    /// <summary>
    /// 현재 게임에 참여 중인 모든 플레이어가 AI인지 확인합니다.
    /// </summary>
    /// <returns>모든 플레이어가 AI면 true, 그렇지 않으면 false</returns>
    private bool CheckIsEveryPlayerAnAI()
    {
        if (!ValidateComponent(CurrentPlayerDataManager.Instance, ERROR_CURRENT_PLAYER_DATA_MANAGER_NOT_SET)) return false;

        var players = CurrentPlayerDataManager.Instance.GetCurrentPlayers();
        foreach (var player in players)
        {
            if (!player.isAI)
            {
                return false;
            }
        }
        return true;
    }
    #endregion

    #region Backfill Management
    /// <summary>
    /// 백필 프로세스를 재개합니다.
    /// </summary>
    private void RestartBackfill() => _ = BackfillManager.Instance.RestartBackfill();
    #endregion

    #region Validation Check
    /// <summary>
    /// 주어진 클라이언트 ID가 유효한지 확인합니다.
    /// </summary>
    /// <param name="clientId">확인할 클라이언트 ID</param>
    /// <returns>클라이언트 ID가 유효하면 true, 그렇지 않으면 false</returns>
    private bool ValidateClientID(ulong clientId)
    {
        bool isValidClientId = CurrentPlayerDataManager.Instance.GetPlayerDataListIndexByClientId(clientId) != -1;

        if (!isValidClientId) Logger.LogError($"유효하지 않은 클라이언트ID: {clientId}");

        return isValidClientId;
    }
    #endregion

    #region Event Handlers
    /// <summary>
    /// 클라이언트 연결 시 호출되는 콜백 메서드입니다.
    /// 새로운 유저 접속 시 모든 AI 플레이어를 준비 상태로 설정합니다.
    /// </summary>
    private void Server_OnClientConnectedCallback(ulong clientId)
    {
        if (!ValidateComponent(GameMatchReadyManagerServer.Instance, ERROR_GAME_MATCH_READY_MANAGER_NOT_SET)) return;

        GameMatchReadyManagerServer.Instance.SetEveryAIPlayersReady();
    }

    /// <summary>
    /// 클라이언트 연결 해제 시 호출되는 콜백 메서드입니다.
    /// 플레이어 데이터 제거, 준비 상태 초기화, AI 시나리오 처리, 백필 재시작, 게임씬에서의 플레이어 연결 해제 등을 수행합니다.
    /// </summary>
    /// <param name="clientId">연결 해제된 클라이언트의 ID</param>
    private void Server_OnClientDisconnectCallback(ulong clientId)
    {
        if (!ValidateComponent(CurrentPlayerDataManager.Instance, ERROR_CURRENT_PLAYER_DATA_MANAGER_NOT_SET)) return;
        if (!ValidateComponent(GameMatchReadyManagerServer.Instance, ERROR_GAME_MATCH_READY_MANAGER_NOT_SET)) return;
        if (!ValidateComponent(AIPlayerGenerator.Instance, ERROR_AI_PLAYER_GENERATOR_NOT_SET)) return;
        if (!ValidateComponent(BackfillManager.Instance, ERROR_BACKFILL_MANAGER_NOT_SET)) return;

        RemoveClientData(clientId);
        ResetAllPlayersReadyState();
        HandleAllAIScenario();
        RestartBackfill();
        HandlePlayerDisconnectInGameScene(clientId);


    }
    #endregion

    #region ICleanable 구현
    /// <summary>
    /// ICleanable 인터페이스 구현. 객체를 정리합니다.
    /// 이 메서드는 씬 전환 시 호출되어 현재 객체를 파괴합니다.
    /// </summary>
    public void Cleanup()
    {
        Destroy(gameObject);
    }
    #endregion
}