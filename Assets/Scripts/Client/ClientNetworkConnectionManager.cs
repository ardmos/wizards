using System;
using Unity.Netcode;
using static ComponentValidator;

/// <summary>
/// Ŭ���̾�Ʈ�� ���� ����, ���� ���� �̺�Ʈ ó�� ���� �����մϴ�.
/// </summary>
public class ClientNetworkConnectionManager : NetworkBehaviour
{
    #region Singleton
    // �̱��� �ν��Ͻ��Դϴ�. ���� ������ ���� ���˴ϴ�.
    public static ClientNetworkConnectionManager Instance { get; private set; }
    #endregion

    #region Events
    // ��ġ�� �������� �� �߻��ϴ� �̺�Ʈ�Դϴ�.
    public event EventHandler OnMatchJoined;
    // ��ġ���� ������ �� �߻��ϴ� �̺�Ʈ�Դϴ�.
    public event EventHandler OnMatchExited;
    #endregion

    #region Constants
    // ���� �޽��� ���
    private const string ERROR_NETWORK_MANAGER_NOT_SET = "ClientNetworkConnectionManager NetworkManager.Singleton�� �������� �ʾҽ��ϴ�.";
    private const string ERROR_CURRENT_PLAYER_DATA_MANAGER_NOT_SET = "ClientNetworkConnectionManager CurrentPlayerDataManager.Instance�� �������� �ʾҽ��ϴ�.";
    private const string ERROR_LOCAL_PLAYER_DATA_MANAGER_CLIENT_NOT_SET = "ClientNetworkConnectionManager LocalPlayerDataManagerClient.Instance�� �������� �ʾҽ��ϴ�.";
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        // �̱��� �ν��Ͻ� �ʱ�ȭ
        if (Instance == null && IsClient) Instance = this;
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Ŭ���̾�Ʈ�� �����ϰ� ��Ʈ��ũ ������ �ʱ�ȭ�մϴ�.
    /// </summary>
    public void StartClient()
    {
        if (!ValidateComponent(NetworkManager.Singleton, ERROR_NETWORK_MANAGER_NOT_SET)) return;

        // ��Ʈ��ũ �̺�Ʈ ������ ���
        NetworkManager.Singleton.OnClientConnectedCallback += Client_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += Client_OnClientDisconnectCallback;
        // Ŭ���̾�Ʈ ����
        NetworkManager.Singleton.StartClient();
    }
    /// <summary>
    /// Ŭ���̾�Ʈ�� �����ϰ� ��Ʈ��ũ ������ �����մϴ�.
    /// </summary>
    public void StopClient()
    {
        if (!ValidateComponent(NetworkManager.Singleton, ERROR_NETWORK_MANAGER_NOT_SET)) return;

        // ��Ʈ��ũ �̺�Ʈ ������ ����
        NetworkManager.Singleton.OnClientConnectedCallback -= Client_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback -= Client_OnClientDisconnectCallback;
        // ��Ʈ��ũ �Ŵ��� ����
        NetworkManager.Singleton.Shutdown();
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Ŭ���̾�Ʈ�� ������ ����Ǿ��� �� ȣ��Ǵ� �ݹ� �޼����Դϴ�.
    /// </summary>
    /// <param name="clientId">����� Ŭ���̾�Ʈ�� ID</param>
    private void Client_OnClientConnectedCallback(ulong clientId)
    {
        if (!ValidateComponent(CurrentPlayerDataManager.Instance, ERROR_CURRENT_PLAYER_DATA_MANAGER_NOT_SET) ||
        !ValidateComponent(LocalPlayerDataManagerClient.Instance, ERROR_LOCAL_PLAYER_DATA_MANAGER_CLIENT_NOT_SET))
        {
            StopClient();
            return;
        }

        OnMatchJoined?.Invoke(this, EventArgs.Empty);
        // ������ �÷��̾� ���� �߰�
        CurrentPlayerDataManager.Instance.AddPlayerServerRPC(LocalPlayerDataManagerClient.Instance.GetPlayerInGameData());
    }
    /// <summary>
    /// Ŭ���̾�Ʈ�� �������� ������ ������ �� ȣ��Ǵ� �ݹ� �޼����Դϴ�.
    /// </summary>
    /// <param name="clientId">������ ���� Ŭ���̾�Ʈ�� ID</param>
    private void Client_OnClientDisconnectCallback(ulong clientId)
    {
        OnMatchExited?.Invoke(this, EventArgs.Empty);
    }
    #endregion
}