#if UNITY_SERVER || UNITY_EDITOR 
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Core;
using Unity.Services.Matchmaker.Models;
using Unity.Services.Multiplay;
using UnityEngine;

/// <summary>
/// Matchmaking 서비스를 위한 서버 설정 스크립트 입니다. 
/// </summary>
public class ServerStartup : NetworkBehaviour
{
    public static ServerStartup Instance = null;

    private const string InternalServerIp = "0.0.0.0";
    private string externalServerIP = "0.0.0.0";
    // Default port 7777
    private ushort serverPort = 7777;

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
            // Command Line Args를 통해 올바른 UGS서버인지 확인한다
            if (args[i] == "-dedicatedServer")
            {
                server = true;
            }
            // port 확인. 서비스 포트 확인
            if (args[i] == "-port" && (i + 1 < args.Length))
            {
                serverPort = (ushort)int.Parse(args[i + 1]);
            }
            // ip 확인. Backfill을 구현하기위해 외부에서 접속 가능한 주소를 파악해둔다.
            if (args[i] == "-ip" && (i + 1 < args.Length))
            {
                externalServerIP = args[i + 1];
            }
        }

        return server;
    }

    // 서버 시작.
    private void StartServer()
    {
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(InternalServerIp, serverPort);
        ServerNetworkConnectionManager.Instance.StartConnectionManager();
        NetworkManager.Singleton.StartServer();
    }

    private async Task StartServerServices()
    {
        // Unity 서비스 초기화
        await UnityServices.InitializeAsync();
        try
        {
            // 멀티플레이 인스턴스 생성
            multiplayService = MultiplayService.Instance;
            // Unity MultiplayService의 SQP 쿼리 핸들러 설정
            await multiplayService.StartServerQueryHandlerAsync((ushort)ConnectionApprovalHandler.MaxPlayers, "n/a", "n/a", "0", "n/a");
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Something went wrong trying to set up the SQP Service:\n{ex}");
        }

        try
        {
            // matchmakerPayload(매치메이커가 발급하는 페이로드) 획득 시도. 타임아웃일 경우 null 반환
            matchmakerPayload = await MatchmakingManager.Instance.GetMatchmakerPayload(); 
            // 페이로드를 정상적으로 받아왔는지 확인
            if (matchmakerPayload != null)
            {
                // 서버를 플레이어 참가 가능 상태로 만듦
                await MultiplayService.Instance.ReadyServerForPlayersAsync();
                // 백필 프로세스 시작
                await BackfillManager.Instance.StartBackfill(matchmakerPayload);
            }
            else
            {
                Debug.LogWarning("Getting the Matchmaker Payload timed out, starting with defaults.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Something went wrong trying to set up the Allocation & Backfill Services:\n{ex}");
        }
    }

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
}
#endif
