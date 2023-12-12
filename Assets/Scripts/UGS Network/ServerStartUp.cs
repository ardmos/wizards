using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Core;
using Unity.Services.Matchmaker;
using Unity.Services.Matchmaker.Models;
using Unity.Services.Multiplay;
using UnityEngine;

public class ServerStartUp : MonoBehaviour
{
    public static event System.Action ClientInstance;

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

    private BackfillTicket localBackfillTicket;
    private CreateBackfillTicketOptions createBackfillTicketOptions;
    private const int ticketCheckMs = 1000;
    private MatchmakingResults matchmakingPayload;

    private bool backfilling = false;

    // Start is called before the first frame update
    async void Start()
    {
        bool server = false;
        var args = System.Environment.GetCommandLineArgs();
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

        if (server)
        {
            StartServer();
            await StartServerServices();
        }
        else
        {
            ClientInstance?.Invoke();
        }
    }

    private void StartServer()
    {
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(InternalServerIp, serverPort);
        NetworkManager.Singleton.StartServer();
        NetworkManager.Singleton.OnClientDisconnectCallback += ClientDisconnected;
    }

    async Task StartServerServices()
    {
        await UnityServices.InitializeAsync();
        try
        {
            multiplayService = MultiplayService.Instance;
            // connect to sqp query handler
            await multiplayService.StartServerQueryHandlerAsync((ushort)ConnectionApprovalHandler.MaxPlayers, "n/a", "n/a", "0", "n/a");
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Something went wrong trying to set up the SQP Service:\n{ex}");
        }

        try
        {
            matchmakingPayload = await GetMatchmakerPayload(multiplayServiceTimeout);
            if (matchmakingPayload != null)
            {
                Debug.Log($"Got payload: {matchmakingPayload}");
                await StartBackfill(matchmakingPayload);
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

    private async Task<MatchmakingResults> GetMatchmakerPayload(int timeout)
    {
        var matchmakerPayloadTask = SubscribeAndAwaitMatchmakerAllocation();
        // 아래 둘 중 하나를 만족하면 결과를 반환한다
        if(await Task.WhenAny(matchmakerPayloadTask, Task.Delay(timeout))==matchmakerPayloadTask) 
        {
            return matchmakerPayloadTask.Result;
        }

        return null;
    }

    private async Task<MatchmakingResults> SubscribeAndAwaitMatchmakerAllocation()
    {
        if(multiplayService == null) { return null; }
        allocationId = null;
        serverCallbacks = new MultiplayEventCallbacks();
        // allocate 됐을 경우 리스너 등록
        serverCallbacks.Allocate += OnMultiplayAllocation;
        serverEvents = await multiplayService.SubscribeToServerEventsAsync(serverCallbacks);

        allocationId = await AwaitAllocationID();
        var mmPayload = await GetMatchmakerAllocationPayloadAsync();
        return mmPayload;
    }

    private void OnMultiplayAllocation(MultiplayAllocation allocation)
    {
        Debug.Log($"OnAllocation: {allocation.AllocationId}");
        if(string.IsNullOrEmpty(allocationId)) { return; }
        allocationId = allocation.AllocationId;
    }

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

    private async Task StartBackfill(MatchmakingResults payload)
    {
        var backfillProperties = new BackfillTicketProperties(payload.MatchProperties);
        localBackfillTicket = new BackfillTicket { Id = payload.MatchProperties.BackfillTicketId, Properties = backfillProperties };
        await BeginBackfilling(payload);
    }

    private async Task BeginBackfilling(MatchmakingResults payload)
    {
        if (string.IsNullOrEmpty(localBackfillTicket.Id))
        {
            var matchProperties = payload.MatchProperties;

            createBackfillTicketOptions = new CreateBackfillTicketOptions
            {
                Connection = externalConnectionString,
                QueueName = payload.QueueName,
                Properties = new BackfillTicketProperties(matchProperties)
            };

            localBackfillTicket.Id = await MatchmakerService.Instance.CreateBackfillTicketAsync(createBackfillTicketOptions);
        }

        backfilling = true;
        #pragma warning disable 4014
        BackfillLoop();
        #pragma warning restore 4014
    }

    private async Task BackfillLoop()
    {
        while (backfilling && NeedsPlayers())
        {
            localBackfillTicket = await MatchmakerService.Instance.ApproveBackfillTicketAsync(localBackfillTicket.Id);
            if (!NeedsPlayers())
            {
                await MatchmakerService.Instance.DeleteBackfillTicketAsync(localBackfillTicket.Id);
                localBackfillTicket.Id = null;
                backfilling = false;
                return;
            }

            await Task.Delay(ticketCheckMs);
        }

        // 만약을 위해 한 번더 false
        backfilling = false;
    }

    private bool NeedsPlayers()
    {
        return NetworkManager.Singleton.ConnectedClients.Count < ConnectionApprovalHandler.MaxPlayers;
    }

    private void ClientDisconnected(ulong clientId)
    {
        if (!backfilling && NetworkManager.Singleton.ConnectedClients.Count > 0 && NeedsPlayers())
        {
            BeginBackfilling(matchmakingPayload);
        }
    }

    // 메모리 누수를 막기 위한 메서드. 실행시점을 고민 후 적용하자
    private void Dispose()
    {
        serverCallbacks.Allocate -= OnMultiplayAllocation;
        serverEvents?.UnsubscribeAsync();
    }
}
