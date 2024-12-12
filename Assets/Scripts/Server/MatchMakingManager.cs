using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Matchmaker.Models;
using Unity.Services.Multiplay;
using static ComponentValidator;

/// <summary>
/// 매치메이킹 프로세스를 관리하는 클래스입니다.
/// 서버 측에서 매치메이킹 할당 및 페이로드 처리를 담당합니다.
/// </summary>
public class MatchmakingManager : NetworkBehaviour
{
    #region Singleton
    // MatchmakingManager의 싱글톤 인스턴스입니다.
    public static MatchmakingManager Instance { get; private set; }
    #endregion

    #region Constants & Fields
    // 에러 메세지 상수들
    private const string ERROR_SERVER_STARTUP_NOT_SET = "MatchmakingManager ServerStartup.Instance 객체가 설정되지 않았습니다.";
    private const string ERROR_SERVER_INFO_PROVIDER_NOT_SET = "MatchmakingManager serverInfoProvider가 설정되지 않았습니다.";
    private const string ERROR_MULTIPLAYER_SERVICE_NOT_SET = "MatchmakingManager serverInfoProvider에 MultiplayerService.Instance 객체가 설정되지 않았습니다.";
    private const string ERROR_MULTIPLAY_SERVER_EVENT_CALLBACKS_NOT_SET = "MatchmakingManager serverCallbacks가 설정되지 않았습니다.";
    private const string ERROR_SERVER_EVENTS_NOT_SET = "MatchmakingManager serverEvents가 설정되지 않았습니다.";
    // 멀티플레이 서비스를 통한 메치메이커 페이로드 할당 타임아웃(ms)
    private const int MULTIPLAY_SERVICE_TIMEOUT_MS = 20000;

    private string allocationId;
    private MultiplayEventCallbacks serverCallbacks;
    private IServerEvents serverEvents;
    private IServerInfoProvider serverInfoProvider;
    #endregion

    #region Unity Lifecycle
    /// <summary>
    /// 싱글톤 인스턴스를 초기화합니다.
    /// </summary>
    private void Awake()
    {
        if (Instance == null && IsServer)
        {
            Instance = this;
        }
        else Destroy(gameObject);
    }

    /// <summary>
    /// 네트워크 객체가 스폰될 때 호출되는 메서드입니다.
    /// serverInfoProvider를 초기화합니다.
    /// </summary>
    public override void OnNetworkSpawn()
    {
        if (!ValidateComponent(ServerStartup.Instance, ERROR_SERVER_STARTUP_NOT_SET)) return;

        serverInfoProvider = ServerStartup.Instance;
    }

    /// <summary>
    /// 네트워크 객체가 디스폰될 때 호출되는 메서드입니다.
    /// 리소스를 해제하고 이벤트 구독을 취소합니다.
    /// </summary>
    public override void OnDestroy()
    {
        Dispose();
    }
    #endregion

    #region Matchmaking Methods
    /// <summary>
    /// 매치메이커 페이로드를 가져오는 작업을 수행하는 비동기 메서드입니다.
    /// 매치메이커 페이로드 할당을 기다리되, 무한정 기다리진 않도록 타임아웃을 설정했습니다.
    /// </summary>
    /// <returns>매치메이킹 결과, 타임아웃시 null 반환</returns>
    public async Task<MatchmakingResults> GetMatchmakerPayload()
    {
        var matchmakerPayloadTask = SubscribeAndAwaitMatchmakerAllocation();

        if (await Task.WhenAny(matchmakerPayloadTask, Task.Delay(MULTIPLAY_SERVICE_TIMEOUT_MS)) == matchmakerPayloadTask)
        {
            return matchmakerPayloadTask.Result;
        }

        Logger.LogWarning("매치메이커 페이로드 가져오기 타임아웃");
        return null;
    }

