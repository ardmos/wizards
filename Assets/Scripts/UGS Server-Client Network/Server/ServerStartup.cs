#if UNITY_SERVER || UNITY_EDITOR
using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Core;
using Unity.Services.Matchmaker.Models;
using Unity.Services.Multiplay;
using static ComponentValidator;

/// <summary>
/// UGS Multiplay ������ �ʱ�ȭ�� ����ϴ� Ŭ�����Դϴ�.
/// </summary>
public class ServerStartup : NetworkBehaviour, IServerInfoProvider, ICleanable
{
    #region Singleton
    // ServerStartup�� �̱��� �ν��Ͻ��Դϴ�.
    public static ServerStartup Instance { get; private set; }
    #endregion

    #region Constants & Fields
    private const string ERROR_SERVER_NETWORK_CONNECTION_MANAGER_NOT_SET = "ServerStartup ServerNetworkConnectionManager.Instance�� �������� �ʾҽ��ϴ�.";
    private const string ERROR_NETWORK_MANAGER_NOT_SET = "ServerStartup NetworkManager.Singleton�� �������� �ʾҽ��ϴ�.";
    private const string ERROR_MATCHMAKING_MANAGER_NOT_SET = "ServerStartup MatchmakingManager.Instance�� �������� �ʾҽ��ϴ�.";
    private const string ERROR_BACKFILL_MANAGER_NOT_SET = "ServerStartup BackfillManager.Instance�� �������� �ʾҽ��ϴ�.";
    private const string ERROR_MULTIPLAY_SERVICE_NOT_SET = "ServerStartup MultiplayService.Instance�� �������� �ʾҽ��ϴ�.";
    private const string INTERNAL_SERVER_IP = "0.0.0.0";

    private string externalServerIP = "0.0.0.0";
    private ushort serverPort = 7777;
    private string externalConnectionString => $"{externalServerIP}:{serverPort}";

    private IMultiplayService multiplayService;
    private MatchmakingResults matchmakerPayload;
    #endregion

