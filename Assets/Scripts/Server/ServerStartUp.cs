#if UNITY_SERVER || UNITY_EDITOR 
using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Core;
using Unity.Services.Matchmaker.Models;
using Unity.Services.Multiplay;
using UnityEngine;

/// <summary>
/// UGS Multiplay ������ ���� �� �ʱ�ȭ�� ����ϴ� Ŭ�����Դϴ�.
/// </summary>
public class ServerStartup : NetworkBehaviour, IServerInfoProvider
{
    public static ServerStartup Instance { get; private set; }

    private const string InternalServerIp = "0.0.0.0";
    private string externalServerIP = "0.0.0.0";
    private ushort serverPort = 7777; // �⺻ ��Ʈ

    private string externalConnectionString => $"{externalServerIP}:{serverPort}";

    private IMultiplayService multiplayService;
    private MatchmakingResults matchmakerPayload;

    private void Awake()
    {
        if (Instance == null && IsServer)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    private async void Start()
    {        
        if (CheckServerCommandLineArgs())
        {
            StartServer();
            await StartServerServices();
        }
    }

    /// <summary>
    /// Ŀ�ǵ���� �μ��� Ȯ���Ͽ� ���� ������ Ȯ���ϰ� ���� Ȯ�� ����� ��ȯ�ϴ� �޼����Դϴ�.
    /// </summary>
    /// <returns>���� Ȯ�� ���� ���θ� ���� boolean����</returns>
    private bool CheckServerCommandLineArgs()
    {
        bool server = false;
        var args = System.Environment.GetCommandLineArgs();

        string commandLineArgs = "";
        foreach (var arg in args)
        {
            commandLineArgs += arg;
        }
        Debug.Log($"CommandLineArgs: {commandLineArgs}");


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

        return server;
    }

    /// <summary>
    /// ������ �����ϴ� �޼����Դϴ�.
    /// </summary>
    private void StartServer()
    {
        if (ServerNetworkConnectionManager.Instance == null) return;

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(InternalServerIp, serverPort);
        ServerNetworkConnectionManager.Instance.StartConnectionManager();
        NetworkManager.Singleton.StartServer();
    }

    /// <summary>
    /// ���� ���񽺸� �����ϴ� �޼����Դϴ�.
    /// </summary>
    /// <returns></returns>
    private async Task StartServerServices()
    {
        if (MatchmakingManager.Instance == null) return;
        if (BackfillManager.Instance == null) return;

        // Unity Service �ʱ�ȭ
        await UnityServices.InitializeAsync();
        try
        {
            // Unity MultiplayService �ν��Ͻ� ����
            multiplayService = MultiplayService.Instance;
            // Unity MultiplayService �� SQP ���� �ڵ鷯 ����
            await multiplayService.StartServerQueryHandlerAsync((ushort)ConnectionApprovalHandler.MaxPlayers, "n/a", "n/a", "0", "n/a");
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"SQP ���񽺸� �����ϴµ� �����߽��ϴ� : \n{ex}");
        }

        try
        {
            // matchmakerPayload ȹ��
            matchmakerPayload = await MatchmakingManager.Instance.GetMatchmakerPayload(); 

            if (matchmakerPayload != null)
            {
                // ������ �÷��̾� ���� ���� ���·� ����
                await MultiplayService.Instance.ReadyServerForPlayersAsync();
                // ���� ���μ��� ����
                await BackfillManager.Instance.StartBackfill(matchmakerPayload);
            }
            else
            {
                Debug.LogWarning("Ÿ�Ӿƿ����� Matchmaker Payload�� ȹ���ϴµ� �����߽��ϴ�");
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"��Ƽ�÷��� ���� �Ҵ� & ���� ���񽺸� �����ϴµ� �����߽��ϴ� : \n{ex}");
        }
    }

    #region IServerInfoProvider
    public IMultiplayService GetMultiplayService()
    {
        return multiplayService;
    }

    public MatchmakingResults GetMatchmakerPayload()
    {
        return matchmakerPayload;
    }

    public string GetExternalConnectionString()
    {
        return externalConnectionString;
    }
    #endregion
}
#endif
