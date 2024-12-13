#if UNITY_SERVER || UNITY_EDITOR
using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Core;
using Unity.Services.Matchmaker.Models;
using Unity.Services.Multiplay;
using static ComponentValidator;

/// <summary>
/// UGS Multiplay 서버의 초기화를 담당하는 클래스입니다.
/// </summary>
public class ServerStartup : NetworkBehaviour, IServerInfoProvider, ICleanable
{
    #region Singleton
    // ServerStartup의 싱글톤 인스턴스입니다.
    public static ServerStartup Instance { get; private set; }
    #endregion

    #region Constants & Fields
    private const string ERROR_SERVER_NETWORK_CONNECTION_MANAGER_NOT_SET = "ServerStartup ServerNetworkConnectionManager.Instance가 설정되지 않았습니다.";
    private const string ERROR_NETWORK_MANAGER_NOT_SET = "ServerStartup NetworkManager.Singleton가 설정되지 않았습니다.";
    private const string ERROR_MATCHMAKING_MANAGER_NOT_SET = "ServerStartup MatchmakingManager.Instance가 설정되지 않았습니다.";
    private const string ERROR_BACKFILL_MANAGER_NOT_SET = "ServerStartup BackfillManager.Instance가 설정되지 않았습니다.";
    private const string ERROR_MULTIPLAY_SERVICE_NOT_SET = "ServerStartup MultiplayService.Instance가 설정되지 않았습니다.";
    private const string INTERNAL_SERVER_IP = "0.0.0.0";

    private string externalServerIP = "0.0.0.0";
    private ushort serverPort = 7777;
    private string externalConnectionString => $"{externalServerIP}:{serverPort}";

    private IMultiplayService multiplayService;
    private MatchmakingResults matchmakerPayload;
    #endregion

