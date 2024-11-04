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
    private const int multiplayServiceTimeout = 20000; //20초가량  

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
    /// 매치메이커 페이로드를 가져오는 작업을 수행하는 비동기 메서드입니다.
    /// 매치메이커 페이로드 할당을 기다리되, 무한정 기다리진 않도록 타임아웃을 설정했습니다.
    /// </summary>
    /// <param name="timeout">타임아웃</param>
    /// <returns>타임아웃시 null 반환</returns>
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
    /// 멀티플레이 서비스에 구독하고, 게임 서버 인스턴스 할당 및 할당 결과 페이로드를 기다리는 메서드입니다.
    /// 추후 매치메이킹 Backfill을 통해 유저들이 참여할 수 있도록 MatchmakingResult형태의 페이로드를 반환합니다.
    /// </summary>
    /// <returns>매치메이커 페이로드를 반환합니다</returns>
    private async Task<MatchmakingResults> SubscribeAndAwaitMatchmakerAllocation()
    {
        if (ServerStartup.Instance.GetMultiplayService() == null) return null;

        allocationId = null;
        serverCallbacks = new MultiplayEventCallbacks();
        // 서버 이벤트 구독
        serverEvents = await ServerStartup.Instance.GetMultiplayService().SubscribeToServerEventsAsync(serverCallbacks);
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
        if (string.IsNullOrEmpty(allocationId)) return;

        allocationId = allocation.AllocationId;
    }

    /// <summary>
    /// allocationId가 설정되기를 기다리다가 설정되면 저장하는 메서드입니다.
    /// 모종의 이유로 OnMultiplayAllocation가 작동하지 않을 경우를 대비해 동작시키는 메서드입니다.
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

    private void Dispose()
    {
        serverCallbacks.Allocate -= OnMultiplayAllocation;
        serverEvents?.UnsubscribeAsync();
    }
}
