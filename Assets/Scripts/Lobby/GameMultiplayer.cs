using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
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

    public void StartHost()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManager_ConnectionApprovalCallback;
        NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
        NetworkManager.Singleton.StartHost();
    }

    private void NetworkManager_OnClientConnectedCallback(ulong clientId)
    {
        playerDataNetworkList.Add(new PlayerData
        {
            clientId = clientId,
        });
    }

    private void NetworkManager_ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
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
    }

    public void StartClient()
    {
        OnTryingToJoinGame?.Invoke(this, EventArgs.Empty);

        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
        NetworkManager.Singleton.StartClient();
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong obj)
    {
        OnFailedToJoinGame.Invoke(this, EventArgs.Empty);
    }

    // CharacterSelectPlayer���� �ش� �ε����� �÷��̾ ���� �Ǿ��� Ȯ���� �� ���
    public bool IsPlayerIndexConnected(int playerIndex)
    {
        return playerIndex < playerDataNetworkList.Count;
    }

    // �÷��̾� Index�� �ܼ��� ClientId ���� ���� �÷��̾� �����͸� ã�� �޼ҵ�
    public PlayerData GetPlayerDataFromPlayerIndex(int playerIndex)
    {
        return playerDataNetworkList[playerIndex];
    }
}
