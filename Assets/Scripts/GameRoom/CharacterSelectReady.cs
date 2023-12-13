using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
#if UNITY_SERVER || UNITY_EDITOR
using Unity.Services.Multiplay;
#endif  


public class CharacterSelectReady : NetworkBehaviour
{
    public static CharacterSelectReady Instance { get; private set; }
    public static event EventHandler OnInstanceCreated;

    public event EventHandler OnReadyChanged;
    public event EventHandler OnGameStarting;

    private Dictionary<ulong, bool> playerReadyDictionary;

    private void Awake()
    {
        Instance = this;
        OnInstanceCreated?.Invoke(this, EventArgs.Empty);
        playerReadyDictionary = new Dictionary<ulong, bool>();
        OnReadyChanged?.Invoke(this, EventArgs.Empty);
    }

    private async void Start()
    {
#if DEDICATED_SERVER
        Debug.Log("DEDICATED_SERVER CHARACTER SELECT READY");

        // ������ �÷��̾� ���� ����� ���·� ����
        Debug.Log("ReadyServerForPlayersAsync");
        await MultiplayService.Instance.ReadyServerForPlayersAsync();

        // �̰� ����� ���� �ȳ�
        Camera.main.enabled = false;
#endif
    }

    public void SetPlayerReady()
    {
        SetPlayerReadyServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerReadyServerRpc(ServerRpcParams serverRpcParams = default)
    {
        // Client�ʿ��� ������ ClientId ��ε�ĳ��Ʈ ����. ���� ȭ�鿡�� ���� ǥ�� �������ϱ� ����
        SetPlayerReadyClientRpc(serverRpcParams.Receive.SenderClientId);
        // �� ������ �����ʿ����� �����ϰ� ó���ϴ� �Ŷ� ������ �ʿ���.
        playerReadyDictionary[serverRpcParams.Receive.SenderClientId] = true;

        bool allClientsReady = true;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!playerReadyDictionary.ContainsKey(clientId) || !playerReadyDictionary[clientId])
            {
                // �� clientId �÷��̾�� ���� ���� �÷��̾��Դϴ�
                allClientsReady = false;
                break;
            }
        }

        // ��� �÷��̾ ���� ���� ���. ���� ������ �̵�
        if (allClientsReady)
        {
            // Game ������ �˸�
            OnGameStarting?.Invoke(this, EventArgs.Empty);
            LoadingSceneManager.LoadNetwork(LoadingSceneManager.Scene.GameScene);
        }
    }

    // Client�� ȭ�� ���� ǥ��
    [ClientRpc]
    private void SetPlayerReadyClientRpc(ulong clientId)
    {
        playerReadyDictionary[clientId] = true;

        OnReadyChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool IsPlayerReady(ulong clientId)
    {
        return playerReadyDictionary.ContainsKey(clientId) && playerReadyDictionary[clientId];
    }
}
