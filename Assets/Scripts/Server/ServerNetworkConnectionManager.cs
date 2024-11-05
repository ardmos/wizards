using Unity.Netcode;
using UnityEditor.PackageManager;

/// <summary>
/// ���� �� ��Ʈ��ũ ���� ������ ����ϴ� ���� ������ Ŭ�����Դϴ�.
/// �÷��̾� ����, ���� �������� ó���մϴ�.
/// </summary>
public class ServerNetworkConnectionManager : NetworkBehaviour, ICleanable
{
    public static ServerNetworkConnectionManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null && IsServer)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneCleanupManager.RegisterCleanableObject(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// ���� �����ڸ� �ʱ�ȭ�ϰ� �����մϴ�.
    /// </summary>
    public void StartConnectionManager()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += Server_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += Server_OnClientDisconnectCallback;
    }

    public override void OnNetworkDespawn()
    {
        NetworkManager.Singleton.OnClientConnectedCallback -= Server_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback -= Server_OnClientDisconnectCallback;
    }

    public override void OnDestroy()
    {
        SceneCleanupManager.UnregisterCleanableObject(this);
    }

    /// <summary>
    /// Ŭ���̾�Ʈ ���� �� ȣ��Ǵ� �ݹ� �޼����Դϴ�.
    /// ���ο� ���� ���� �� ��� AI �÷��̾ ���� ���·� �����մϴ�.
    /// </summary>
    private void Server_OnClientConnectedCallback(ulong obj)
    {
        if (GameMatchReadyManagerServer.Instance == null) return;
        GameMatchReadyManagerServer.Instance.SetEveryAIPlayersReady();
    }

    /// <summary>
    /// Ŭ���̾�Ʈ ���� ���� �� ȣ��Ǵ� �ݹ� �޼����Դϴ�.
    /// </summary>
    private void Server_OnClientDisconnectCallback(ulong clientId)
    {
        if (CurrentPlayerDataManager.Instance == null) return;
        if (GameMatchReadyManagerServer.Instance == null) return;
        if (AIPlayerGenerator.Instance == null) return;
        if (BackfillManager.Instance == null) return;

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

        // Backfill �����
        BackfillManager.Instance.RestartBackfill();
        // �÷��̾ ��Ż�� ���� ���� ���Ӿ��̶�� �ʿ��� �߰� ó�� ����
        GameSceneDisconnectHandler(clientId);
    }

    /// <summary>
    /// ��� �÷��̾ AI���� Ȯ�����ִ� �޼����Դϴ�
    /// </summary>
    /// <returns>��� �÷��̾ AI���� ���θ� ���� boolean ����</returns>
    private bool CheckIsEveryPlayerAnAI()
    {
        if(CurrentPlayerDataManager.Instance == null) return false;

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

    /// <summary>
    /// ���� �������� ���� ���� ó���� ����մϴ�.
    /// </summary>
    private void GameSceneDisconnectHandler(ulong clientId)
    {
        if (MultiplayerGameManager.Instance == null) return;
        if (CurrentPlayerDataManager.Instance == null) return;
        // clientId ��ȿ�� üũ
        if (CurrentPlayerDataManager.Instance.GetPlayerDataListIndexByClientId(clientId) == -1) return;

        // ���� ������ clientId �÷��̾ GameOver ó�� �մϴ�.
        MultiplayerGameManager.Instance.UpdatePlayerGameOverOnServer(clientId);

        // Ȥ�� �����ִ� �÷��̾ ���� ���, ������ �ٽ� �κ������ ���ư��ϴ�.
        if (CurrentPlayerDataManager.Instance.GetCurrentPlayerCount() <= 0)
        {
            Cleanup();
            LoadSceneManager.Load(LoadSceneManager.Scene.LobbyScene);
        }
    }

    /// <summary>
    /// Ư�� �ε����� �÷��̾ ����Ǿ� �ִ��� Ȯ���մϴ�.
    /// </summary>
    public bool IsPlayerIndexConnected(int toggleArrayPlayerIndex)
    {
        if(CurrentPlayerDataManager.Instance == null) return false;

        return toggleArrayPlayerIndex < CurrentPlayerDataManager.Instance.GetCurrentPlayerCount();
    }

    /// <summary>
    /// Ư�� ������ �̵��� �� ������Ʈ�� �ı��Ǵ� ����� �������������� �����߽��ϴ�.
    /// </summary>
    public void Cleanup()
    {
        Destroy(gameObject);
    }
}
