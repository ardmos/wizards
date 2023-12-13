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

        // 서버를 플레이어 수용 대기중 상태로 만듦
        Debug.Log("ReadyServerForPlayersAsync");
        await MultiplayService.Instance.ReadyServerForPlayersAsync();

        // 이거 해줘야 에러 안남
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
        // Client쪽에도 레디한 ClientId 브로드캐스트 해줌. 각자 화면에서 레디 표시 띄워줘야하기 때문
        SetPlayerReadyClientRpc(serverRpcParams.Receive.SenderClientId);
        // 이 과정은 서버쪽에서만 저장하고 처리하는 거라 윗줄이 필요함.
        playerReadyDictionary[serverRpcParams.Receive.SenderClientId] = true;

        bool allClientsReady = true;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!playerReadyDictionary.ContainsKey(clientId) || !playerReadyDictionary[clientId])
            {
                // 이 clientId 플레이어는 레디 안한 플레이어입니다
                allClientsReady = false;
                break;
            }
        }

        // 모든 플레이어가 레디 했을 경우. 게임 씬으로 이동
        if (allClientsReady)
        {
            // Game 시작을 알림
            OnGameStarting?.Invoke(this, EventArgs.Empty);
            LoadingSceneManager.LoadNetwork(LoadingSceneManager.Scene.GameScene);
        }
    }

    // Client쪽 화면 레디 표시
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
