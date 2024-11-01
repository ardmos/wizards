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
public class ServerStartUp : MonoBehaviour
{
    public static ServerStartUp Instance = null;

    private const string InternalServerIp = "0.0.0.0";
    private string externalServerIP = "0.0.0.0";
    // Default port 7777
    private ushort serverPort = 7777;

    private string externalConnectionString => $"{externalServerIP}:{serverPort}";

    private IMultiplayService multiplayService;
    private const int multiplayServiceTimeout = 20000; //20초가량

    private string allocationId;
    private MultiplayEventCallbacks serverCallbacks;
    private IServerEvents serverEvents;


    
  
    private MatchmakingResults matchmakerPayload;



    async void Start()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
        
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
            // Command Line Args를 통해 서버인지 확인한다
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

        if (server)
        {
            StartServer();
            await StartServerServices();
        }
    }

    public void OnDestroy()
    {
        Dispose();
    }

    // 서버 시작.
    private void StartServer()
    {
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(InternalServerIp, serverPort);
        ServerNetworkConnectionManager.Instance.StartConnectionManager();
        NetworkManager.Singleton.StartServer();
    }

    async Task StartServerServices()
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
            matchmakerPayload = await GetMatchmakerPayload(multiplayServiceTimeout); /////여기까지.
            // 페이로드를 정상적으로 받아왔는지 확인
            if (matchmakerPayload != null)
            {
                // 서버를 플레이어 참가 가능 상태로 만듦
                await MultiplayService.Instance.ReadyServerForPlayersAsync();
                // 백필 프로세스 시작
                await StartBackfill(matchmakerPayload);
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

    /// <summary>
    /// 매치메이커 페이로드를 가져오는 작업을 수행하는 비동기 메서드입니다.
    /// 매치메이커 페이로드 할당을 기다리되, 무한정 기다리진 않도록 타임아웃을 설정했습니다.
    /// </summary>
    /// <param name="timeout">타임아웃</param>
    /// <returns>타임아웃시 null 반환</returns>
    private async Task<MatchmakingResults> GetMatchmakerPayload(int timeout)
    {
        var matchmakerPayloadTask = SubscribeAndAwaitMatchmakerAllocation();

        if(await Task.WhenAny(matchmakerPayloadTask, Task.Delay(timeout))==matchmakerPayloadTask) 
        {
            return matchmakerPayloadTask.Result;
        }

        return null;
    }

    /// <summary>
    /// 멀티플레이 서비스에 구독하고, 게임 서버 인스턴스 할당 및 할당 결과 페이로드를 기다리는 메서드입니다.
    /// 추후 매치메이킹 Backfill을 통해 유저들이 참여할 수 있도록 MatchmakingResult형태의 페이로드를 반환합니다.
    /// </summary>
    /// <returns>매치메이커 페이로드를 반환합니다</returns>
    private async Task<MatchmakingResults> SubscribeAndAwaitMatchmakerAllocation()
    {
        if(multiplayService == null) return null;

        allocationId = null;
        serverCallbacks = new MultiplayEventCallbacks();
        // 서버 이벤트 구독
        serverEvents = await multiplayService.SubscribeToServerEventsAsync(serverCallbacks);
        // 멀티플레이 게임 서버 인스턴스가 할당되었을 경우 allocationId를 저장합니다.
        serverCallbacks.Allocate += OnMultiplayAllocation;
        // 마찬가지로 allocationId를 저장하는 메서드를 await합니다. 만약을 위한 이중 안전장치입니다.
        allocationId = await AwaitAllocationID();
        // MatchmakingResults형태로 가공된 매치메이킹 서버 인스턴스 할당 결과 페이로드 받아오기
        var matchmakingResultPayload = await GetMatchmakerAllocationPayloadAsync();
        return matchmakingResultPayload;
    }

    /// <summary>
    /// 멀티플레이 게임 서버가 할당되었을 경우 동작하는 이벤트 핸들러입니다.
    /// allocationId를 저장합니다.
    /// </summary>
    /// <param name="allocation"></param>
    private void OnMultiplayAllocation(MultiplayAllocation allocation)
    {
        if(string.IsNullOrEmpty(allocationId)) return; 

        allocationId = allocation.AllocationId;
    }

    /// <summary>
    /// allocationId가 설정되기를 기다리다가 설정되면 저장하는 메서드입니다.
    /// 모종의 이유로 OnMultiplayAllocation가 작동하지 않을 경우를 대비해 동작시키는 메서드입니다.
    /// </summary>
    /// <returns></returns>
    private async Task<string> AwaitAllocationID()
    {
        var config = multiplayService.ServerConfig;
        Debug.Log($"Awaiting Allocation. Server Config is:\n" +
            $"-ServerID: {config.ServerId}\n"+
            $"-AllocationID: {config.AllocationId}\n"+
            $"-QPort: {config.QueryPort}\n"+
            $"-logs: {config.ServerLogDirectory}");

        while(string.IsNullOrEmpty(allocationId)) 
        {
            var configId = config.AllocationId;
            if(!string.IsNullOrEmpty(configId) && string.IsNullOrEmpty(allocationId)) 
            {
                allocationId = configId;
                break;
            }

            await Task.Delay(100);
        }

        return allocationId;
    }

    /// <summary>
    /// 멀티플레이 서버 인스턴스 할당 결과를 가져오는 메서드입니다.
    /// </summary>
    /// <returns>MatchmakingResults로 가공된 멀티플레이 서버 할당 페이로드</returns>
    private async Task<MatchmakingResults> GetMatchmakerAllocationPayloadAsync()
    {
        try
        {
            var payloadAllocation = await MultiplayService.Instance.GetPayloadAllocationFromJsonAs<MatchmakingResults>();
            var modelAsJson = JsonConvert.SerializeObject(payloadAllocation, Formatting.Indented);
            Debug.Log($"{nameof(GetMatchmakerAllocationPayloadAsync)}:\n{modelAsJson}");
            return payloadAllocation;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Something went wrong trying to get the Matchmaker Payload in GetMatchmakerAllocationPayloadAsync:\n{ex}");
        }

        return null;
    }

    

    // 메모리 누수를 막기 위한 메서드. 실행시점을 고민 후 적용하자
    private void Dispose()
    {
        serverCallbacks.Allocate -= OnMultiplayAllocation;
        serverEvents?.UnsubscribeAsync();
    }
}
#endif
