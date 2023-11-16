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
/// UGS Dedicated Server 세팅
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
    // USG doc 권장 따름 요청과 요청 사이의 갭은 1초 이상.
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
            // 백필 티켓 삭제. 더 이상의 플레이어는 받지 않음
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
            // Doc에서 매 업데이트마다 호출해주라고 했음
            serverQueryHandler.UpdateServerCheck();
        }

        // UGS doc에서 back fill도 마찬가지로 매 초 티켓 승인을 받으라고 했음. 게임 서버가 아직 실행중임을 매치메이커 서비스에게 증명하는데 필요하다고 함. 이걸 안하면 백필 할당이 안됨. 새로운 플레이어 못들어옴
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

        // serverConfig를 통해 이미 Allocated됐는지 확인하는 부분
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

        // 생성된 서버 정보 받아다 접속
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
        // Json형태로 들어오는 정보 받아다 backfillTicketID 저장해둠
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

            // 새 플레이어 데이터까지 포함한 리스트
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
                // 이 작업을 플레이어리스트에 변화가 있을 때 해주기 위해서 GameMultiplayer_OnPlayerDataNetworkListChanged 이벤트 리스너 사용해 현 함수 호출되도록 함.
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
