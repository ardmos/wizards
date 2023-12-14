using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Spawn network objects
/// </summary>

public class GameMultiplayer : NetworkBehaviour
{
    private const int MAX_PLAYER_AMOUNT = 4;

    private NetworkList<PlayerData> playerDataNetworkList;

    public static GameMultiplayer Instance { get; private set; }

    public event EventHandler OnTryingToJoinGame;
    public event EventHandler OnFailedToJoinGame;
    public event EventHandler OnPlayerDataNetworkListChanged;

    private void Awake()
    {
        Instance = this;

        DontDestroyOnLoad(gameObject);

        playerDataNetworkList = new NetworkList<PlayerData>();
        // CharacterSelectPlayer ���� ���Ѻ��� EventHandler
        playerDataNetworkList.OnListChanged += PlayerDataNetworkList_OnListChanged;
    }

    private void PlayerDataNetworkList_OnListChanged(NetworkListEvent<PlayerData> changeEvent)
    {
        OnPlayerDataNetworkListChanged?.Invoke(this, EventArgs.Empty);
    }

    private void NetworkManager_OnClientConnectedCallback(ulong clientId)
    {
        playerDataNetworkList.Add(new PlayerData
        {
            clientId = clientId,
        });
    }

/*    private void NetworkManager_ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        // GameRoom ������ Ȯ��
        if (SceneManager.GetActiveScene().name != LoadingSceneManager.Scene.GameRoomScene.ToString()) { 
            response.Approved = false;
            response.Reason = "Game has already started";
            return;
        }

        // Maximum Player Ȯ��
        if (NetworkManager.Singleton.ConnectedClientsIds.Count >= MAX_PLAYER_AMOUNT)
        {
            response.Approved = false;
            response.Reason = "Game is full";
            return;
        }

        response.Approved = true;
    }*/

    private void NetworkManager_Client_OnClientDisconnectCallback(ulong obj)
    {
        Debug.Log($"OnClientDisconnectCallback : {obj}");
        OnFailedToJoinGame?.Invoke(this, EventArgs.Empty);
    }

    // GameRoom���� Client�� ������ �� �÷��̾ �����ִ� �κ�.
    private void NetworkManager_Server_OnClientDisconnectCallback(ulong clientId)
    {
        for (int i = 0; i<playerDataNetworkList.Count; i++)
        {
            PlayerData playerData = playerDataNetworkList[i];
            if (playerData.clientId == clientId)
            {
                // �ش� �÷��̾� Disconnected
                playerDataNetworkList.RemoveAt(i);
            }
        }
    }
    public void StartHost()
    {
        //NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManager_ConnectionApprovalCallback;
        NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_Server_OnClientDisconnectCallback;
        NetworkManager.Singleton.StartHost();
    }

    // UGS Dedicated Server 
    public void StartServer()
    {
        //NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManager_ConnectionApprovalCallback;
        NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_Server_OnClientDisconnectCallback;
        NetworkManager.Singleton.StartServer();
    }

    public void StartClient()
    {
        OnTryingToJoinGame?.Invoke(this, EventArgs.Empty);

        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_Client_OnClientDisconnectCallback;
        NetworkManager.Singleton.StartClient();
    }

    // CharacterSelectPlayer���� �ش� �ε����� �÷��̾ ���� �Ǿ��� Ȯ���� �� ���
    public bool IsPlayerIndexConnected(int playerIndex)
    {
        return playerIndex < playerDataNetworkList.Count;
    }

    // �÷��̾� client�� �ܼ��� player Index�� ã�� �޼ҵ�
    public int GetPlayerDataIndexFromClientId(ulong clientId)
    {
        for (int i = 0; i < playerDataNetworkList.Count; i++)
        {
            if (playerDataNetworkList[i].clientId == clientId)
            {
                return i;
            }
        }
        return -1;
    }

    // �÷��̾� client�� �ܼ��� PlayerData(ClientId ���� ���� �÷��̾� ������)�� ã�� �޼ҵ�
    public PlayerData GetPlayerDataFromClientId(ulong clientId)
    {
        foreach (PlayerData playerData in playerDataNetworkList)
        {
            if (playerData.clientId == clientId)
            {
                return playerData;
            }
        }
        return default;
    }

    // �÷��̾� Index�� �ܼ��� PlayerData(ClientId ���� ���� �÷��̾� ������)�� ã�� �޼ҵ�
    public PlayerData GetPlayerDataFromPlayerIndex(int playerIndex)
    {
        return playerDataNetworkList[playerIndex];
    }

    // �÷��̾� kick
    public void KickPlayer(ulong clientId)
    {
        NetworkManager.Singleton.DisconnectClient(clientId);
        // �� DisconnectClient ���� �� �Ʒ� ������ �ڵ����� ȣ���� �ȵż� ���� ȣ������.
        NetworkManager_Server_OnClientDisconnectCallback(clientId);
    }

    public bool HasAvailablePlayerSlots()
    {
        return NetworkManager.Singleton.ConnectedClientsIds.Count < MAX_PLAYER_AMOUNT;
    }

    public NetworkList<PlayerData> GetPlayerDataNetworkList()
    {
        return playerDataNetworkList;
    }

    // �÷��̾� �̸� ����ִ� �κ� () ������ ��� NetworkManager_OnclientConnectedCallback���� ȣ�����ְ� Ŭ���̾�Ʈ�� ���StartClient���� ȣ���������
    // ���� �̸� ����ϴºκ��� ???? 
    /*[ServerRpc(RequireOwnership = false)]
    private void SetPlayerNameServerRpc(string playerName, ServerRpcParams serverRpcParams = default)
    {
        int playerDataIndex = GetPlayerDataIndexFromClientId(serverRpcParams.Receive.SenderClientId);

        PlayerData playerData = playerDataNetworkList[playerDataIndex];

        playerData.playerName = playerName;

        playerDataNetworkList[playerDataIndex] = playerData;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerIdServerRpc(string playerId, ServerRpcParams serverRpcParams = default)
    {
        int playerDataIndex = GetPlayerDataIndexFromClientId(serverRpcParams.Receive.SenderClientId);

        PlayerData playerData = playerDataNetworkList[playerDataIndex];

        playerData.playerId = playerId;

        playerDataNetworkList[playerDataIndex] = playerData;
    }*/
}
