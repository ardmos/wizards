using System;
using Unity.Netcode;
using static ComponentValidator;

/// <summary>
/// 클라이언트측 서버 연결, 연결 상태 이벤트 처리 등을 수행합니다.
/// </summary>
public class ClientNetworkConnectionManager : NetworkBehaviour
{
    #region Singleton
    // 싱글톤 인스턴스입니다. 전역 접근을 위해 사용됩니다.
    public static ClientNetworkConnectionManager Instance { get; private set; }
    #endregion

    #region Events
    // 매치에 참가했을 때 발생하는 이벤트입니다.
    public event EventHandler OnMatchJoined;
    // 매치에서 나갔을 때 발생하는 이벤트입니다.
    public event EventHandler OnMatchExited;
    #endregion

    #region Constants
    // 에러 메시지 상수
    private const string ERROR_NETWORK_MANAGER_NOT_SET = "ClientNetworkConnectionManager NetworkManager.Singleton이 설정되지 않았습니다.";
    private const string ERROR_CURRENT_PLAYER_DATA_MANAGER_NOT_SET = "ClientNetworkConnectionManager CurrentPlayerDataManager.Instance가 설정되지 않았습니다.";
    private const string ERROR_LOCAL_PLAYER_DATA_MANAGER_CLIENT_NOT_SET = "ClientNetworkConnectionManager LocalPlayerDataManagerClient.Instance가 설정되지 않았습니다.";
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        // 싱글톤 인스턴스 초기화
        if (Instance == null && IsClient) Instance = this;
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// 클라이언트를 시작하고 네트워크 연결을 초기화합니다.
    /// </summary>
    public void StartClient()
    {
        if (!ValidateComponent(NetworkManager.Singleton, ERROR_NETWORK_MANAGER_NOT_SET)) return;

        // 네트워크 이벤트 리스너 등록
        NetworkManager.Singleton.OnClientConnectedCallback += Client_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += Client_OnClientDisconnectCallback;
        // 클라이언트 시작
        NetworkManager.Singleton.StartClient();
    }
    /// <summary>
    /// 클라이언트를 중지하고 네트워크 연결을 종료합니다.
    /// </summary>
    public void StopClient()
    {
        if (!ValidateComponent(NetworkManager.Singleton, ERROR_NETWORK_MANAGER_NOT_SET)) return;

        // 네트워크 이벤트 리스너 제거
        NetworkManager.Singleton.OnClientConnectedCallback -= Client_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback -= Client_OnClientDisconnectCallback;
        // 네트워크 매니저 종료
        NetworkManager.Singleton.Shutdown();
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// 클라이언트가 서버에 연결되었을 때 호출되는 콜백 메서드입니다.
    /// </summary>
    /// <param name="clientId">연결된 클라이언트의 ID</param>
    private void Client_OnClientConnectedCallback(ulong clientId)
    {
        if (!ValidateComponent(CurrentPlayerDataManager.Instance, ERROR_CURRENT_PLAYER_DATA_MANAGER_NOT_SET) ||
        !ValidateComponent(LocalPlayerDataManagerClient.Instance, ERROR_LOCAL_PLAYER_DATA_MANAGER_CLIENT_NOT_SET))
        {
            StopClient();
            return;
        }

        OnMatchJoined?.Invoke(this, EventArgs.Empty);
        // 서버에 플레이어 정보 추가
        CurrentPlayerDataManager.Instance.AddPlayerServerRPC(LocalPlayerDataManagerClient.Instance.GetPlayerInGameData());
    }
    /// <summary>
    /// 클라이언트가 서버와의 연결이 끊겼을 때 호출되는 콜백 메서드입니다.
    /// </summary>
    /// <param name="clientId">연결이 끊긴 클라이언트의 ID</param>
    private void Client_OnClientDisconnectCallback(ulong clientId)
    {
        OnMatchExited?.Invoke(this, EventArgs.Empty);
    }
    #endregion
}