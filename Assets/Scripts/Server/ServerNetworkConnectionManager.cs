using Unity.Netcode;

/// <summary>
/// ���� �� ������ ����մϴ�. �÷��̾� ������ ����, ���� �� ���� ���� ó�� ���� �����մϴ�.
/// </summary>
public class ServerNetworkConnectionManager : NetworkBehaviour
{
    public static ServerNetworkConnectionManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null && IsServer)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= Server_OnClientConnectedCallback;
            NetworkManager.Singleton.OnClientDisconnectCallback -= Server_OnClientDisconnectCallback;
        }
    }

    // UGS Dedicated Server
    public void StartServer()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += Server_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += Server_OnClientDisconnectCallback;
        NetworkManager.Singleton.StartServer();
    }

    private void Server_OnClientConnectedCallback(ulong obj)
    {
        if (GameMatchReadyManagerServer.Instance == null) return;
        // ���ο� ������ �����ϸ� �������̴� ��� AI�������� ���� ���·� ����ϴ�
        GameMatchReadyManagerServer.Instance.SetEveryAIPlayersReady();
    }

    /// <summary>
    /// �÷��̾ ���� ��쿡���� ó���Դϴ�.
    /// </summary>
    private void Server_OnClientDisconnectCallback(ulong clientId)
    {
        if (CurrentPlayerDataManager.Instance == null) return;
        if(GameMatchReadyManagerServer.Instance == null) return;
        if(AIPlayerGenerator.Instance == null) return;

        // �÷��̾� ������ ����
        CurrentPlayerDataManager.Instance.RemovePlayer(clientId);

        // �� ��ġ�� �������� ��� �÷��̾� ������� �ʱ�ȭ
        GameMatchReadyManagerServer.Instance.SetEveryPlayerUnReady();

        // ���� �÷��̾���� ���� AI���� Ȯ�� �� �׷��ٸ� ���� ���� ó��
        if (CheckIsEveryPlayerAnAI())
        {
            CurrentPlayerDataManager.Instance.RemoveAllPlayers();
            AIPlayerGenerator.Instance.ResetAIPlayerGenerator();
        }

        // �÷��̾ ��Ż�� ���� ���� ���Ӿ��̶�� �߰� ó�� ����
        GameSceneConnectionHandler(clientId);
    }

    private bool CheckIsEveryPlayerAnAI()
    {
        bool areAllPlayersAI = true;
        foreach (PlayerInGameData player in CurrentPlayerDataManager.Instance.GetCurrentPlayers())
        {
            if (!player.isAI)
            {
                areAllPlayersAI = false;
            }
        }
        return areAllPlayersAI;
    }

    private void GameSceneConnectionHandler(ulong clientId)
    {
        if (MultiplayerGameManager.Instance == null) return;

        // ���� clientId GameOver ó��
        if (CurrentPlayerDataManager.Instance.GetPlayerDataListIndexByClientId(clientId) != -1)
            MultiplayerGameManager.Instance.UpdatePlayerGameOverOnServer(clientId);

        // Ȥ�� ��� �÷��̾ ��������, ������ �ٽ� �κ������ ���ư���
        if (CurrentPlayerDataManager.Instance.GetCurrentPlayerCount() == 0)
        {
            CleanUp();
            // �κ������ �̵�
            LoadSceneManager.Load(LoadSceneManager.Scene.LobbyScene);
        }
    }

    // �κ������ ���Ʊ� �� �ʱ�ȭ
    private void CleanUp()
    {
        // Ŭ���̾�Ʈ ����� if �ɼ�.
#if UNITY_SERVER || UNITY_EDITOR
        if (MultiplayerGameManager.Instance != null)
        {
            MultiplayerGameManager.Instance.CleanUpChildObjects();
            Destroy(MultiplayerGameManager.Instance.gameObject);
        }
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
            Destroy(NetworkManager.Singleton.gameObject);
        }
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        if (ServerStartUp.Instance != null)
        {
            Destroy(ServerStartUp.Instance.gameObject);
        }
        if(CurrentPlayerDataManager.Instance != null)
        {
            Destroy(CurrentPlayerDataManager.Instance.gameObject);
        }
        if(AIPlayerGenerator.Instance != null)
            Destroy(AIPlayerGenerator.Instance.gameObject);
#endif
    }

    // GameRoomPlayerCharacter���� �ش� �ε����� �÷��̾ ���� �Ǿ��� Ȯ���� �� ���
    public bool IsPlayerIndexConnected(int toggleArrayPlayerIndex)
    {
        return toggleArrayPlayerIndex < CurrentPlayerDataManager.Instance.GetCurrentPlayerCount();
    }
}
