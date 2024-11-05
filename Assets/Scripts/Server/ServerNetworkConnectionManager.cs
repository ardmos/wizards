using Unity.Netcode;
using UnityEditor.PackageManager;

/// <summary>
/// 서버 측 네트워크 연결 관리를 담당하는 연결 관리자 클래스입니다.
/// 플레이어 연결, 연결 해제등을 처리합니다.
/// </summary>
public class ServerNetworkConnectionManager : NetworkBehaviour, ICleanable
{
    public static ServerNetworkConnectionManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null && IsServer)
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
    /// 연결 관리자를 초기화하고 시작합니다.
    /// </summary>
    public void StartConnectionManager()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += Server_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += Server_OnClientDisconnectCallback;
    }

    public override void OnNetworkDespawn()
    {
        NetworkManager.Singleton.OnClientConnectedCallback -= Server_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback -= Server_OnClientDisconnectCallback;
    }

    public override void OnDestroy()
    {
        SceneCleanupManager.UnregisterCleanableObject(this);
    }

    /// <summary>
    /// 클라이언트 연결 시 호출되는 콜백 메서드입니다.
    /// 새로운 유저 접속 시 모든 AI 플레이어를 레디 상태로 설정합니다.
    /// </summary>
    private void Server_OnClientConnectedCallback(ulong obj)
    {
        if (GameMatchReadyManagerServer.Instance == null) return;
        GameMatchReadyManagerServer.Instance.SetEveryAIPlayersReady();
    }

    /// <summary>
    /// 클라이언트 연결 해제 시 호출되는 콜백 메서드입니다.
    /// </summary>
    private void Server_OnClientDisconnectCallback(ulong clientId)
    {
        if (CurrentPlayerDataManager.Instance == null) return;
        if (GameMatchReadyManagerServer.Instance == null) return;
        if (AIPlayerGenerator.Instance == null) return;
        if (BackfillManager.Instance == null) return;

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

        // Backfill 재시작
        BackfillManager.Instance.RestartBackfill();
        // 플레이어가 이탈한 현재 씬이 게임씬이라면 필요한 추가 처리 진행
        GameSceneDisconnectHandler(clientId);
    }

    /// <summary>
    /// 모든 플레이어가 AI인지 확인해주는 메서드입니다
    /// </summary>
    /// <returns>모든 플레이어가 AI인지 여부를 담은 boolean 변수</returns>
    private bool CheckIsEveryPlayerAnAI()
    {
        if(CurrentPlayerDataManager.Instance == null) return false;

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

    /// <summary>
    /// 게임 씬에서의 연결 해제 처리를 담당합니다.
    /// </summary>
    private void GameSceneDisconnectHandler(ulong clientId)
    {
        if (MultiplayerGameManager.Instance == null) return;
        if (CurrentPlayerDataManager.Instance == null) return;
        // clientId 유효성 체크
        if (CurrentPlayerDataManager.Instance.GetPlayerDataListIndexByClientId(clientId) == -1) return;

        // 연결 해제된 clientId 플레이어를 GameOver 처리 합니다.
        MultiplayerGameManager.Instance.UpdatePlayerGameOverOnServer(clientId);

        // 혹시 남아있는 플레이어가 없는 경우, 서버도 다시 로비씬으로 돌아갑니다.
        if (CurrentPlayerDataManager.Instance.GetCurrentPlayerCount() <= 0)
        {
            Cleanup();
            LoadSceneManager.Load(LoadSceneManager.Scene.LobbyScene);
        }
    }

    /// <summary>
    /// 특정 인덱스의 플레이어가 연결되어 있는지 확인합니다.
    /// </summary>
    public bool IsPlayerIndexConnected(int toggleArrayPlayerIndex)
    {
        if(CurrentPlayerDataManager.Instance == null) return false;

        return toggleArrayPlayerIndex < CurrentPlayerDataManager.Instance.GetCurrentPlayerCount();
    }

    /// <summary>
    /// 특정 씬으로 이동시 현 오브젝트가 파괴되는 기능을 옵저버패턴으로 구현했습니다.
    /// </summary>
    public void Cleanup()
    {
        Destroy(gameObject);
    }
}
