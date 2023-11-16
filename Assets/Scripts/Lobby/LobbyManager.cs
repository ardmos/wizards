using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
#if UNITY_SERVER || UNITY_EDITOR
using Unity.Services.Core;
using Unity.Services.Matchmaker.Models;
using Unity.Services.Matchmaker;
using Unity.Services.Multiplay;
#endif


/// <summary>
/// UGS Dedicated Server ����
/// </summary>

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager instance { get; private set; }

#if DEDICATED_SERVER
    private const ushort defaultMaxPlayers = 4;
    private const string defaultServerName = "MyServerName";
    private const string defaultGameType = "MyGameType";
    private const string defaultBuildId = "MyBuildId";
    private const string defaultMap = "MyMap";

    private float autoAllocateTimer = 9999999f;
    private bool alreadyAutoAllocated;
    private static IServerQueryHandler serverQueryHandler; // static so it doesn't get destroyed when this object is destroyed
    private string backfillTicketId;
    // USG doc ���� ���� ��û�� ��û ������ ���� 1�� �̻�.
    private float acceptBackfillTicketsTimer;
    private float acceptBackfillTicketsTimerMax = 1.1f;
    private PayloadAllocation payloadAllocation;
#endif

    private void Awake()
    {
        instance = this; 

        DontDestroyOnLoad(gameObject);

        InitializeUGSDedicatedServer();
    }

    private void Start()
    {
        GameMultiplayer.Instance.OnPlayerDataNetworkListChanged += GameMultiplayer_OnPlayerDataNetworkListChanged;
        CharacterSelectReady.OnInstanceCreated += CharacterSelectReady_OnInstanceCreated;
    }

    private void CharacterSelectReady_OnInstanceCreated(object sender, EventArgs e)
    {
        CharacterSelectReady.Instance.OnGameStarting += CharacterSelectReady_OnGameStarting;
    }

    private async void CharacterSelectReady_OnGameStarting(object sender, EventArgs e)
    {
#if DEDICATED_SERVER
        if(backfillTicketId != null)
        {
            // ���� Ƽ�� ����. �� �̻��� �÷��̾�� ���� ����
            await MatchmakerService.Instance.DeleteBackfillTicketAsync(backfillTicketId);
        }
#endif
    }

    private async void GameMultiplayer_OnPlayerDataNetworkListChanged(object sender, EventArgs e)
    {
#if DEDICATED_SERVER
        HandleUpdateBackfillTickets();

        if (GameMultiplayer.Instance.HasAvailablePlayerSlots())
        {
            await MultiplayService.Instance.ReadyServerForPlayersAsync();
        }
        else
        {
            await MultiplayService.Instance.UnreadyServerAsync();
        }
#endif
    }

    private void Update()
    {
#if DEDICATED_SERVER
        autoAllocateTimer -= Time.deltaTime;
        if (autoAllocateTimer <= 0f)
        {
            autoAllocateTimer = 999f;
            MultiplayEventCallbacks_Allocate(null);
        }

        if(serverQueryHandler != null)
        {
            if (NetworkManager.Singleton.IsServer)
            {
                // When player count changed 
                serverQueryHandler.CurrentPlayers = (ushort)NetworkManager.Singleton.ConnectedClientsIds.Count;
            }
            // Doc���� �� ������Ʈ���� ȣ�����ֶ�� ����
            serverQueryHandler.UpdateServerCheck();
        }

        // UGS doc���� back fill�� ���������� �� �� Ƽ�� ������ ������� ����. ���� ������ ���� ���������� ��ġ����Ŀ ���񽺿��� �����ϴµ� �ʿ��ϴٰ� ��. �̰� ���ϸ� ���� �Ҵ��� �ȵ�. ���ο� �÷��̾� ������
        if (backfillTicketId != null)
        {
            acceptBackfillTicketsTimer -= Time.deltaTime;
            if (acceptBackfillTicketsTimer <= 0f)
            {
                acceptBackfillTicketsTimer = acceptBackfillTicketsTimerMax;
                HandleBackfillTickets();
            }
        }
#endif
    }

    private async void InitializeUGSDedicatedServer()
    {
#if DEDICATED_SERVER
        Debug.Log("DEDICATED_SERVER LOBBY");

        // Listen to server events
        MultiplayEventCallbacks multiplayEventCallbacks = new MultiplayEventCallbacks();
        multiplayEventCallbacks.Allocate += MultiplayEventCallbacks_Allocate;
        multiplayEventCallbacks.Deallocate += MultiplayEventCallbacks_Deallocate;
        multiplayEventCallbacks.Error += MultiplayEventCallbacks_Error;
        multiplayEventCallbacks.SubscriptionStateChanged += MultiplayEventCallbacks_SubscriptionStateChanged;
        IServerEvents serverEvents = await MultiplayService.Instance.SubscribeToServerEventsAsync(multiplayEventCallbacks);

        // Start server query handler
        serverQueryHandler = await MultiplayService.Instance.StartServerQueryHandlerAsync(defaultMaxPlayers, defaultServerName, defaultGameType, defaultBuildId, defaultMap);

        // serverConfig�� ���� �̹� Allocated�ƴ��� Ȯ���ϴ� �κ�
        var serverConfig = MultiplayService.Instance.ServerConfig;
        if (serverConfig.AllocationId != "")
        {
            // Already Allocated
            MultiplayEventCallbacks_Allocate(new MultiplayAllocation("", serverConfig.ServerId, serverConfig.AllocationId));
        }
#endif
    }

