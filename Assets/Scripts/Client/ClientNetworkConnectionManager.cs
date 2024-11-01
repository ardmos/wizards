using System;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Ŭ���̾�Ʈ �� ������ ����մϴ�. ���� ����, ���� ���� �̺�Ʈ ó�� ���� �����մϴ�.
/// </summary>
public class ClientNetworkConnectionManager : NetworkBehaviour
{
    public static ClientNetworkConnectionManager Instance { get; private set; }

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

    private void Client_OnClientConnectedCallback(ulong clientId)
    {
        // ���� ������ �˸�
        OnMatchJoined?.Invoke(this, EventArgs.Empty);
        // �� Player ������ �������� ���� 
        CurrentPlayerDataManager.Instance.AddPlayerServerRPC(PlayerDataManager.Instance.GetPlayerInGameData());
    }
    private void Client_OnClientDisconnectCallback(ulong clientId)
    {
        // ��Ī UI ������ ���� �̺�Ʈ �ڵ鷯 ȣ��. 
        OnMatchExited?.Invoke(this, EventArgs.Empty);
    }
}
