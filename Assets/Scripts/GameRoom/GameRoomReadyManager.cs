using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using System.Linq;
#if UNITY_SERVER || UNITY_EDITOR
using Unity.Services.Multiplay;
#endif  
/// <summary>
/// Game Match 레디 상태 관리. 서버/클라이언트 분리 필요. 이제 게임룸이 아니라 로비에서 사용되기 때문에 클래스 이름 변경이필요합니다.
/// </summary>
public class GameRoomReadyManager : NetworkBehaviour
{
    public static GameRoomReadyManager Instance { get; private set; }
    public static event EventHandler OnInstanceCreated; // 게임룸 입장시 더 이상의 중간 진입을 막고싶을 때 사용함. Backfill 차단.

    public event EventHandler OnClintPlayerReadyDictionaryChanged;
    public event EventHandler OnGameStarting; // 게임 입장시 더 이상의 중간 진입을 막고싶을 때 사용함. Backfill 차단.

    private Dictionary<ulong, bool> playerReadyDictionary;

    private void Awake()
    {
        Instance = this;
        OnInstanceCreated?.Invoke(this, EventArgs.Empty);
        playerReadyDictionary = new Dictionary<ulong, bool>();
        OnClintPlayerReadyDictionaryChanged?.Invoke(this, EventArgs.Empty);
    }

    private async void Start()
    {
#if DEDICATED_SERVER
        Debug.Log("DEDICATED_SERVER GAME ROOM READY MANAGER");

        // 서버를 플레이어 수용 대기중 상태로 만듦. Backfill을 위한 것. 지금은 ServerStartup에서 다 관리. 플레이어 재접속 문제 관련 참고가 필요할경우를 대비해서 주석으로 남겨둠.
        //Debug.Log("ReadyServerForPlayersAsync");
        //await MultiplayService.Instance.ReadyServerForPlayersAsync();

        // 이거 해줘야 에러 안남
        //Camera.main.enabled = false;
#endif
    }

    /// <summary>
    /// 모든 플레이어의 레디상태를 취소해주는 메소드 입니다.
    /// 클라이언트가 매칭에서 나갔을 경우에 사용됩니다.
    /// </summary>
    /// <param name="serverRpcParams"></param>
    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerUnReadyServerRPC(ServerRpcParams serverRpcParams = default)
    {
        playerReadyDictionary.Keys.ToList().ForEach(clientId =>
        {
            playerReadyDictionary[clientId] = false;
        });

        // 클라이언트측 딕셔너리와도 내용 동기화
        SetPlayerUnReadyClientRpc();
    }

    /// <summary>
    /// 특정 playerIndex 플레이어의 레디상태를 등록해주는 메소드 입니다. 
    /// 모든 플레이어가 레디했을 경우 게임을 시작합니다. 
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerReadyServerRpc(ServerRpcParams serverRpcParams = default)
    {
        Debug.Log($"레디 보고가 들어왔습니다. 보고자: clientId:{serverRpcParams.Receive.SenderClientId}, playerIndex:{GameMultiplayer.Instance.GetPlayerDataIndexFromClientId(serverRpcParams.Receive.SenderClientId)}");
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

        OnClintPlayerReadyDictionaryChanged?.Invoke(this, EventArgs.Empty);
    }
    [ClientRpc]
    private void SetPlayerUnReadyClientRpc()
    {
        playerReadyDictionary.Keys.ToList().ForEach(clientId =>
        {
            playerReadyDictionary[clientId] = false;
        });
        OnClintPlayerReadyDictionaryChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool IsPlayerReady(ulong clientId)
    {
        return playerReadyDictionary.ContainsKey(clientId) && playerReadyDictionary[clientId];
    }
}
