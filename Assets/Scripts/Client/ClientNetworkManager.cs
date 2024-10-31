using System;
using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;


/// <summary>
/// 클라이언트 측 로직을 담당합니다. 서버 연결, 연결 상태 이벤트 처리 등을 수행합니다.
/// </summary>
public class ClientNetworkManager : NetworkBehaviour
{
    public static ClientNetworkManager Instance { get; private set; }

    public event EventHandler OnMatchJoined;
    public event EventHandler OnMatchExited;

    private void Awake()
    {
        if (Instance == null && IsClient)
        {
            Instance = this;
        }        
    }

    public void StartClient()
    {
        Debug.Log("StartClient()");
        NetworkManager.Singleton.OnClientConnectedCallback += Client_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += Client_OnClientDisconnectCallback;
        NetworkManager.Singleton.StartClient();
    }

    public void StopClient()
    {
        Debug.Log("StopClient()");
        NetworkManager.Singleton.OnClientConnectedCallback -= Client_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback -= Client_OnClientDisconnectCallback;
        NetworkManager.Singleton.Shutdown();
    }

    public int GetPlayerScore()
    {
        return GetPlayerData().score;
    }

    public PlayerInGameData GetPlayerData()
    {
        foreach (PlayerInGameData playerData in CurrentPlayerDataManager.Instance.GetCurrentPlayers())
        {
            if (playerData.clientId == OwnerClientId)
            {
                return playerData;
            }
        }

        return default;
    }

    private void Client_OnClientConnectedCallback(ulong clientId)
    {
        // 접속 성공을 알림
        OnMatchJoined?.Invoke(this, EventArgs.Empty);
        // 현 Player 정보를 서버측에 전달 
        CurrentPlayerDataManager.Instance.AddPlayerServerRPC(PlayerDataManager.Instance.GetPlayerInGameData());
    }
    private void Client_OnClientDisconnectCallback(ulong clientId)
    {
        // 매칭 UI 숨김을 위한 이벤트 핸들러 호출. 
        OnMatchExited?.Invoke(this, EventArgs.Empty);
    }
}
