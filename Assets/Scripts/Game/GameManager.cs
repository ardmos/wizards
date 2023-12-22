using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
#if UNITY_SERVER || UNITY_EDITOR
using Unity.Services.Multiplay;
#endif


/// <summary>
/// GameScene ���� �帧 ����
/// Singleton
/// EventSystem(Observer Pattern)
/// Game State Machine
/// ownerPlayerObject
///
/// </summary>

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    public event EventHandler OnStateChanged;
    public event EventHandler OnAlivePlayerCountChanged;

    public enum State
    {
        WatingToStart,
        CountdownToStart,
        GamePlaying,
        GameOver,
    }

    public NetworkVariable<State> state = new NetworkVariable<State>(State.WatingToStart); 
    private bool isLocalPlayerReady;
    private NetworkVariable<float> countdownToStartTimer = new NetworkVariable<float>(3f);
    private NetworkVariable<float> gamePlayingTimer = new NetworkVariable<float>(0f);
    private float gamePlayingTimerMax = 10f;
    private Dictionary<ulong, bool> playerReadyList;

    public NetworkList<ulong> playerGameOverListServer;

    [SerializeField] private int currentAlivePlayerCount;     

    void Awake()
    {
        Instance = this;
        playerReadyList = new Dictionary<ulong, bool>();
        playerGameOverListServer = new NetworkList<ulong>();
    }

    // Start is called before the first frame update
    async void Start()
    {
#if DEDICATED_SERVER
        // Backfill?? ?????? ServerStartup???? ???????? ?????? ???? ???? ????. ???? ?????? ?????? ????.
        // ������ �÷��̾� ���� ���� �� ����� ���·� ����
        //await MultiplayService.Instance.UnreadyServerAsync();

        // ���⼭�� �̰� ����� ���� �ȳ�
        //Camera.main.enabled = false;
#endif
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log($"GameManager OnNetworkSpawn state.Value:{state.Value}");
        state.OnValueChanged += State_OnValueChanged;
        state.Value = State.WatingToStart; // ������ ���ÿ� Default�� ����. 
        Debug.Log($"GameManager OnNetworkSpawn state.Value:{state.Value}");
        if (IsServer)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
        }
    }

    /// <summary>
    /// Game Scene �ε� �Ϸ�� ����˴ϴ�.
    /// 1. Player Character �ε�
    /// </summary>
    private void SceneManager_OnLoadEventCompleted(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            // Player Character Client ���� ���
            //GameObject player = Instantiate(GetCurrentPlayerCharacterPrefab(GameMultiplayer.Instance.GetPlayerDataFromClientId(clientId).playerClass));
            // Player Character Server ���� ���
            CharacterClasses.Class playerClass = GameMultiplayer.Instance.GetPlayerDataFromClientId(clientId).playerClass;
            GameObject playerPrefab = GetCurrentPlayerCharacterPrefab(playerClass);
            GameObject player = Instantiate(playerPrefab);
            if (player != null)
                player.transform.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
            else
                Debug.Log($"SceneManager_OnLoadEventCompleted : player prefab load failed. prefab is null");
        }
    }

    private GameObject GetCurrentPlayerCharacterPrefab(CharacterClasses.Class playerClass)
    {
        GameObject playerPrefab = null;
        switch (playerClass)
        {
            case CharacterClasses.Class.Wizard:
                playerPrefab = GameAssets.instantiate.wizard_Male;
                break;
            case CharacterClasses.Class.Knight:
                playerPrefab = GameAssets.instantiate.knight_Male;
                break;
            default:
                break;
        }
        return playerPrefab;
    }


    private void State_OnValueChanged(State previousValue, State newValue)
    {
        OnStateChanged?.Invoke(this, EventArgs.Empty);
        Debug.Log($"OnStateChanged!!!!! previousValue:{previousValue}, newValue:{newValue}");
    }

    // Update is called once per frame
    void Update()
    {
        // Update ��ſ� State �ٲ𶧸��� ȣ��Ǵ� Eventhandler ����ϸ� �ɰͰ�����! ���� ����
        RunStateMachine();
    }

    private void RunStateMachine()
    {
        if(!IsServer) { return; }

        switch (state.Value)
        {
            case State.WatingToStart:
                break;
            case State.CountdownToStart:
                countdownToStartTimer.Value -= Time.deltaTime;
                if (countdownToStartTimer.Value < 0f)
                {
                    state.Value = State.GamePlaying;
                    gamePlayingTimer.Value = gamePlayingTimerMax;
                }
                break;
            case State.GamePlaying:
                gamePlayingTimer.Value -= Time.deltaTime;
                if (gamePlayingTimer.Value < 0f)
                {
                    state.Value = State.GameOver;
                }
                break;
            case State.GameOver:
                break;
            default:
                break;
        }
        //Debug.Log(state);
    }

    // ������� ������ ����
    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerReadyServerRpc(ServerRpcParams serverRpcParams = default)
    {
        playerReadyList[serverRpcParams.Receive.SenderClientId] = true;

        bool allClientsReady = true;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if(!playerReadyList.ContainsKey(clientId) || !playerReadyList[clientId])
            {
                // �� clientId �÷��̾�� ���� ���� �÷��̾��Դϴ�
                allClientsReady = false;
                break;
            }
        }

        // ��� �÷��̾ ���� ���� ���. ī��Ʈ�ٿ� ����
        if (allClientsReady)
        {
            // �÷��̾� ī��Ʈ ���� ������Ʈ(�� ���� �������� �ο�.)
            UpdateCurrentAlivePlayerCount(NetworkManager.ConnectedClients.Count);
            Debug.Log($"allClientsReady state.Value:{state.Value}");
            state.Value = State.CountdownToStart;
            Debug.Log($"allClientsReady state.Value:{state.Value}");
        }

        Debug.Log($"SetPlayerReadyServerRpc game state:{state.Value}, allClientsReady: {allClientsReady}");
    }

    public void UpdateCurrentAlivePlayerCount(int playerCount)
    {
        // �������� �÷��̾� ����(���ӿ��� �ȴ��ϰ�)
        currentAlivePlayerCount = playerCount;
        OnAlivePlayerCountChanged?.Invoke(this, EventArgs.Empty);
    }

    // ���� ���۽� ���̴� Ready UI ��ư�� Ŭ������ �� �����ϴ� �޼��� �Դϴ�.
    public void LocalPlayerReady()
    {
        if (state.Value == State.WatingToStart)
        {
            Debug.Log($"LocalPlayerReady game state:{state.Value}");
            isLocalPlayerReady = true;
            SetPlayerReadyServerRpc();
        }
    }

    // Client ���ӿ�����
    // 1. �������� ����.  ������ ��ųʸ��� ClientID ���ӿ��� true�� ���.
    // 2. Ŭ���̾�Ʈ�� ��ųʸ� ����Ʈ���� �ش� ���� ����.
    // 3. �ش� Ŭ���̾�Ʈ GameOverUIPopup�� Ŭ���̾�Ʈ�� ��ųʸ� ����Ʈ OnValueChanged �̺�Ʈ�ڵ鷯�� ���� Ȱ��ȭ��.
    // 4. ������ ���ӿ��� False�� �ο� 1���� ��� �¸�.
    // (�̷��� �ϸ� ������� �߰� ����. ���� �̱���. ���⼭ �߰��ϸ� ��.)
    public void UpdatePlayerGameOver()
    {
        UpdatePlayerGameOverListServerRPC();
    } 
    [ServerRpc(RequireOwnership = false)]
    private void UpdatePlayerGameOverListServerRPC(ServerRpcParams serverRpcParams = default)
    {
        Debug.Log("UpdatePlayerGameOverListServerRPC");
        var clientId = serverRpcParams.Receive.SenderClientId;
        // GameOver �÷��̾� ����Ʈ ������Ʈ  (���� ���ӿ�����Ų��� �г��� ������ �̱���)
        playerGameOverListServer.Add(clientId);

        // ���ӿ����� �÷��̾� ȭ�鿡 ���ӿ���UI ����

        if (NetworkManager.ConnectedClients.ContainsKey(clientId))
        {
            // ���ӿ����ƴٴ� �÷��̾� ������Ʈ ã��
            NetworkClient networkClient = NetworkManager.ConnectedClients[clientId];
            // player���� ���ӿ��� �˾� ����� ��Ŵ
            networkClient.PlayerObject.GetComponent<Player>().PopupGameOverUI();
        }
    }

    public bool IsLocalPlayerReady()
    {
        return isLocalPlayerReady;
    }

    public bool IsWatingToStart()
    {
        return state.Value == State.WatingToStart;
    }

    public bool IsGamePlaying()
    {
        return state.Value == State.GamePlaying;
    }

    public bool IsCountdownToStartActive()
    {
        return state.Value == State.CountdownToStart;
    }
    public bool IsGameOver()
    {
        return state.Value == State.GameOver;
    }

    public float GetCountdownToStartTimer()
    {
        return countdownToStartTimer.Value;
    }

    public int GetCurrentAlivePlayerCount()
    {
        return currentAlivePlayerCount;
    }

    public float GetGamePlayingTimer()
    {
        if(gamePlayingTimer.Value == 0f) return 0f;

        return gamePlayingTimerMax - gamePlayingTimer.Value;
    }
}
