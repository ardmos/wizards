using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Matchmaker;
using Unity.Services.Matchmaker.Models;
using static ComponentValidator;

/// <summary>
/// ���� ������ ���� ���μ����� �����ϴ� Ŭ�����Դϴ�.
/// �÷��̾� ��Ī�� ���� ������ ����մϴ�.
/// </summary>
public class BackfillManager : NetworkBehaviour
{
    #region Singleton
    // BackfillManager�� �̱��� �ν��Ͻ��Դϴ�.
    public static BackfillManager Instance { get; private set; }
    #endregion

    #region Constants & Fields
    // ���� �޽��� �����
    private const string ERROR_MATCHMAKER_PAYLOAD_NOT_SET = "BackfillManager matchmakerPayload ��ü�� �������� �ʾҽ��ϴ�.";
    private const string ERROR_SERVER_INFO_PROVIDER_NOT_SET = "BackfillManager serverInfoProvider�� �������� �ʾҽ��ϴ�.";
    private const string ERROR_NETWORK_MANAGER_NOT_SET = "BackfillManager NetworkManager.Singleton ��ü�� �������� �ʾҽ��ϴ�.";
    private const string ERROR_MATCHMAKER_SERVICE_NOT_SET = "BackfillManager MatchmakerService.Instance ��ü�� �������� �ʾҽ��ϴ�.";
    private const string ERROR_SERVER_STARTUP_NOT_SET = "BackfillManager ServerStartup.Instance ��ü�� �������� �ʾҽ��ϴ�.";
    // Ƽ�� üũ ����(ms)
    private const int TICKET_CHECK_MS = 1000;

    private BackfillTicket localBackfillTicket;
    private CreateBackfillTicketOptions createBackfillTicketOptions;
    private IServerInfoProvider serverInfoProvider;
    private bool backfilling = false;
    #endregion

    #region Unity Lifecycle
    /// <summary>
    /// �̱��� �ν��Ͻ��� �ʱ�ȭ�մϴ�.
    /// </summary>
    private void Awake()
    {
        if (Instance == null)
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
    #endregion

    #region Backfill Methods
    /// <summary>
    /// ���� ���μ����� �����մϴ�.
    /// </summary>
    /// <param name="matchmakerPayload">��ġ����Ŀ�κ��� ���� ���̷ε�</param>
    public async Task StartBackfill(MatchmakingResults matchmakerPayload)
    {
        if (!ValidateComponent(matchmakerPayload, ERROR_MATCHMAKER_PAYLOAD_NOT_SET)) return;

        try
        {
            var backfillProperties = new BackfillTicketProperties(matchmakerPayload.MatchProperties);
            localBackfillTicket = new BackfillTicket { Id = matchmakerPayload.MatchProperties.BackfillTicketId, Properties = backfillProperties };
            await BeginBackfilling(matchmakerPayload);
        }
        catch (Exception ex)
        {
            Logger.LogError($"���� ���� �� ���� �߻�: {ex.Message}");
        }
    }

    /// <summary>
    /// ���� ���μ����� ������մϴ�.
    /// </summary>
    public async Task RestartBackfill()
    {
        if (!ValidateComponent(serverInfoProvider, ERROR_SERVER_INFO_PROVIDER_NOT_SET)) return;
        if (!ValidateComponent(NetworkManager.Singleton, ERROR_NETWORK_MANAGER_NOT_SET)) return;
        if (backfilling || NetworkManager.Singleton.ConnectedClients.Count <= 0 || !NeedsPlayers()) return;

        await BeginBackfilling(serverInfoProvider.GetMatchmakerPayload());
    }

    /// <summary>
    /// ���� ���μ����� �����ϴ� ���� �޼����Դϴ�.
    /// </summary>
    /// <param name="matchmakerPayload">��ġ����Ŀ�κ��� ���� ���̷ε�</param>
    private async Task BeginBackfilling(MatchmakingResults matchmakerPayload)
    {
        if (!ValidateComponent(matchmakerPayload, ERROR_MATCHMAKER_PAYLOAD_NOT_SET)) return;
        if (!ValidateComponent(serverInfoProvider, ERROR_SERVER_INFO_PROVIDER_NOT_SET)) return;
        if (!ValidateComponent(MatchmakerService.Instance, ERROR_MATCHMAKER_SERVICE_NOT_SET)) return;

        try
        {
            if (string.IsNullOrEmpty(localBackfillTicket.Id))
            {
                var matchProperties = matchmakerPayload.MatchProperties;

                createBackfillTicketOptions = new CreateBackfillTicketOptions
                {
                    Connection = serverInfoProvider.GetExternalConnectionString(),
                    QueueName = matchmakerPayload.QueueName,
                    Properties = new BackfillTicketProperties(matchProperties)
                };

                localBackfillTicket.Id = await MatchmakerService.Instance.CreateBackfillTicketAsync(createBackfillTicketOptions);
            }

            backfilling = true;
            await BackfillLoop();
        }
        catch (Exception ex)
        {
            Logger.LogError($"���� ���μ��� ���� �� ���� �߻�: {ex.Message}");
            backfilling = false;
        }
    }

    /// <summary>
    /// ���� ������ �����ϴ� �޼����Դϴ�.
    /// �ֱ������� ���� Ƽ���� Ȯ���ϰ� ������Ʈ�մϴ�.
    /// </summary>
    private async Task BackfillLoop()
    {
        if (!ValidateComponent(MatchmakerService.Instance, ERROR_MATCHMAKER_SERVICE_NOT_SET)) return;

        try
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

                await Task.Delay(TICKET_CHECK_MS);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError($"���� ���� ���� �� ���� �߻�: {ex.Message}");
        }
        finally
        {
            backfilling = false;
        }
    }
    #endregion

    #region Private Method
    /// <summary>
    /// ���ӿ� �߰� �÷��̾ �ʿ����� Ȯ���մϴ�.
    /// </summary>
    /// <returns>�߰� �÷��̾ �ʿ��ϸ� true, �׷��� ������ false</returns>
    private bool NeedsPlayers()
    {
        if (!ValidateComponent(NetworkManager.Singleton, ERROR_NETWORK_MANAGER_NOT_SET)) return false;

        return NetworkManager.Singleton.ConnectedClients.Count < ConnectionApprovalHandler.MaxPlayers;
    }
    #endregion
}