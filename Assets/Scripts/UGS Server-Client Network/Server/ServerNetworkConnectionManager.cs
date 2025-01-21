using System.Diagnostics;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using static ComponentValidator;

/// <summary>
/// ���� �� ��Ʈ��ũ ������ �����ϴ� Ŭ�����Դϴ�.
/// �÷��̾��� ����, ���� ���� �� ���� �̺�Ʈ�� ó���մϴ�.
/// </summary>
public class ServerNetworkConnectionManager : NetworkBehaviour, ICleanable
{
    #region Singleton
    // ServerNetworkConnectionManager�� �̱��� �ν��Ͻ��Դϴ�.
    public static ServerNetworkConnectionManager Instance { get; private set; }
    #endregion

    #region Constants
    // ���� �޽��� �����
    private const string ERROR_NETWORK_MANAGER_NOT_SET = "ServerNetworkConnectionManager NetworkManager.Singleton ��ü�� �������� �ʾҽ��ϴ�.";
    private const string ERROR_GAME_MATCH_READY_MANAGER_NOT_SET = "ServerNetworkConnectionManager GameMatchReadyManagerServer.Instance ��ü�� �������� �ʾҽ��ϴ�.";
    private const string ERROR_CURRENT_PLAYER_DATA_MANAGER_NOT_SET = "ServerNetworkConnectionManager CurrentPlayerDataManager.Instance ��ü�� �������� �ʾҽ��ϴ�.";
    private const string ERROR_AI_PLAYER_GENERATOR_NOT_SET = "ServerNetworkConnectionManager AIPlayerGenerator.Instance ��ü�� �������� �ʾҽ��ϴ�.";
    private const string ERROR_BACKFILL_MANAGER_NOT_SET = "ServerNetworkConnectionManager BackfillManager.Instance ��ü�� �������� �ʾҽ��ϴ�.";
    private const string ERROR_MULTIPLAYER_GAME_MANAGER_NOT_SET = "ServerNetworkConnectionManager MultiplayerGameManager.Instance ��ü�� �������� �ʾҽ��ϴ�.";
    #endregion