    #region Unity Lifecycle
    /// <summary>
    /// �̱��� �ν��Ͻ��� �ʱ�ȭ�ϰ� �ı����� �ʵ��� �����մϴ�.
    /// ���ÿ� �� ���� �Ŵ����� ���� ������ν� ����մϴ�.
    /// </summary>
    private void Awake()
    {
        if (Instance == null && IsServer)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneCleanupManager.RegisterCleanableObject(this);
        }
        else Destroy(gameObject);
    }

    /// <summary>
    /// ���� ������ �ʱ�ȭ�ϰ� ���񽺸� �����մϴ�.
    /// </summary>
    private async void Start()
    {
        if (!InitializeServerSettingsFromArgs()) return; // ���н� ���� ���� �������� ����
        if (!StartServer()) return; // ���н� ���� ���� �������� ����

        await StartServerServices();
    }

    /// <summary>
    /// ��ü�� �ı��� �� �� ���� �Ŵ����� �ص״� ����� �����մϴ�.
    /// </summary>
    public override void OnDestroy()
    {
        SceneCleanupManager.UnregisterCleanableObject(this);
    }
    #endregion

    #region Unity Multiplay Server Startup
    /// <summary>
    /// Ŀ�ǵ���� �μ��� Ȯ���Ͽ� ���� ������ �ʱ�ȭ�ϰ� �� ����� ��ȯ�ϴ� �޼����Դϴ�.
    /// </summary>
    /// <returns>���� ���� �ʱ�ȭ ���� ����</returns>
    private bool InitializeServerSettingsFromArgs()
    {
        var args = Environment.GetCommandLineArgs();
        Logger.Log($"CommandLineArgs: {string.Join(" ", args)}");

        bool hasDedicatedServer = false;
        bool hasPort = false;
        bool hasIp = false;

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "-dedicatedServer":
                    hasDedicatedServer = true;
                    break;
                case "-port" when i + 1 < args.Length:
                    if (ushort.TryParse(args[i + 1], out ushort port))
                    {
                        serverPort = port;
                        hasPort = true;
                    }
                    i++; // ��Ʈ �ѹ� �κ��� �迭 ��Ҹ� �ǳʶݴϴ�
                    break;
                case "-ip" when i + 1 < args.Length:
                    externalServerIP = args[i + 1];
                    hasIp = true;
                    i++; // ������ �ּ� �κ��� �迭 ��Ҹ� �ǳʶݴϴ�
                    break;
            }
        }

        bool isInitialized = hasDedicatedServer && hasPort && hasIp;
        if (!isInitialized)
        {
            Logger.LogError("Ŀ�ǵ���� �μ��� ���� ���� ���� �ʱ�ȭ�� �����߽��ϴ�.");
        }

        return isInitialized;
    }

    /// <summary>
    /// ������ �����ϴ� �޼����Դϴ�.
    /// </summary>
    /// <returns>���� ������ ���� ����</returns>
    private bool StartServer()
    {
        if (!ValidateComponent(ServerNetworkConnectionManager.Instance, ERROR_SERVER_NETWORK_CONNECTION_MANAGER_NOT_SET)) return false;
        if (!ValidateComponent(NetworkManager.Singleton, ERROR_NETWORK_MANAGER_NOT_SET)) return false;

        try
        {
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(INTERNAL_SERVER_IP, serverPort);
            ServerNetworkConnectionManager.Instance.InitalizeConnectionManager();
            NetworkManager.Singleton.StartServer();
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"������ �����ϴµ� �����߽��ϴ� : \n{ex}");
            return false;
        }
    }
    #endregion

    #region Unity Services Startup
    /// <summary>
    /// ���� ���񽺸� �����ϴ� �޼����Դϴ�.
    /// </summary>
    private async Task StartServerServices()
    {
        if (!ValidateComponent(MatchmakingManager.Instance, ERROR_MATCHMAKING_MANAGER_NOT_SET)) return;
        if (!ValidateComponent(BackfillManager.Instance, ERROR_BACKFILL_MANAGER_NOT_SET)) return;
        if (!ValidateComponent(MultiplayService.Instance, ERROR_MULTIPLAY_SERVICE_NOT_SET)) return;

        if (!await InitializeUnityServices()) return; // ���н� ���� ���� �������� ����
        if (!await SetupMultiplayService()) return; // ���н� ���� ���� �������� ����

        await ConfigureMatchmakingAndBackfill();
    }

    /// <summary>
    /// Unity ���񽺸� �ʱ�ȭ�մϴ�.
    /// </summary>
    /// <returns>Unity Service �ʱ�ȭ�� ���� ����</returns>
    private async Task<bool> InitializeUnityServices()
    {
        try
        {
            await UnityServices.InitializeAsync();
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"Unity Services �ʱ�ȭ�� �����߽��ϴ�: \n{ex}");
            return false;
        }
    }

    /// <summary>
    /// Multiplay ���񽺸� �����մϴ�.
    /// </summary>
    /// <returns>Multiplay ���� ���� ���� ����</returns>
    private async Task<bool> SetupMultiplayService()
    {
        try
        {
            // Unity MultiplayService �ν��Ͻ� ����
            multiplayService = MultiplayService.Instance;
            // Unity MultiplayService �� SQP ���� �ڵ鷯 ����
            await multiplayService.StartServerQueryHandlerAsync((ushort)ConnectionApprovalHandler.MaxPlayers, "n/a", "n/a", "0", "n/a");
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"MultiplayService �ν��Ͻ� ȹ��� SQP ���񽺸� �����ϴµ� �����߽��ϴ�: \n{ex}");
            return false;
        }
    }
    #endregion

    #region Matchmaking and Backfill Configuration
    /// <summary>
    /// ��ġ����ŷ�� ������ �����մϴ�.
    /// </summary>
    private async Task ConfigureMatchmakingAndBackfill()
    {
        try
        {
            // ��ġ����ŷ �ý��ۿ� ��Ƽ�÷��� ������ �Ҵ� ���(matchmakerPayload) ȹ��
            matchmakerPayload = await MatchmakingManager.Instance.GetMatchmakerPayload();

            if (matchmakerPayload != null)
            {
                // ������ �÷��̾� ���� ���� ���·� ����
                await multiplayService.ReadyServerForPlayersAsync();
                // ���� ���μ��� ����
                await BackfillManager.Instance.StartBackfill(matchmakerPayload);
            }
            else
            {
                Logger.LogWarning("��ġ����ŷ �ý��ۿ� ��Ƽ�÷��� ������ �Ҵ� ��� ȹ���� �����߽��ϴ� : ���� -> Ÿ�Ӿƿ�");
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"��ġ����ŷ �ý��ۿ� ��Ƽ�÷��� ���� �Ҵ�� ���� ���񽺸� �����ϴµ� �����߽��ϴ� : \n{ex}");
        }
    }
    #endregion

    #region IServerInfoProvider ����
    public IMultiplayService GetMultiplayService() => multiplayService;
    public MatchmakingResults GetMatchmakerPayload() => matchmakerPayload;
    public string GetExternalConnectionString() => externalConnectionString;
    #endregion

    #region ICleanable ����
    public void Cleanup() => Destroy(gameObject);
    #endregion
}
#endif