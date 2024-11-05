using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Matchmaker;
using Unity.Services.Matchmaker.Models;

/// <summary>
/// ���� ������ ���� ���μ����� �����ϴ� Ŭ�����Դϴ�.
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
    /// ���� ���μ����� �����մϴ�.
    /// </summary>
    public async Task StartBackfill(MatchmakingResults payload)
    {
        var backfillProperties = new BackfillTicketProperties(payload.MatchProperties);
        localBackfillTicket = new BackfillTicket { Id = payload.MatchProperties.BackfillTicketId, Properties = backfillProperties };
        await BeginBackfilling(payload);
    }

    /// <summary>
    /// ���� ���μ����� ������մϴ�.
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
    /// ���� ���μ����� �����ϴ� ���� �޼����Դϴ�.
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
    /// ���� ������ �����ϴ� �޼����Դϴ�.
    /// </summary>
    private async Task BackfillLoop()
    {
        while (backfilling && NeedsPlayers())
        {
            localBackfillTicket = await MatchmakerService.Instance.ApproveBackfillTicketAsync(localBackfillTicket.Id);

            // Ǯ���� �Ǿ����� Ȯ��
            if (!NeedsPlayers())
            {
                // Ǯ���� �Ǿ��� �� �߰� ���� ����
                await MatchmakerService.Instance.DeleteBackfillTicketAsync(localBackfillTicket.Id);
                localBackfillTicket.Id = null;
                backfilling = false;
                return;
            }

            await Task.Delay(ticketCheckMs);
        }

        // ������ ���� �� ���� false
        backfilling = false;
    }

    /// <summary>
    /// ���ӿ� �߰� �÷��̾ �ʿ����� Ȯ���մϴ�.
    /// </summary>
    private bool NeedsPlayers()
    {
        return NetworkManager.Singleton.ConnectedClients.Count < ConnectionApprovalHandler.MaxPlayers;
    }
}
