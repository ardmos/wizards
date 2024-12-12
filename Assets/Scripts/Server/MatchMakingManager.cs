using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Matchmaker.Models;
using Unity.Services.Multiplay;
using static ComponentValidator;

/// <summary>
/// ��ġ����ŷ ���μ����� �����ϴ� Ŭ�����Դϴ�.
/// ���� ������ ��ġ����ŷ �Ҵ� �� ���̷ε� ó���� ����մϴ�.
/// </summary>
public class MatchmakingManager : NetworkBehaviour
{
    #region Singleton
    // MatchmakingManager�� �̱��� �ν��Ͻ��Դϴ�.
    public static MatchmakingManager Instance { get; private set; }
    #endregion

    #region Constants & Fields
    // ���� �޼��� �����
    private const string ERROR_SERVER_STARTUP_NOT_SET = "MatchmakingManager ServerStartup.Instance ��ü�� �������� �ʾҽ��ϴ�.";
    private const string ERROR_SERVER_INFO_PROVIDER_NOT_SET = "MatchmakingManager serverInfoProvider�� �������� �ʾҽ��ϴ�.";
    private const string ERROR_MULTIPLAYER_SERVICE_NOT_SET = "MatchmakingManager serverInfoProvider�� MultiplayerService.Instance ��ü�� �������� �ʾҽ��ϴ�.";
    private const string ERROR_MULTIPLAY_SERVER_EVENT_CALLBACKS_NOT_SET = "MatchmakingManager serverCallbacks�� �������� �ʾҽ��ϴ�.";
    private const string ERROR_SERVER_EVENTS_NOT_SET = "MatchmakingManager serverEvents�� �������� �ʾҽ��ϴ�.";
    // ��Ƽ�÷��� ���񽺸� ���� ��ġ����Ŀ ���̷ε� �Ҵ� Ÿ�Ӿƿ�(ms)
    private const int MULTIPLAY_SERVICE_TIMEOUT_MS = 20000;

    private string allocationId;
    private MultiplayEventCallbacks serverCallbacks;
    private IServerEvents serverEvents;
    private IServerInfoProvider serverInfoProvider;
    #endregion

    #region Unity Lifecycle
    /// <summary>
    /// �̱��� �ν��Ͻ��� �ʱ�ȭ�մϴ�.
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
    /// ��Ʈ��ũ ��ü�� ������ �� ȣ��Ǵ� �޼����Դϴ�.
    /// serverInfoProvider�� �ʱ�ȭ�մϴ�.
    /// </summary>
    public override void OnNetworkSpawn()
    {
        if (!ValidateComponent(ServerStartup.Instance, ERROR_SERVER_STARTUP_NOT_SET)) return;

        serverInfoProvider = ServerStartup.Instance;
    }

    /// <summary>
    /// ��Ʈ��ũ ��ü�� ������ �� ȣ��Ǵ� �޼����Դϴ�.
    /// ���ҽ��� �����ϰ� �̺�Ʈ ������ ����մϴ�.
    /// </summary>
    public override void OnDestroy()
    {
        Dispose();
    }
    #endregion

    #region Matchmaking Methods
    /// <summary>
    /// ��ġ����Ŀ ���̷ε带 �������� �۾��� �����ϴ� �񵿱� �޼����Դϴ�.
    /// ��ġ����Ŀ ���̷ε� �Ҵ��� ��ٸ���, ������ ��ٸ��� �ʵ��� Ÿ�Ӿƿ��� �����߽��ϴ�.
    /// </summary>
    /// <returns>��ġ����ŷ ���, Ÿ�Ӿƿ��� null ��ȯ</returns>
    public async Task<MatchmakingResults> GetMatchmakerPayload()
    {
        var matchmakerPayloadTask = SubscribeAndAwaitMatchmakerAllocation();

        if (await Task.WhenAny(matchmakerPayloadTask, Task.Delay(MULTIPLAY_SERVICE_TIMEOUT_MS)) == matchmakerPayloadTask)
        {
            return matchmakerPayloadTask.Result;
        }

        Logger.LogWarning("��ġ����Ŀ ���̷ε� �������� Ÿ�Ӿƿ�");
        return null;
    }

