#if UNITY_SERVER || UNITY_EDITOR 
using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Core;
using Unity.Services.Matchmaker.Models;
using Unity.Services.Multiplay;
using UnityEngine;

/// <summary>
/// UGS Multiplay 서버의 설정 및 초기화를 담당하는 클래스입니다.
/// </summary>
public class ServerStartup : NetworkBehaviour, IServerInfoProvider
{
    public static ServerStartup Instance { get; private set; }

    private const string InternalServerIp = "0.0.0.0";
    private string externalServerIP = "0.0.0.0";
    private ushort serverPort = 7777; // 기본 포트

    private string externalConnectionString => $"{externalServerIP}:{serverPort}";

    private IMultiplayService multiplayService;
    private MatchmakingResults matchmakerPayload;

    private void Awake()
    {
        if (Instance == null && IsServer)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    private async void Start()
    {        
        if (CheckServerCommandLineArgs())
        {
            StartServer();
            await StartServerServices();
        }
    }

    /// <summary>
    /// 커맨드라인 인수를 확인하여 서버 설정을 확인하고 설정 확인 결과를 반환하는 메서드입니다.
    /// </summary>
    /// <returns>설정 확인 성공 여부를 담은 boolean변수</returns>
    private bool CheckServerCommandLineArgs()
    {
        bool server = false;
        var args = System.Environment.GetCommandLineArgs();

        string commandLineArgs = "";
        foreach (var arg in args)
        {
            commandLineArgs += arg;
        }
        Debug.Log($"CommandLineArgs: {commandLineArgs}");


        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-dedicatedServer")
            {
                server = true;
            }
            if (args[i] == "-port" && (i + 1 < args.Length))
            {
                serverPort = (ushort)int.Parse(args[i + 1]);
            }
            if (args[i] == "-ip" && (i + 1 < args.Length))
            {
                externalServerIP = args[i + 1];
            }
        }

        return server;
    }

    /// <summary>
    /// 서버를 시작하는 메서드입니다.
    /// </summary>
    private void StartServer()
    {
        if (ServerNetworkConnectionManager.Instance == null) return;

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(InternalServerIp, serverPort);
        ServerNetworkConnectionManager.Instance.StartConnectionManager();
        NetworkManager.Singleton.StartServer();
    }

    /// <summary>
    /// 서버 서비스를 시작하는 메서드입니다.
    /// </summary>
    /// <returns></returns>
    private async Task StartServerServices()
    {
        if (MatchmakingManager.Instance == null) return;
        if (BackfillManager.Instance == null) return;

        // Unity Service 초기화
        await UnityServices.InitializeAsync();
        try
        {
            // Unity MultiplayService 인스턴스 생성
            multiplayService = MultiplayService.Instance;
            // Unity MultiplayService 의 SQP 쿼리 핸들러 설정
            await multiplayService.StartServerQueryHandlerAsync((ushort)ConnectionApprovalHandler.MaxPlayers, "n/a", "n/a", "0", "n/a");
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"SQP 서비스를 설정하는데 실패했습니다 : \n{ex}");
        }

        try
        {
            // matchmakerPayload 획득
            matchmakerPayload = await MatchmakingManager.Instance.GetMatchmakerPayload(); 

            if (matchmakerPayload != null)
            {
                // 서버를 플레이어 참가 가능 상태로 만듦
                await MultiplayService.Instance.ReadyServerForPlayersAsync();
                // 백필 프로세스 시작
                await BackfillManager.Instance.StartBackfill(matchmakerPayload);
            }
            else
            {
                Debug.LogWarning("타임아웃으로 Matchmaker Payload를 획득하는데 실패했습니다");
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"멀티플레이 서버 할당 & 백필 서비스를 시작하는데 실패했습니다 : \n{ex}");
        }
    }

    #region IServerInfoProvider
    public IMultiplayService GetMultiplayService()
    {
        return multiplayService;
    }

    public MatchmakingResults GetMatchmakerPayload()
    {
        return matchmakerPayload;
    }

    public string GetExternalConnectionString()
    {
        return externalConnectionString;
    }
    #endregion
}
#endif
