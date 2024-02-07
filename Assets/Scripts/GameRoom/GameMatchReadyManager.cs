using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using static PopupGameRoomUIController;
#if UNITY_SERVER || UNITY_EDITOR
#endif  
/// <summary>
/// Game Match ���� ���� ����. ����/Ŭ���̾�Ʈ �и� �ʿ�. ���� ���ӷ��� �ƴ϶� �κ񿡼� ���Ǳ� ������ Ŭ���� �̸� �������ʿ��մϴ�.
/// </summary>
public class GameMatchReadyManager : NetworkBehaviour
{
    public static GameMatchReadyManager Instance { get; private set; }
    public static event EventHandler OnInstanceCreated; // ���ӷ� ����� �� �̻��� �߰� ������ ������� �� �����. Backfill ����.
    public const float readyCountdownMaxTime = 5f;

    // Ŭ���̾�Ʈ�ܿ��� �����ϴ� �̺�Ʈ �ڵ鷯
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
            // 1. ���� ���� 
            // Game ������ �˸�
            //OnGameStarting?.Invoke(this, EventArgs.Empty); ������ �Ⱦ����ֽ��ϴ�. BackFill ���� �ٽ� �츱 �� ����Ұ��Դϴ�.
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

    /// <summary>
    /// �������� ���� Ŭ���̾�Ʈ���� �˷��ݴϴ�.
    /// </summary>
    /// <returns>Ŭ���̾�ƮID�� ��� ����Ʈ�� �����մϴ�</returns>
    private List<ulong> GetUnReadyPlayerList()
    {
        List<ulong> resultList = new List<ulong>();

        playerReadyDictionary.Keys.ToList().ForEach(clientId =>
        {
            if(!IsPlayerReady(clientId)) resultList.Add(clientId);
        });

        return resultList;
    }

    public bool IsPlayerReady(ulong clientId)
    {
        return playerReadyDictionary.ContainsKey(clientId) && playerReadyDictionary[clientId];
    }
}
