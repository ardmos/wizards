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

    // 클라이언트단에서 동작하는 이벤트 핸들러. 이 이벤트 핸들러를 통해 옵저버 패턴으로 매칭UI(PopupGameRoomUIController)를 업데이트 합니다.
    public event EventHandler OnPlayerReadyDictionaryClientChanged;

    private Dictionary<ulong, bool> playerReadyDictionaryOnClient;

    private void Awake()
    {
        Instance = this;
        playerReadyDictionaryOnClient = new Dictionary<ulong, bool>();
        OnPlayerReadyDictionaryClientChanged?.Invoke(this, EventArgs.Empty);
    }

    // Client쪽 화면 레디 표시
    [ClientRpc]
    public void SetPlayerReadyClientRpc(ulong clientId)
    {
        //Debug.Log($"GameMatchReadyManagerClient.SetPlayerReadyClientRpc Called. clientId{clientId}");
        playerReadyDictionaryOnClient[clientId] = true;

        OnPlayerReadyDictionaryClientChanged?.Invoke(this, EventArgs.Empty);
    }
    [ClientRpc]
    public void SetEveryPlayerUnReadyClientRpc()
    {
        playerReadyDictionaryOnClient.Keys.ToList().ForEach(clientId =>
        {
            playerReadyDictionaryOnClient[clientId] = false;
        });
        OnPlayerReadyDictionaryClientChanged?.Invoke(this, EventArgs.Empty);
    }

    public void ClearPlayerReadyList()
    {
        playerReadyDictionaryOnClient.Clear();
    }

    public bool IsPlayerReady(ulong clientId)
    {
        //Debug.Log($"player clientID: {clientId} is ready? {playerReadyDictionaryOnClient.ContainsKey(clientId) && playerReadyDictionaryOnClient[clientId]}");
        return playerReadyDictionaryOnClient.ContainsKey(clientId) && playerReadyDictionaryOnClient[clientId];
    }
}
