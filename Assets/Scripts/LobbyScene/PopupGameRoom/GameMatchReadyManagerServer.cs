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
    /// ��� �÷��̾��� ������¸� ������ִ� �޼ҵ� �Դϴ�.
    /// Ŭ���̾�Ʈ�� ��Ī���� ������ ��쿡 ���˴ϴ�.
    /// </summary>
    /// <param name="serverRpcParams"></param>
    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerUnReadyServerRPC(ServerRpcParams serverRpcParams = default)
    {
        playerReadyDictionaryOnServer.Keys.ToList().ForEach(clientId =>
        {
            playerReadyDictionaryOnServer[clientId] = false;
        });

        // Ŭ���̾�Ʈ�� ��ųʸ��͵� ���� ����ȭ
        GameMatchReadyManagerClient.Instance.SetPlayerUnReadyClientRpc();
    }

    /// <summary>
    /// Ư�� playerIndex �÷��̾��� ������¸� ������ִ� �޼ҵ� �Դϴ�. 
    /// ��� �÷��̾ �������� ��� ������ �����մϴ�. 
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerReadyServerRpc(ServerRpcParams serverRpcParams = default)
    {
        Debug.Log($"���� ������ ���Խ��ϴ�. ������: clientId:{serverRpcParams.Receive.SenderClientId}, playerIndex:{GameMultiplayer.Instance.GetPlayerDataIndexFromClientId(serverRpcParams.Receive.SenderClientId)}");
        // Client�ʿ��� ������ ClientId ��ε�ĳ��Ʈ ����. ���� ȭ�鿡�� ���� ǥ�� �������ϱ� ����
        GameMatchReadyManagerClient.Instance.SetPlayerReadyClientRpc(serverRpcParams.Receive.SenderClientId);
        // �� ������ �����ʿ����� �����ϰ� ó���ϴ� �Ŷ� ������ �ʿ���.
        playerReadyDictionaryOnServer[serverRpcParams.Receive.SenderClientId] = true;

        bool allClientsReady = true;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!playerReadyDictionaryOnServer.ContainsKey(clientId) || !playerReadyDictionaryOnServer[clientId])
            {
                // �� clientId �÷��̾�� ���� ���� �÷��̾��Դϴ�
                allClientsReady = false;
                break;
            }
        }

        // ��� �÷��̾ ���� ���� ���. ���� ������ �̵�
        if (allClientsReady)
        {
            // 1. ���� ���� 
            LoadSceneManager.LoadNetwork(LoadSceneManager.Scene.GameScene);

            // Game ������ �˸�
            //OnGameStarting?.Invoke(this, EventArgs.Empty); ������ �Ⱦ����ֽ��ϴ�. BackFill ���� �ٽ� �츱 �� ����Ұ��Դϴ�.
        }
    }

    /// <summary>
    /// ����AI�� ���� �޼���.
    /// </summary>
    /// <param name="��ǻ�Ϳ�Ŭ���̾�Ʈ���̵�"></param>
    public void SetAIPlayerReady(ulong ��ǻ�Ϳ�Ŭ���̾�Ʈ���̵�)
    {
        Debug.Log($"AI ���� ������ ���Խ��ϴ�. ������: AIClientId:{��ǻ�Ϳ�Ŭ���̾�Ʈ���̵�}, AIplayerIndex:{GameMultiplayer.Instance.GetPlayerDataIndexFromClientId(��ǻ�Ϳ�Ŭ���̾�Ʈ���̵�)}");
        // �� ������ �����ʿ����� �����ϰ� ó���ϴ� �Ŷ� ������ �ʿ���.
        playerReadyDictionaryOnServer[��ǻ�Ϳ�Ŭ���̾�Ʈ���̵�] = true;
        // Client�ʿ��� ������ ClientId ��ε�ĳ��Ʈ ����. ���� ȭ�鿡�� ���� ǥ�� �������ϱ� ����
        GameMatchReadyManagerClient.Instance.SetPlayerReadyClientRpc(��ǻ�Ϳ�Ŭ���̾�Ʈ���̵�);
    }
}