    #region Unity Lifecycle
    /// <summary>
    /// �̱��� �ν��Ͻ��� �ʱ�ȭ�ϰ� �� ��ȯ �� �ı����� �ʵ��� �����մϴ�.
    /// ���ÿ� �� ���� �Ŵ����� ���� ������ν� ����մϴ�.
    /// </summary>
    private void Awake()
    {
        if (Instance == null)
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
    /// ��Ʈ��ũ ��ü�� ������ �� ȣ��Ǵ� �޼����Դϴ�.
    /// ��ϵ� Ŭ���̾�Ʈ ���� �� ���� ���� �̺�Ʈ �ݹ��� �����մϴ�.
    /// </summary>
    public override void OnNetworkDespawn()
    {
        if (!ValidateComponent(NetworkManager.Singleton, ERROR_NETWORK_MANAGER_NOT_SET)) return;

        NetworkManager.Singleton.OnClientConnectedCallback -= Server_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback -= Server_OnClientDisconnectCallback;
    }

    /// <summary>
    /// ������Ʈ �ı��� �� ���� �Ŵ����� �ص״� ����� �����մϴ�.
    /// </summary>
    public override void OnDestroy()
    {
        SceneCleanupManager.UnregisterCleanableObject(this);
    }
    #endregion

    #region Initialization
    /// <summary>
    /// ������ ���۵� �� ȣ��Ǿ� Ŀ�ؼ� �Ŵ����� �ʱ�ȭ�����ִ� �޼����Դϴ�.
    /// Ŭ���̾�Ʈ ���� �� ���� ���� �̺�Ʈ�� ���� �ݹ��� ����մϴ�.
    /// </summary>
    public void InitalizeConnectionManager()
    {
        if (!ValidateComponent(NetworkManager.Singleton, ERROR_NETWORK_MANAGER_NOT_SET)) return;

        NetworkManager.Singleton.OnClientConnectedCallback += Server_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += Server_OnClientDisconnectCallback;
    }
    #endregion

    #region Player Management
    /// <summary>
    /// Ư�� �ε����� �÷��̾ ����Ǿ� �ִ��� Ȯ���մϴ�.
    /// </summary>
    /// <param name="toggleArrayPlayerIndex">Ȯ���� �÷��̾��� �ε���</param>
    /// <returns>�÷��̾ ����Ǿ� ������ true, �׷��� ������ false</returns>
    public bool IsPlayerIndexConnected(int toggleArrayPlayerIndex)
    {
        if (!ValidateComponent(CurrentPlayerDataManager.Instance, ERROR_CURRENT_PLAYER_DATA_MANAGER_NOT_SET)) return false;

        return toggleArrayPlayerIndex < CurrentPlayerDataManager.Instance.GetCurrentPlayerCount();
    }

    /// <summary>
    /// ���� �������� �÷��̾� ���� ������ ó���մϴ�.
    /// ���� ������ �÷��̾ ���� ���� ó���ϰ�, ���� �÷��̾ ���� ��� �κ�� ���ư��ϴ�.
    /// </summary>
    /// <param name="clientId">���� ������ Ŭ���̾�Ʈ�� ID</param>
    public void HandlePlayerDisconnectInGameScene(ulong clientId)
    {
        if (SceneManager.GetActiveScene().name != LoadSceneManager.Scene.GameScene_MultiPlayer.ToString()) {
            Logger.Log($"���Ӿ��� �ƴմϴ�");
            return; 
        }

        SetDisconnectedPlayerGameOver(clientId);
        RemoveClientData(clientId); // Ŭ���̾�Ʈ ������ ����
        HandleAllAIScenarioInGameScene();
    }

    /// <summary>
    /// Ŭ���̾�Ʈ �����͸� �����մϴ�.
    /// </summary>
    /// <param name="clientId">������ Ŭ���̾�Ʈ�� ID</param>
    private void RemoveClientData(ulong clientId)
    {
        if (!ValidateComponent(CurrentPlayerDataManager.Instance, ERROR_CURRENT_PLAYER_DATA_MANAGER_NOT_SET)) return;
        CurrentPlayerDataManager.Instance.RemovePlayer(clientId);
    }

    /// <summary>
    /// ��� �÷��̾��� �غ� ���¸� �ʱ�ȭ�մϴ�.
    /// </summary>
    private void ResetAllPlayersReadyState()
    {
        if (!ValidateComponent(GameMatchReadyManagerServer.Instance, ERROR_GAME_MATCH_READY_MANAGER_NOT_SET)) return;
        GameMatchReadyManagerServer.Instance.SetEveryPlayerUnReady();
    }

    /// <summary>
    /// ������ ���� �÷��̾ ���� ���� ���·� �����մϴ�.
    /// </summary>
    /// <param name="clientId">���� ���� ó���� Ŭ���̾�Ʈ�� ID</param>
    private void SetDisconnectedPlayerGameOver(ulong clientId)
    {
        if (!ValidateComponent(MultiplayerGameManager.Instance, ERROR_MULTIPLAYER_GAME_MANAGER_NOT_SET)) return;

        MultiplayerGameManager.Instance.UpdatePlayerGameOverOnServer(clientId);
    }
    #endregion

    #region AI Player Management
    /// <summary>
    /// ���Ӿ����� ��� �÷��̾ AI�� ����� �ó������� ó���մϴ�.
    /// AI�� ������ ��� ��� �÷��̾� �����͸� �ʱ�ȭ�ϰ� �κ�� ���ư��ϴ�.
    /// </summary>
    private void HandleAllAIScenarioInGameScene()
    {
        if (!ValidateComponent(CurrentPlayerDataManager.Instance, ERROR_CURRENT_PLAYER_DATA_MANAGER_NOT_SET)) return;

        if (CheckIsEveryPlayerAnAI())
        {
            Logger.Log("��� �ΰ� �÷��̾ ��Ż�߽��ϴ�. ������ �����մϴ�.");
            CurrentPlayerDataManager.Instance.RemoveAllPlayers();
            LoadSceneManager.Load(LoadSceneManager.Scene.LobbyScene);
        }
    }

    /// <summary>
    /// �κ������ ��� �÷��̾ AI�� ����� �ó������� ó���մϴ�.
    /// AI�� ������ ��� ��� �÷��̾� �����͸� �ʱ�ȭ�ϰ� AI �����⸦ �ʱ�ȭ�մϴ�.
    /// </summary>
    private void HandleAllAIScenarioInLobbyScene()
    {
        if (!ValidateComponent(CurrentPlayerDataManager.Instance, ERROR_CURRENT_PLAYER_DATA_MANAGER_NOT_SET)) return;
        if (!ValidateComponent(AIPlayerGenerator.Instance, ERROR_AI_PLAYER_GENERATOR_NOT_SET)) return;

        if (CheckIsEveryPlayerAnAI())
        {
            CurrentPlayerDataManager.Instance.RemoveAllPlayers();
            AIPlayerGenerator.Instance.ResetAIPlayerGenerator();
        }
    }

    /// <summary>
    /// ���� ���ӿ� ���� ���� ��� �÷��̾ AI���� Ȯ���մϴ�.
    /// </summary>
    /// <returns>��� �÷��̾ AI�� true, �׷��� ������ false</returns>
    private bool CheckIsEveryPlayerAnAI()
    {
        if (!ValidateComponent(CurrentPlayerDataManager.Instance, ERROR_CURRENT_PLAYER_DATA_MANAGER_NOT_SET)) return false;

        bool isAllAI = true;
        foreach (var player in CurrentPlayerDataManager.Instance.GetCurrentPlayers())
        {
            if (!player.isAI)
            {
                isAllAI = false;
            }
        }
        return isAllAI;
    }
    #endregion

    #region Backfill Management
    /// <summary>
    /// ���� ���μ����� �簳�մϴ�.
    /// </summary>
    private void RestartBackfill()
    {
        if (!ValidateComponent(BackfillManager.Instance, ERROR_BACKFILL_MANAGER_NOT_SET)) return;
        _ = BackfillManager.Instance.RestartBackfill();
    }
    #endregion

    #region Event Handlers
    /// <summary>
    /// Ŭ���̾�Ʈ ���� �� ȣ��Ǵ� �ݹ� �޼����Դϴ�.
    /// ���ο� ���� ���� �� ��� AI �÷��̾ �غ� ���·� �����մϴ�.
    /// </summary>
    private void Server_OnClientConnectedCallback(ulong clientId)
    {
        if (!ValidateComponent(GameMatchReadyManagerServer.Instance, ERROR_GAME_MATCH_READY_MANAGER_NOT_SET)) return;

        GameMatchReadyManagerServer.Instance.SetEveryAIPlayersReady();
    }

    /// <summary>
    /// Ŭ���̾�Ʈ ���� ���� �� ȣ��Ǵ� �ݹ� �޼����Դϴ�.
    /// �÷��̾� ������ ����, �غ� ���� �ʱ�ȭ, AI �ó����� ó��, ���� �����, ���Ӿ������� �÷��̾� ���� ���� ���� �����մϴ�.
    /// </summary>
    /// <param name="clientId">���� ������ Ŭ���̾�Ʈ�� ID</param>
    private void Server_OnClientDisconnectCallback(ulong clientId)
    {
        if (SceneManager.GetActiveScene().name == LoadSceneManager.Scene.GameScene_MultiPlayer.ToString())
            HandlePlayerDisconnectInGameScene(clientId);
        else
        {
            RemoveClientData(clientId); // Ŭ���̾�Ʈ ������ ����
            ResetAllPlayersReadyState();
            HandleAllAIScenarioInLobbyScene();
            RestartBackfill();
        }
    }
    #endregion

    #region ICleanable ����
    /// <summary>
    /// ICleanable �������̽� ����. ��ü�� �����մϴ�.
    /// �� �޼���� �� ��ȯ �� ȣ��Ǿ� ���� ��ü�� �ı��մϴ�.
    /// </summary>
    public void Cleanup()
    {
        Destroy(gameObject);
    }
    #endregion
}