    /// <summary>
    /// 멀티플레이 서비스에 구독하고, 멀티플레이 게임 서버 인스턴스 할당 및 할당 결과 페이로드를 기다리는 메서드입니다.
    /// 추후 매치메이킹 Backfill을 통해 유저들이 참여할 수 있도록 MatchmakingResult형태의 페이로드를 반환합니다.
    /// </summary>
    /// <returns>매치메이커 페이로드를 반환합니다</returns>
    private async Task<MatchmakingResults> SubscribeAndAwaitMatchmakerAllocation()
    {
        if (!ValidateComponent(serverInfoProvider, ERROR_SERVER_INFO_PROVIDER_NOT_SET)) return null;
        if (!ValidateComponent(serverInfoProvider.GetMultiplayService(), ERROR_MULTIPLAYER_SERVICE_NOT_SET)) return null;

        allocationId = null;
        serverCallbacks = new MultiplayEventCallbacks();
        // 서버 이벤트 구독
        serverEvents = await serverInfoProvider.GetMultiplayService().SubscribeToServerEventsAsync(serverCallbacks);
        // 멀티플레이 게임 서버 인스턴스가 할당되었을 경우 allocationId를 저장합니다.
        serverCallbacks.Allocate += OnMultiplayAllocation;
        // 마찬가지로 allocationId를 저장하는 메서드를 await합니다. 만약을 위한 이중 안전장치입니다.
        allocationId = await AwaitAllocationID();
        // MatchmakingResults형태로 가공된 멀티플레이 서버 인스턴스 할당 결과 페이로드 받아오기
        var matchmakingResultPayload = await GetMatchmakerAllocationPayloadAsync();
        return matchmakingResultPayload;
    }

    /// <summary>
    /// allocationId가 설정되기를 기다리다가 설정되면 저장하는 메서드입니다.
    /// OnMultiplayAllocation이 작동하지 않을 경우를 대비한 백업 메서드입니다.
    /// </summary>
    /// <returns>할당된 서버 ID</returns>
    private async Task<string> AwaitAllocationID()
    {
        var config = serverInfoProvider.GetMultiplayService().ServerConfig;
        Logger.Log($"서버 할당을 기다리고 있습니다. Server Config is:\n" +
        $"-ServerID: {config.ServerId}\n" +
        $"-AllocationID: {config.AllocationId}\n" +
        $"-QPort: {config.QueryPort}\n" +
        $"-logs: {config.ServerLogDirectory}");

        while (string.IsNullOrEmpty(allocationId))
        {
            var configId = config.AllocationId;
            if (!string.IsNullOrEmpty(configId) && string.IsNullOrEmpty(allocationId))
            {
                allocationId = configId;
                break;
            }

            await Task.Delay(100);
        }

        return allocationId;
    }

    /// <summary>
    /// 매치메이킹 시스템에 멀티플레이 서버의 할당 결과를 가져오는 메서드입니다.
    /// </summary>
    /// <returns>MatchmakingResults로 가공된 멀티플레이 서버 할당 페이로드</returns>
    private async Task<MatchmakingResults> GetMatchmakerAllocationPayloadAsync()
    {
        try
        {
            var payloadAllocation = await serverInfoProvider.GetMultiplayService().GetPayloadAllocationFromJsonAs<MatchmakingResults>();
            var modelAsJson = JsonConvert.SerializeObject(payloadAllocation, Formatting.Indented);
            Logger.Log($"GetMatchmakerAllocationPayloadAsync: \n{modelAsJson}");
            return payloadAllocation;
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"Matchmaker allocation payload를 얻는데 실패했습니다. {ex}");
        }
        return null;
    }

    /// <summary>
    /// 리소스를 해제하고 이벤트 구독을 취소합니다.
    /// </summary>
    private void Dispose()
    {
        if (!ValidateComponent(serverCallbacks, ERROR_MULTIPLAY_SERVER_EVENT_CALLBACKS_NOT_SET)) return;
        if (!ValidateComponent(serverEvents, ERROR_SERVER_EVENTS_NOT_SET)) return;

        serverCallbacks.Allocate -= OnMultiplayAllocation;
        serverEvents.UnsubscribeAsync();
    }
    #endregion

    #region Event Handlers
    /// <summary>
    /// 멀티플레이 게임 서버가 할당되었을 경우 동작하는 이벤트 핸들러입니다.
    /// allocationId를 저장합니다.
    /// </summary>
    /// <param name="allocation">할당된 멀티플레이 정보</param>
    private void OnMultiplayAllocation(MultiplayAllocation allocation)
    {
        if (!string.IsNullOrEmpty(allocationId)) return;

        allocationId = allocation.AllocationId;
        Logger.Log($"멀티플레이 게임 서버 할당 완료: {allocationId}");
    }
    #endregion
}