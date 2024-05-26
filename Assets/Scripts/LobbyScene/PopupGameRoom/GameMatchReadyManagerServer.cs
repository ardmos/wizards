using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class GameMatchReadyManagerServer : NetworkBehaviour
{
    public static GameMatchReadyManagerServer Instance { get; private set; }

    private Dictionary<ulong, bool> playerReadyDictionaryOnServer;

    private void Awake()
    {
        Instance = this;
        playerReadyDictionaryOnServer = new Dictionary<ulong, bool>(); 
    }

    /// <summary>
    /// 모든 플레이어의 레디상태를 취소해주는 메소드 입니다.
    /// 클라이언트가 매칭에서 나갔을 경우에 사용됩니다.
    /// </summary>
    /// <param name="serverRpcParams"></param>
    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerUnReadyServerRPC(ServerRpcParams serverRpcParams = default)
    {
        playerReadyDictionaryOnServer.Keys.ToList().ForEach(clientId =>
        {
            playerReadyDictionaryOnServer[clientId] = false;
        });

        // 클라이언트측 딕셔너리와도 내용 동기화
        GameMatchReadyManagerClient.Instance.SetPlayerUnReadyClientRpc();
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
        GameMatchReadyManagerClient.Instance.SetPlayerReadyClientRpc(serverRpcParams.Receive.SenderClientId);
        // 이 과정은 서버쪽에서만 저장하고 처리하는 거라 윗줄이 필요함.
        playerReadyDictionaryOnServer[serverRpcParams.Receive.SenderClientId] = true;

        bool allClientsReady = true;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!playerReadyDictionaryOnServer.ContainsKey(clientId) || !playerReadyDictionaryOnServer[clientId])
            {
                // 이 clientId 플레이어는 레디 안한 플레이어입니다
                allClientsReady = false;
                break;
            }
        }

        // 모든 플레이어가 레디 했을 경우. 게임 씬으로 이동
        if (allClientsReady)
        {
            // 1. 게임 시작 
            LoadSceneManager.LoadNetwork(LoadSceneManager.Scene.GameScene);

            // Game 시작을 알림
            //OnGameStarting?.Invoke(this, EventArgs.Empty); 지금은 안쓰고있습니다. BackFill 설정 다시 살릴 때 사용할것입니다.
        }
    }

    /// <summary>
    /// 서버AI용 레디 메서드.
    /// </summary>
    /// <param name="컴퓨터용클라이언트아이디"></param>
    public void SetAIPlayerReady(ulong 컴퓨터용클라이언트아이디)
    {
        Debug.Log($"AI 레디 보고가 들어왔습니다. 보고자: AIClientId:{컴퓨터용클라이언트아이디}, AIplayerIndex:{GameMultiplayer.Instance.GetPlayerDataIndexFromClientId(컴퓨터용클라이언트아이디)}");
        // 이 과정은 서버쪽에서만 저장하고 처리하는 거라 윗줄이 필요함.
        playerReadyDictionaryOnServer[컴퓨터용클라이언트아이디] = true;
        // Client쪽에도 레디한 ClientId 브로드캐스트 해줌. 각자 화면에서 레디 표시 띄워줘야하기 때문
        GameMatchReadyManagerClient.Instance.SetPlayerReadyClientRpc(컴퓨터용클라이언트아이디);
    }
}