#if DEDICATED_SERVER
    private void MultiplayEventCallbacks_SubscriptionStateChanged(MultiplayServerSubscriptionState obj)
    {
        Debug.Log("DEDICATED_SERVER MultiplayEventCallbacks_SubscriptionStateChanged");
        Debug.Log(obj);
    }

    private void MultiplayEventCallbacks_Error(MultiplayError obj)
    {
        Debug.Log("DEDICATED_SERVER MultiplayEventCallbacks_Error");
        Debug.Log(obj.Reason);
    }

    private void MultiplayEventCallbacks_Deallocate(MultiplayDeallocation obj)
    {
        Debug.Log("DEDICATED_SERVER MultiplayEventCallbacks_Deallocate");
    }

    private void MultiplayEventCallbacks_Allocate(MultiplayAllocation obj)
    {
        Debug.Log("DEDICATED_SERVER MultiplayEventCallbacks_Allocate");
        Debug.Log($"eventID:{obj.EventId}, serverID:{obj.ServerId}, allocationID:{obj.AllocationId}");

        if (alreadyAutoAllocated)
        {
            Debug.Log("Already auto allocated!");
            return;
        }

        alreadyAutoAllocated = true;

        // ������ ���� ���� �޾ƴ� ����
        var serverConfig = MultiplayService.Instance.ServerConfig;
        Debug.Log($"Server ID[{serverConfig.ServerId}]");
        Debug.Log($"AllocationID[{serverConfig.AllocationId}]");
        Debug.Log($"Port[{serverConfig.Port}]");
        Debug.Log($"QueryPort[{serverConfig.QueryPort}]");
        Debug.Log($"LogDirectory[{serverConfig.ServerLogDirectory}]");

        string ipv4Address = "0.0.0.0";
        ushort port = serverConfig.Port;
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData( ipv4Address, port, "0.0.0.0" );

        SetupBackfillTickets();

        GameMultiplayer.Instance.StartServer();
        LoadingSceneManager.LoadNetwork(LoadingSceneManager.Scene.GameRoomScene);
    }

    private async void SetupBackfillTickets()
    {
        Debug.Log("SetupBackfillTickets");
        // Json���·� ������ ���� �޾ƴ� backfillTicketID �����ص�
        payloadAllocation = await MultiplayService.Instance.GetPayloadAllocationFromJsonAs<PayloadAllocation>();

        backfillTicketId = payloadAllocation.BackfillTicketId;
        Debug.Log("backfillTicketId: " + backfillTicketId);

        acceptBackfillTicketsTimer = acceptBackfillTicketsTimerMax;
    }
    private async void HandleUpdateBackfillTickets()
    {
        if (backfillTicketId != null && payloadAllocation != null && GameMultiplayer.Instance.HasAvailablePlayerSlots())
        {
            Debug.Log("HandleUpdateBackfillTickets");

            List<Unity.Services.Matchmaker.Models.Player> playerList = new List<Unity.Services.Matchmaker.Models.Player>();

            // �� �÷��̾� �����ͱ��� ������ ����Ʈ
            foreach (PlayerData playerData in GameMultiplayer.Instance.GetPlayerDataNetworkList())
            {
                playerList.Add(new Unity.Services.Matchmaker.Models.Player(playerData.playerId.ToString()));
            }

            MatchProperties matchProperties = new MatchProperties(
                payloadAllocation.MatchProperties.Teams,
                playerList,
                payloadAllocation.MatchProperties.Region,
                payloadAllocation.MatchProperties.BackfillTicketId
            );

            try
            {
                // �� �۾��� �÷��̾��Ʈ�� ��ȭ�� ���� �� ���ֱ� ���ؼ� GameMultiplayer_OnPlayerDataNetworkListChanged �̺�Ʈ ������ ����� �� �Լ� ȣ��ǵ��� ��.
                await MatchmakerService.Instance.UpdateBackfillTicketAsync(payloadAllocation.BackfillTicketId,
                    new BackfillTicket(backfillTicketId, properties: new BackfillTicketProperties(matchProperties))
                );
            }
            catch (MatchmakerServiceException e)
            {
                Debug.Log("Error: " + e);
            }
        }
    }

    private async void HandleBackfillTickets()
    {
        if (GameMultiplayer.Instance.HasAvailablePlayerSlots())
        {
            BackfillTicket backfillTicket = await MatchmakerService.Instance.ApproveBackfillTicketAsync(backfillTicketId);
            backfillTicketId = backfillTicket.Id;
        }
    }

    [Serializable]
    public class PayloadAllocation
    {
        public Unity.Services.Matchmaker.Models.MatchProperties MatchProperties;
        public string GeneratorName;
        public string QueueName;
        public string PoolName;
        public string EnvironmentId;
        public string BackfillTicketId;
        public string MatchId;
        public string PoolId;
    }
#endif

}
