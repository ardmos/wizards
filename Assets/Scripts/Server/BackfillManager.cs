using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Matchmaker;
using Unity.Services.Matchmaker.Models;

/// <summary>
/// 게임 세션의 백필 프로세스를 관리하는 클래스입니다.
/// </summary>
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
        serverInfoProvider = ServerStartup.Instance;
    }

    /// <summary>
    /// 백필 프로세스를 시작합니다.
    /// </summary>
    public async Task StartBackfill(MatchmakingResults payload)
    {
        var backfillProperties = new BackfillTicketProperties(payload.MatchProperties);
        localBackfillTicket = new BackfillTicket { Id = payload.MatchProperties.BackfillTicketId, Properties = backfillProperties };
        await BeginBackfilling(payload);
    }

    /// <summary>
    /// 백필 프로세스를 재시작합니다.
    /// </summary>
    public void RestartBackfill()
    {
        if (serverInfoProvider == null) return;
        if (backfilling || NetworkManager.Singleton.ConnectedClients.Count <= 0 || !NeedsPlayers()) return;

#pragma warning disable 4014
        BeginBackfilling(serverInfoProvider.GetMatchmakerPayload());
#pragma warning restore 4014
    }

    /// <summary>
    /// 백필 프로세스를 시작하는 내부 메서드입니다.
    /// </summary>
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

    /// <summary>
    /// 백필 루프를 실행하는 메서드입니다.
    /// </summary>
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

    /// <summary>
    /// 게임에 추가 플레이어가 필요한지 확인합니다.
    /// </summary>
    private bool NeedsPlayers()
    {
        return NetworkManager.Singleton.ConnectedClients.Count < ConnectionApprovalHandler.MaxPlayers;
    }
}