    #region Unity Lifecycle
    /// <summary>
    /// 싱글톤 인스턴스를 초기화하고 파괴되지 않도록 설정합니다.
    /// 동시에 씬 정리 매니저에 정리 대상으로써 등록합니다.
    /// </summary>
    private void Awake()
    {
        if (Instance == null && IsServer)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneCleanupManager.RegisterCleanableObject(this);
        }
        else Destroy(gameObject);
    }

    /// <summary>
    /// 서버 설정을 초기화하고 서비스를 시작합니다.
    /// </summary>
    private async void Start()
    {
        if (!InitializeServerSettingsFromArgs()) return; // 실패시 이후 로직 실행하지 않음
        if (!StartServer()) return; // 실패시 이후 로직 실행하지 않음

        await StartServerServices();
    }

    /// <summary>
    /// 객체가 파괴될 때 씬 정리 매니저에 해뒀던 등록을 해제합니다.
    /// </summary>
    public override void OnDestroy()
    {
        SceneCleanupManager.UnregisterCleanableObject(this);
    }
    #endregion

    #region Unity Multiplay Server Startup
    /// <summary>
    /// 커맨드라인 인수를 확인하여 서버 설정을 초기화하고 그 결과를 반환하는 메서드입니다.
    /// </summary>
    /// <returns>서버 설정 초기화 성공 여부</returns>
    private bool InitializeServerSettingsFromArgs()
    {
        var args = Environment.GetCommandLineArgs();
        Logger.Log($"CommandLineArgs: {string.Join(" ", args)}");

        bool hasDedicatedServer = false;
        bool hasPort = false;
        bool hasIp = false;

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "-dedicatedServer":
                    hasDedicatedServer = true;
                    break;
                case "-port" when i + 1 < args.Length:
                    if (ushort.TryParse(args[i + 1], out ushort port))
                    {
                        serverPort = port;
                        hasPort = true;
                    }
                    i++; // 포트 넘버 부분의 배열 요소를 건너뜁니다
                    break;
                case "-ip" when i + 1 < args.Length:
                    externalServerIP = args[i + 1];
                    hasIp = true;
                    i++; // 아이피 주소 부분의 배열 요소를 건너뜁니다
                    break;
            }
        }

        bool isInitialized = hasDedicatedServer && hasPort && hasIp;
        if (!isInitialized)
        {
            Logger.LogError("커맨드라인 인수를 통한 서버 설정 초기화에 실패했습니다.");
        }

        return isInitialized;
    }

    /// <summary>
    /// 서버를 시작하는 메서드입니다.
    /// </summary>
    /// <returns>서버 시작의 성공 여부</returns>
    private bool StartServer()
    {
        if (!ValidateComponent(ServerNetworkConnectionManager.Instance, ERROR_SERVER_NETWORK_CONNECTION_MANAGER_NOT_SET)) return false;
        if (!ValidateComponent(NetworkManager.Singleton, ERROR_NETWORK_MANAGER_NOT_SET)) return false;

        try
        {
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(INTERNAL_SERVER_IP, serverPort);
            ServerNetworkConnectionManager.Instance.InitalizeConnectionManager();
            NetworkManager.Singleton.StartServer();
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"서버를 시작하는데 실패했습니다 : \n{ex}");
            return false;
        }
    }
    #endregion

    #region Unity Services Startup
    /// <summary>
    /// 서버 서비스를 시작하는 메서드입니다.
    /// </summary>
    private async Task StartServerServices()
    {
        if (!ValidateComponent(MatchmakingManager.Instance, ERROR_MATCHMAKING_MANAGER_NOT_SET)) return;
        if (!ValidateComponent(BackfillManager.Instance, ERROR_BACKFILL_MANAGER_NOT_SET)) return;
        if (!ValidateComponent(MultiplayService.Instance, ERROR_MULTIPLAY_SERVICE_NOT_SET)) return;

        if (!await InitializeUnityServices()) return; // 실패시 이후 로직 실행하지 않음
        if (!await SetupMultiplayService()) return; // 실패시 이후 로직 실행하지 않음

        await ConfigureMatchmakingAndBackfill();
    }

    /// <summary>
    /// Unity 서비스를 초기화합니다.
    /// </summary>
    /// <returns>Unity Service 초기화의 성공 여부</returns>
    private async Task<bool> InitializeUnityServices()
    {
        try
        {
            await UnityServices.InitializeAsync();
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"Unity Services 초기화에 실패했습니다: \n{ex}");
            return false;
        }
    }

    /// <summary>
    /// Multiplay 서비스를 설정합니다.
    /// </summary>
    /// <returns>Multiplay 서비스 설정 성공 여부</returns>
    private async Task<bool> SetupMultiplayService()
    {
        try
        {
            // Unity MultiplayService 인스턴스 설정
            multiplayService = MultiplayService.Instance;
            // Unity MultiplayService 의 SQP 쿼리 핸들러 설정
            await multiplayService.StartServerQueryHandlerAsync((ushort)ConnectionApprovalHandler.MaxPlayers, "n/a", "n/a", "0", "n/a");
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"MultiplayService 인스턴스 획득과 SQP 서비스를 설정하는데 실패했습니다: \n{ex}");
            return false;
        }
    }
    #endregion

    #region Matchmaking and Backfill Configuration
    /// <summary>
    /// 매치메이킹과 백필을 구성합니다.
    /// </summary>
    private async Task ConfigureMatchmakingAndBackfill()
    {
        try
        {
            // 매치메이킹 시스템에 멀티플레이 서버의 할당 결과(matchmakerPayload) 획득
            matchmakerPayload = await MatchmakingManager.Instance.GetMatchmakerPayload();

            if (matchmakerPayload != null)
            {
                // 서버를 플레이어 참가 가능 상태로 만듦
                await multiplayService.ReadyServerForPlayersAsync();
                // 백필 프로세스 시작
                await BackfillManager.Instance.StartBackfill(matchmakerPayload);
            }
            else
            {
                Logger.LogWarning("매치메이킹 시스템에 멀티플레이 서버의 할당 결과 획득을 실패했습니다 : 이유 -> 타임아웃");
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"매치메이킹 시스템에 멀티플레이 서버 할당과 백필 서비스를 시작하는데 실패했습니다 : \n{ex}");
        }
    }
    #endregion

    #region IServerInfoProvider 구현
    public IMultiplayService GetMultiplayService() => multiplayService;
    public MatchmakingResults GetMatchmakerPayload() => matchmakerPayload;
    public string GetExternalConnectionString() => externalConnectionString;
    #endregion

    #region ICleanable 구현
    public void Cleanup() => Destroy(gameObject);
    #endregion
}
#endif