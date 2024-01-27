using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using System.Linq;
#if UNITY_SERVER || UNITY_EDITOR
using Unity.Services.Multiplay;
#endif  
/// <summary>
/// Game Match ���� ���� ����. ����/Ŭ���̾�Ʈ �и� �ʿ�. ���� ���ӷ��� �ƴ϶� �κ񿡼� ���Ǳ� ������ Ŭ���� �̸� �������ʿ��մϴ�.
/// </summary>
public class GameRoomReadyManager : NetworkBehaviour
{
    public static GameRoomReadyManager Instance { get; private set; }
    public static event EventHandler OnInstanceCreated; // ���ӷ� ����� �� �̻��� �߰� ������ ������� �� �����. Backfill ����.

    public event EventHandler OnClintPlayerReadyDictionaryChanged;
    public event EventHandler OnGameStarting; // ���� ����� �� �̻��� �߰� ������ ������� �� �����. Backfill ����.

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

        // ������ �÷��̾� ���� ����� ���·� ����. Backfill�� ���� ��. ������ ServerStartup���� �� ����. �÷��̾� ������ ���� ���� ���� �ʿ��Ұ�츦 ����ؼ� �ּ����� ���ܵ�.
        //Debug.Log("ReadyServerForPlayersAsync");
        //await MultiplayService.Instance.ReadyServerForPlayersAsync();

        // �̰� ����� ���� �ȳ�
        //Camera.main.enabled = false;
#endif
    }

    /// <summary>
    /// ��� �÷��̾��� ������¸� ������ִ� �޼ҵ� �Դϴ�.
    /// Ŭ���̾�Ʈ�� ��Ī���� ������ ��쿡 ���˴ϴ�.
    /// </summary>
    /// <param name="serverRpcParams"></param>
    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerUnReadyServerRPC(ServerRpcParams serverRpcParams = default)
    {
        playerReadyDictionary.Keys.ToList().ForEach(clientId =>
        {
            playerReadyDictionary[clientId] = false;
        });

        // Ŭ���̾�Ʈ�� ��ųʸ��͵� ���� ����ȭ
        SetPlayerUnReadyClientRpc();
    }

    /// <summary>
    /// Ư�� playerIndex �÷��̾��� ������¸� ������ִ� �޼ҵ� �Դϴ�. 
    /// ��� �÷��̾ �������� ��� ������ �����մϴ�. 
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerReadyServerRpc(ServerRpcParams serverRpcParams = default)
    {
        Debug.Log($"���� ���� ���Խ��ϴ�. ������: clientId:{serverRpcParams.Receive.SenderClientId}, playerIndex:{GameMultiplayer.Instance.GetPlayerDataIndexFromClientId(serverRpcParams.Receive.SenderClientId)}");
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
