using System;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Spawn network objects
/// NetworkList ����
/// </summary>

public class GameMultiplayer : NetworkBehaviour
{
    // ConnectionApprovalHandler���� ��
    // private const int MAX_PLAYER_AMOUNT = 4;

    // ������ �������� �÷��̾���� �����Ͱ� ��� ����Ʈ
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
                Debug.Log($"Disconnected Player clientID: {clientId},");
                Debug.Log($"playerDataNetworkList Index: {i},");
                Debug.Log($"playerDataNetworkList.Count: {playerDataNetworkList.Count},");
                Debug.Log($"playerDataNetworkList[index]: {playerDataNetworkList[i]}");

                // �÷��̾� Disconnected. �ش� �ε��� ������ ����
                playerDataNetworkList.RemoveAt(i);
            }
        }
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
        NetworkManager.Singleton.OnClientConnectedCallback += Client_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_Client_OnClientDisconnectCallback;
        NetworkManager.Singleton.StartClient();
    }
    /// <summary>
    ///  Ŭ���̾�Ʈ ������. ���� ������ �� �ϵ�
    ///  1. NetworkList�� PlayerDataList�� ���� �������� Ŭ���� ������ ����Ѵ�.
    /// </summary>
    private void Client_OnClientConnectedCallback(ulong clientId)
    {
        // ����RPC�� ���� ������ ����
        ChangePlayerClass(PlayerProfileData.Instance.GetCurrentSelectedClass());
    }

    // CharacterSelectPlayer���� �ش� �ε����� �÷��̾ ���� �Ǿ��� Ȯ���� �� ���
    public bool IsPlayerIndexConnected(int playerIndex)
    {
        return playerIndex < playerDataNetworkList.Count;
    }

    // �÷��̾� clientID�� �ܼ��� player Index�� ã�� �޼ҵ�
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

    public bool HasAvailablePlayerSlots()
    {
        //return NetworkManager.Singleton.ConnectedClientsIds.Count < MAX_PLAYER_AMOUNT;
        return NetworkManager.Singleton.ConnectedClientsIds.Count < ConnectionApprovalHandler.MaxPlayers;
    }

    public NetworkList<PlayerData> GetPlayerDataNetworkList()
    {
        return playerDataNetworkList;
    }

    /// <summary>
    /// Ŭ���� ������ �������� ������ �� �ֽ��ϴ�.
    /// ���� ������ Server�� Allocate �� ����! 
    /// �� GameRoom�� ���鼭 �Դϴ�.
    /// </summary>
    /// <param name="playerClass"></param>    
    public void ChangePlayerClass(CharacterClasses.Class playerClass)
    {
        ChangePlayerClassServerRPC(playerClass);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangePlayerClassServerRPC(CharacterClasses.Class playerClass, ServerRpcParams serverRpcParams = default)
    {
        int playerDataIndex = GetPlayerDataIndexFromClientId(serverRpcParams.Receive.SenderClientId);
        PlayerData playerData = playerDataNetworkList[playerDataIndex];
        playerData.playerClass = playerClass;
        playerDataNetworkList[playerDataIndex] = playerData;
    }

    /// <summary>
    /// PlayerIndex�� ���� �÷��̾ �������� Class�� ������ ������Ʈ�� ���� �� �ֽ��ϴ�.
    /// 
    /// ������ ���⼭ ������� �ݿ��ؼ� ��ȯ�������. ������ Ŭ������ �ݿ��ؼ� ��ȯ����
    /// </summary>
    /// <returns></returns>
    public GameObject GetPlayerClassPrefabByPlayerIndex_NotForGameSceneObject(int playerIndex)
    {       
        GameObject resultObejct = null;
        switch (GetPlayerDataFromPlayerIndex(playerIndex).playerClass)
        {
            case CharacterClasses.Class.Wizard:
                resultObejct = GameAssets.instantiate.wizard_Male_ForLobby;
                break;
            case CharacterClasses.Class.Knight:
                resultObejct = GameAssets.instantiate.knight_Male_ForLobby;
                break;
            default:
                break;
        }     
        return resultObejct;
    }
    public GameObject GetPlayerClassPrefabByPlayerIndex_ForGameSceneObject(int playerIndex)
    {
        GameObject resultObejct = null;
        switch (GetPlayerDataFromPlayerIndex(playerIndex).playerClass)
        {
            case CharacterClasses.Class.Wizard:
                resultObejct = GameAssets.instantiate.wizard_Male;
                break;
            case CharacterClasses.Class.Knight:
                resultObejct = GameAssets.instantiate.knight_Male;
                break;
            default:
                break;
        }

        return resultObejct;
    }


    //

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
