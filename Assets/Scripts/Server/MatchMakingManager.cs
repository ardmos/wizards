using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Matchmaker.Models;
using Unity.Services.Multiplay;
using UnityEngine;

public class MatchmakingManager : NetworkBehaviour
{
    public static MatchmakingManager Instance { get; private set; }

    private string allocationId;
    private MultiplayEventCallbacks serverCallbacks;
    private IServerEvents serverEvents;
    private const int multiplayServiceTimeout = 20000; //20�ʰ���  

    private void Awake()
    {
        if (Instance == null && IsServer)
            Instance = this;
    }

    public override void OnDestroy()
    {
        Dispose();
    }

    /// <summary>
    /// ��ġ����Ŀ ���̷ε带 �������� �۾��� �����ϴ� �񵿱� �޼����Դϴ�.
    /// ��ġ����Ŀ ���̷ε� �Ҵ��� ��ٸ���, ������ ��ٸ��� �ʵ��� Ÿ�Ӿƿ��� �����߽��ϴ�.
    /// </summary>
    /// <param name="timeout">Ÿ�Ӿƿ�</param>
    /// <returns>Ÿ�Ӿƿ��� null ��ȯ</returns>
    public async Task<MatchmakingResults> GetMatchmakerPayload()
    {
        var matchmakerPayloadTask = SubscribeAndAwaitMatchmakerAllocation();

        if (await Task.WhenAny(matchmakerPayloadTask, Task.Delay(multiplayServiceTimeout)) == matchmakerPayloadTask)
        {
            return matchmakerPayloadTask.Result;
        }

        return null;
    }

    /// <summary>
    /// ��Ƽ�÷��� ���񽺿� �����ϰ�, ���� ���� �ν��Ͻ� �Ҵ� �� �Ҵ� ��� ���̷ε带 ��ٸ��� �޼����Դϴ�.
    /// ���� ��ġ����ŷ Backfill�� ���� �������� ������ �� �ֵ��� MatchmakingResult������ ���̷ε带 ��ȯ�մϴ�.
    /// </summary>
    /// <returns>��ġ����Ŀ ���̷ε带 ��ȯ�մϴ�</returns>
    private async Task<MatchmakingResults> SubscribeAndAwaitMatchmakerAllocation()
    {
        if (ServerStartup.Instance.GetMultiplayService() == null) return null;

        allocationId = null;
        serverCallbacks = new MultiplayEventCallbacks();
        // ���� �̺�Ʈ ����
        serverEvents = await ServerStartup.Instance.GetMultiplayService().SubscribeToServerEventsAsync(serverCallbacks);
        // ��Ƽ�÷��� ���� ���� �ν��Ͻ��� �Ҵ�Ǿ��� ��� allocationId�� �����մϴ�.
        serverCallbacks.Allocate += OnMultiplayAllocation;
        // ���������� allocationId�� �����ϴ� �޼��带 await�մϴ�. ������ ���� ���� ������ġ�Դϴ�.
        allocationId = await AwaitAllocationID();
        // MatchmakingResults���·� ������ ��ġ����ŷ ���� �ν��Ͻ� �Ҵ� ��� ���̷ε� �޾ƿ���
        var matchmakingResultPayload = await GetMatchmakerAllocationPayloadAsync();
        return matchmakingResultPayload;
    }

    /// <summary>
    /// ��Ƽ�÷��� ���� ������ �Ҵ�Ǿ��� ��� �����ϴ� �̺�Ʈ �ڵ鷯�Դϴ�.
    /// allocationId�� �����մϴ�.
    /// </summary>
    /// <param name="allocation"></param>
    private void OnMultiplayAllocation(MultiplayAllocation allocation)
    {
        if (string.IsNullOrEmpty(allocationId)) return;

        allocationId = allocation.AllocationId;
    }

    /// <summary>
    /// allocationId�� �����Ǳ⸦ ��ٸ��ٰ� �����Ǹ� �����ϴ� �޼����Դϴ�.
    /// ������ ������ OnMultiplayAllocation�� �۵����� ���� ��츦 ����� ���۽�Ű�� �޼����Դϴ�.
    /// </summary>
    /// <returns></returns>
    private async Task<string> AwaitAllocationID()
    {
        var config = ServerStartup.Instance.GetMultiplayService().ServerConfig;
        Debug.Log($"Awaiting Allocation. Server Config is:\n" +
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
    /// ��Ƽ�÷��� ���� �ν��Ͻ� �Ҵ� ����� �������� �޼����Դϴ�.
    /// </summary>
    /// <returns>MatchmakingResults�� ������ ��Ƽ�÷��� ���� �Ҵ� ���̷ε�</returns>
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

    private void Dispose()
    {
        serverCallbacks.Allocate -= OnMultiplayAllocation;
        serverEvents?.UnsubscribeAsync();
    }
}
