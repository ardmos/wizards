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
public class ServerStartUp : MonoBehaviour
{
    public static ServerStartUp Instance = null;

    private const string InternalServerIp = "0.0.0.0";
    private string externalServerIP = "0.0.0.0";
    // Default port 7777
    private ushort serverPort = 7777;

    private string externalConnectionString => $"{externalServerIP}:{serverPort}";

    private IMultiplayService multiplayService;
    private const int multiplayServiceTimeout = 20000; //20�ʰ���

    private string allocationId;
    private MultiplayEventCallbacks serverCallbacks;
    private IServerEvents serverEvents;


    
  
    private MatchmakingResults matchmakerPayload;



    async void Start()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
        
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
            // Command Line Args�� ���� �������� Ȯ���Ѵ�
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

        if (server)
        {
            StartServer();
            await StartServerServices();
        }
    }

    public void OnDestroy()
    {
        Dispose();
    }

    // ���� ����.
    private void StartServer()
    {
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(InternalServerIp, serverPort);
        ServerNetworkConnectionManager.Instance.StartConnectionManager();
        NetworkManager.Singleton.StartServer();
    }

    async Task StartServerServices()
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
            matchmakerPayload = await GetMatchmakerPayload(multiplayServiceTimeout); /////�������.
            // ���̷ε带 ���������� �޾ƿԴ��� Ȯ��
            if (matchmakerPayload != null)
            {
                // ������ �÷��̾� ���� ���� ���·� ����
                await MultiplayService.Instance.ReadyServerForPlayersAsync();
                // ���� ���μ��� ����
                await StartBackfill(matchmakerPayload);
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

    /// <summary>
    /// ��ġ����Ŀ ���̷ε带 �������� �۾��� �����ϴ� �񵿱� �޼����Դϴ�.
    /// ��ġ����Ŀ ���̷ε� �Ҵ��� ��ٸ���, ������ ��ٸ��� �ʵ��� Ÿ�Ӿƿ��� �����߽��ϴ�.
    /// </summary>
    /// <param name="timeout">Ÿ�Ӿƿ�</param>
    /// <returns>Ÿ�Ӿƿ��� null ��ȯ</returns>
    private async Task<MatchmakingResults> GetMatchmakerPayload(int timeout)
    {
        var matchmakerPayloadTask = SubscribeAndAwaitMatchmakerAllocation();

        if(await Task.WhenAny(matchmakerPayloadTask, Task.Delay(timeout))==matchmakerPayloadTask) 
        {
            return matchmakerPayloadTask.Result;
        }

        return null;
    }

    /// <summary>
    /// ��Ƽ�÷��� ���񽺿� �����ϰ�, ���� ���� �ν��Ͻ� �Ҵ� �� �Ҵ� ��� ���̷ε带 ��ٸ��� �޼����Դϴ�.
    /// ���� ��ġ����ŷ Backfill�� ���� �������� ������ �� �ֵ��� MatchmakingResult������ ���̷ε带 ��ȯ�մϴ�.
    /// </summary>
    /// <returns>��ġ����Ŀ ���̷ε带 ��ȯ�մϴ�</returns>
    private async Task<MatchmakingResults> SubscribeAndAwaitMatchmakerAllocation()
    {
        if(multiplayService == null) return null;

        allocationId = null;
        serverCallbacks = new MultiplayEventCallbacks();
        // ���� �̺�Ʈ ����
        serverEvents = await multiplayService.SubscribeToServerEventsAsync(serverCallbacks);
        // ��Ƽ�÷��� ���� ���� �ν��Ͻ��� �Ҵ�Ǿ��� ��� allocationId�� �����մϴ�.
        serverCallbacks.Allocate += OnMultiplayAllocation;
        // ���������� allocationId�� �����ϴ� �޼��带 await�մϴ�. ������ ���� ���� ������ġ�Դϴ�.
        allocationId = await AwaitAllocationID();
        // MatchmakingResults���·� ������ ��ġ����ŷ ���� �ν��Ͻ� �Ҵ� ��� ���̷ε� �޾ƿ���
        var matchmakingResultPayload = await GetMatchmakerAllocationPayloadAsync();
        return matchmakingResultPayload;
    }

    /// <summary>
    /// ��Ƽ�÷��� ���� ������ �Ҵ�Ǿ��� ��� �����ϴ� �̺�Ʈ �ڵ鷯�Դϴ�.
    /// allocationId�� �����մϴ�.
    /// </summary>
    /// <param name="allocation"></param>
    private void OnMultiplayAllocation(MultiplayAllocation allocation)
    {
        if(string.IsNullOrEmpty(allocationId)) return; 

        allocationId = allocation.AllocationId;
    }

    /// <summary>
    /// allocationId�� �����Ǳ⸦ ��ٸ��ٰ� �����Ǹ� �����ϴ� �޼����Դϴ�.
    /// ������ ������ OnMultiplayAllocation�� �۵����� ���� ��츦 ����� ���۽�Ű�� �޼����Դϴ�.
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// ��Ƽ�÷��� ���� �ν��Ͻ� �Ҵ� ����� �������� �޼����Դϴ�.
    /// </summary>
    /// <returns>MatchmakingResults�� ������ ��Ƽ�÷��� ���� �Ҵ� ���̷ε�</returns>
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

    

    // �޸� ������ ���� ���� �޼���. ��������� ��� �� ��������
    private void Dispose()
    {
        serverCallbacks.Allocate -= OnMultiplayAllocation;
        serverEvents?.UnsubscribeAsync();
    }
}
#endif
