using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Matchmaker;
using Unity.Services.Matchmaker.Models;
using static ComponentValidator;

/// <summary>
/// 게임 세션의 백필 프로세스를 관리하는 클래스입니다.
/// 플레이어 매칭과 세션 유지를 담당합니다.
/// </summary>
public class BackfillManager : NetworkBehaviour
{
    #region Singleton
    // BackfillManager의 싱글톤 인스턴스입니다.
    public static BackfillManager Instance { get; private set; }
    #endregion

    #region Constants & Fields
    // 에러 메시지 상수들
    private const string ERROR_MATCHMAKER_PAYLOAD_NOT_SET = "BackfillManager matchmakerPayload 객체가 설정되지 않았습니다.";
    private const string ERROR_SERVER_INFO_PROVIDER_NOT_SET = "BackfillManager serverInfoProvider가 설정되지 않았습니다.";
    private const string ERROR_NETWORK_MANAGER_NOT_SET = "BackfillManager NetworkManager.Singleton 객체가 설정되지 않았습니다.";
    private const string ERROR_MATCHMAKER_SERVICE_NOT_SET = "BackfillManager MatchmakerService.Instance 객체가 설정되지 않았습니다.";
    private const string ERROR_SERVER_STARTUP_NOT_SET = "BackfillManager ServerStartup.Instance 객체가 설정되지 않았습니다.";
    // 티켓 체크 간격(ms)
    private const int TICKET_CHECK_MS = 1000;

    private BackfillTicket localBackfillTicket;
    private CreateBackfillTicketOptions createBackfillTicketOptions;
    private IServerInfoProvider serverInfoProvider;
    private bool backfilling = false;
    #endregion

    #region Unity Lifecycle
    /// <summary>
    /// 싱글톤 인스턴스를 초기화합니다.
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
    /// 네트워크 객체가 스폰될 때 호출되는 메서드입니다.
    /// serverInfoProvider를 초기화합니다.
    /// </summary>
    public override void OnNetworkSpawn()
    {
        if (!ValidateComponent(ServerStartup.Instance, ERROR_SERVER_STARTUP_NOT_SET)) return;

        serverInfoProvider = ServerStartup.Instance;
    }
    #endregion

    #region Backfill Methods
    /// <summary>
    /// 백필 프로세스를 시작합니다.
    /// </summary>
    /// <param name="matchmakerPayload">매치메이커로부터 받은 페이로드</param>
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
            Logger.LogError($"백필 시작 중 오류 발생: {ex.Message}");
        }
    }

    /// <summary>
    /// 백필 프로세스를 재시작합니다.
    /// </summary>
    public async Task RestartBackfill()
    {
        if (!ValidateComponent(serverInfoProvider, ERROR_SERVER_INFO_PROVIDER_NOT_SET)) return;
        if (!ValidateComponent(NetworkManager.Singleton, ERROR_NETWORK_MANAGER_NOT_SET)) return;
        if (backfilling || NetworkManager.Singleton.ConnectedClients.Count <= 0 || !NeedsPlayers()) return;

        await BeginBackfilling(serverInfoProvider.GetMatchmakerPayload());
    }

    /// <summary>
    /// 백필 프로세스를 시작하는 내부 메서드입니다.
    /// </summary>
    /// <param name="matchmakerPayload">매치메이커로부터 받은 페이로드</param>
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
            Logger.LogError($"백필 프로세스 시작 중 오류 발생: {ex.Message}");
            backfilling = false;
        }
    }

    /// <summary>
    /// 백필 루프를 실행하는 메서드입니다.
    /// 주기적으로 백필 티켓을 확인하고 업데이트합니다.
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
            Logger.LogError($"백필 루프 실행 중 오류 발생: {ex.Message}");
        }
        finally
        {
            backfilling = false;
        }
    }
    #endregion

    #region Private Method
    /// <summary>
    /// 게임에 추가 플레이어가 필요한지 확인합니다.
    /// </summary>
    /// <returns>추가 플레이어가 필요하면 true, 그렇지 않으면 false</returns>
    private bool NeedsPlayers()
    {
        if (!ValidateComponent(NetworkManager.Singleton, ERROR_NETWORK_MANAGER_NOT_SET)) return false;

        return NetworkManager.Singleton.ConnectedClients.Count < ConnectionApprovalHandler.MaxPlayers;
    }
    #endregion
}