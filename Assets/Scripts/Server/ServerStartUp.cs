#if UNITY_SERVER || UNITY_EDITOR 
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Core;
using Unity.Services.Matchmaker.Models;
using Unity.Services.Multiplay;
using UnityEngine;

/// <summary>
/// Matchmaking ���񽺸� ���� ���� ���� ��ũ��Ʈ �Դϴ�. 
/// </summary>
public class ServerStartup : NetworkBehaviour
{
    public static ServerStartup Instance = null;

    private const string InternalServerIp = "0.0.0.0";
    private string externalServerIP = "0.0.0.0";
    // Default port 7777
    private ushort serverPort = 7777;

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
            // Command Line Args�� ���� �ùٸ� UGS�������� Ȯ���Ѵ�
            if (args[i] == "-dedicatedServer")
            {
                server = true;
            }
            // port Ȯ��. ���� ��Ʈ Ȯ��
            if (args[i] == "-port" && (i + 1 < args.Length))
            {
                serverPort = (ushort)int.Parse(args[i + 1]);
            }
            // ip Ȯ��. Backfill�� �����ϱ����� �ܺο��� ���� ������ �ּҸ� �ľ��صд�.
            if (args[i] == "-ip" && (i + 1 < args.Length))
            {
                externalServerIP = args[i + 1];
            }
        }

        return server;
    }

    // ���� ����.
    private void StartServer()
    {
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(InternalServerIp, serverPort);
        ServerNetworkConnectionManager.Instance.StartConnectionManager();
        NetworkManager.Singleton.StartServer();
    }

    private async Task StartServerServices()
    {
        // Unity ���� �ʱ�ȭ
        await UnityServices.InitializeAsync();
        try
        {
            // ��Ƽ�÷��� �ν��Ͻ� ����
            multiplayService = MultiplayService.Instance;
            // Unity MultiplayService�� SQP ���� �ڵ鷯 ����
            await multiplayService.StartServerQueryHandlerAsync((ushort)ConnectionApprovalHandler.MaxPlayers, "n/a", "n/a", "0", "n/a");
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Something went wrong trying to set up the SQP Service:\n{ex}");
        }

        try
        {
            // matchmakerPayload(��ġ����Ŀ�� �߱��ϴ� ���̷ε�) ȹ�� �õ�. Ÿ�Ӿƿ��� ��� null ��ȯ
            matchmakerPayload = await MatchmakingManager.Instance.GetMatchmakerPayload(); 
            // ���̷ε带 ���������� �޾ƿԴ��� Ȯ��
            if (matchmakerPayload != null)
            {
                // ������ �÷��̾� ���� ���� ���·� ����
                await MultiplayService.Instance.ReadyServerForPlayersAsync();
                // ���� ���μ��� ����
                await BackfillManager.Instance.StartBackfill(matchmakerPayload);
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
}
#endif