    /// <summary>
    /// ��Ƽ�÷��� ���񽺿� �����ϰ�, ��Ƽ�÷��� ���� ���� �ν��Ͻ� �Ҵ� �� �Ҵ� ��� ���̷ε带 ��ٸ��� �޼����Դϴ�.
    /// ���� ��ġ����ŷ Backfill�� ���� �������� ������ �� �ֵ��� MatchmakingResult������ ���̷ε带 ��ȯ�մϴ�.
    /// </summary>
    /// <returns>��ġ����Ŀ ���̷ε带 ��ȯ�մϴ�</returns>
    private async Task<MatchmakingResults> SubscribeAndAwaitMatchmakerAllocation()
    {
        if (!ValidateComponent(serverInfoProvider, ERROR_SERVER_INFO_PROVIDER_NOT_SET)) return null;
        if (!ValidateComponent(serverInfoProvider.GetMultiplayService(), ERROR_MULTIPLAYER_SERVICE_NOT_SET)) return null;

        allocationId = null;
        serverCallbacks = new MultiplayEventCallbacks();
        // ���� �̺�Ʈ ����
        serverEvents = await serverInfoProvider.GetMultiplayService().SubscribeToServerEventsAsync(serverCallbacks);
        // ��Ƽ�÷��� ���� ���� �ν��Ͻ��� �Ҵ�Ǿ��� ��� allocationId�� �����մϴ�.
        serverCallbacks.Allocate += OnMultiplayAllocation;
        // ���������� allocationId�� �����ϴ� �޼��带 await�մϴ�. ������ ���� ���� ������ġ�Դϴ�.
        allocationId = await AwaitAllocationID();
        // MatchmakingResults���·� ������ ��Ƽ�÷��� ���� �ν��Ͻ� �Ҵ� ��� ���̷ε� �޾ƿ���
        var matchmakingResultPayload = await GetMatchmakerAllocationPayloadAsync();
        return matchmakingResultPayload;
    }

    /// <summary>
    /// allocationId�� �����Ǳ⸦ ��ٸ��ٰ� �����Ǹ� �����ϴ� �޼����Դϴ�.
    /// OnMultiplayAllocation�� �۵����� ���� ��츦 ����� ��� �޼����Դϴ�.
    /// </summary>
    /// <returns>�Ҵ�� ���� ID</returns>
    private async Task<string> AwaitAllocationID()
    {
        var config = serverInfoProvider.GetMultiplayService().ServerConfig;
        Logger.Log($"���� �Ҵ��� ��ٸ��� �ֽ��ϴ�. Server Config is:\n" +
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
    /// ��ġ����ŷ �ý��ۿ� ��Ƽ�÷��� ������ �Ҵ� ����� �������� �޼����Դϴ�.
    /// </summary>
    /// <returns>MatchmakingResults�� ������ ��Ƽ�÷��� ���� �Ҵ� ���̷ε�</returns>
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
            Logger.LogWarning($"Matchmaker allocation payload�� ��µ� �����߽��ϴ�. {ex}");
        }
        return null;
    }

    /// <summary>
    /// ���ҽ��� �����ϰ� �̺�Ʈ ������ ����մϴ�.
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
    /// ��Ƽ�÷��� ���� ������ �Ҵ�Ǿ��� ��� �����ϴ� �̺�Ʈ �ڵ鷯�Դϴ�.
    /// allocationId�� �����մϴ�.
    /// </summary>
    /// <param name="allocation">�Ҵ�� ��Ƽ�÷��� ����</param>
    private void OnMultiplayAllocation(MultiplayAllocation allocation)
    {
        if (!string.IsNullOrEmpty(allocationId)) return;

        allocationId = allocation.AllocationId;
        Logger.Log($"��Ƽ�÷��� ���� ���� �Ҵ� �Ϸ�: {allocationId}");
    }
    #endregion
}