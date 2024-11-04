using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Matchmaker.Models;
using Unity.Services.Matchmaker;
using UnityEngine;
using System.Threading.Tasks;

public class BackfillManager : NetworkBehaviour
{
    public static BackfillManager Instance { get; private set; }

    private BackfillTicket localBackfillTicket;
    private CreateBackfillTicketOptions createBackfillTicketOptions;
    private IServerInfoProvider serverInfoProvider;
    private const int ticketCheckMs = 1000;
    private bool backfilling = false;

    private void Awake()
    {
        if (Instance == null && IsServer)
        {
            Instance = this;
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        serverInfoProvider = ServerStartup.Instance;
    }

    public async Task StartBackfill(MatchmakingResults payload)
    {
        var backfillProperties = new BackfillTicketProperties(payload.MatchProperties);
        localBackfillTicket = new BackfillTicket { Id = payload.MatchProperties.BackfillTicketId, Properties = backfillProperties };
        await BeginBackfilling(payload);
    }

    public void RestartBackfill()
    {
        if (serverInfoProvider == null) return;
        if (backfilling || NetworkManager.Singleton.ConnectedClients.Count <= 0 || !NeedsPlayers()) return;

#pragma warning disable 4014
        BeginBackfilling(serverInfoProvider.GetMatchmakerPayload());
#pragma warning restore 4014
    }

    private async Task BeginBackfilling(MatchmakingResults payload)
    {
        if(serverInfoProvider == null) return;

        if (string.IsNullOrEmpty(localBackfillTicket.Id))
        {
            var matchProperties = payload.MatchProperties;

            createBackfillTicketOptions = new CreateBackfillTicketOptions
            {
                Connection = serverInfoProvider.GetExternalConnectionString(),
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

            // 풀방이 되었는지 확인
            if (!NeedsPlayers())
            {
                // 풀방이 되었을 시 추가 진입 차단
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


}
