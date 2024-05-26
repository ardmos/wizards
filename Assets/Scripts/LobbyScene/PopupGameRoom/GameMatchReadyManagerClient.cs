using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class GameMatchReadyManagerClient : NetworkBehaviour
{
    public static GameMatchReadyManagerClient Instance { get; private set; }

    public const float readyCountdownMaxTime = 7f;

    // Ŭ���̾�Ʈ�ܿ��� �����ϴ� �̺�Ʈ �ڵ鷯. �� �̺�Ʈ �ڵ鷯�� ���� ������ �������� ��ĪUI(PopupGameRoomUIController)�� ������Ʈ �մϴ�.
    public event EventHandler OnPlayerReadyDictionaryClientChanged;

    private Dictionary<ulong, bool> playerReadyDictionaryOnClient;

    private void Awake()
    {
        Instance = this;
        playerReadyDictionaryOnClient = new Dictionary<ulong, bool>();
        OnPlayerReadyDictionaryClientChanged?.Invoke(this, EventArgs.Empty);
    }

    // Client�� ȭ�� ���� ǥ��
    [ClientRpc]
    public void SetPlayerReadyClientRpc(ulong clientId)
    {
        Debug.Log($"GameMatchReadyManagerClient.SetPlayerReadyClientRpc Called. clientId{clientId}");
        playerReadyDictionaryOnClient[clientId] = true;

        OnPlayerReadyDictionaryClientChanged?.Invoke(this, EventArgs.Empty);
    }
    [ClientRpc]
    public void SetPlayerUnReadyClientRpc()
    {
        playerReadyDictionaryOnClient.Keys.ToList().ForEach(clientId =>
        {
            playerReadyDictionaryOnClient[clientId] = false;
        });
        OnPlayerReadyDictionaryClientChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool IsPlayerReady(ulong clientId)
    {
        return playerReadyDictionaryOnClient.ContainsKey(clientId) && playerReadyDictionaryOnClient[clientId];
    }
}
