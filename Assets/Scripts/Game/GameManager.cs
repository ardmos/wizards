using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
#if UNITY_SERVER || UNITY_EDITOR
//using Unity.Services.Multiplay;
#endif


/// <summary>
/// Server���� �����ϴ� ���� �Ŵ���.  
/// Server Auth ��Ŀ����� ���� �Ŵ����� �翬�� �������� ���ư����Ѵ�. 
/// Ȥ�� ����Auth��� �ƴ� �κе� ������ �����ؾ���.
/// 
/// GameScene ���� �帧 ����
/// Singleton
/// EventSystem(Observer Pattern)
/// Game State Machine
/// ownerPlayerObject
/// </summary>

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    public event EventHandler OnGameStateChanged;
    public event EventHandler OnAlivePlayerCountChanged;
    public event EventHandler OnGameOverListChanged;

    [SerializeField] private NetworkList<ulong> playerGameOverList;
    [SerializeField] private NetworkVariable<GameState> gameState = new NetworkVariable<GameState>(GameState.WatingToStart);
    [SerializeField] private NetworkVariable<int> startedPlayerCount = new NetworkVariable<int>(0);
    [SerializeField] private NetworkVariable<int> currentAlivePlayerCount = new NetworkVariable<int>(0);
    [SerializeField] private NetworkVariable<float> countdownToStartTimer = new NetworkVariable<float>(3f);
    [SerializeField] private NetworkVariable<float> gamePlayingTimer = new NetworkVariable<float>(0f);

    [SerializeField] private float gamePlayingTimerMax = 10000f;
    [SerializeField] private Dictionary<ulong, bool> playerReadyList;
    [SerializeField] private bool isLocalPlayerReady;
 

    void Awake()
    {
        Instance = this;
        playerReadyList = new Dictionary<ulong, bool>();
        playerGameOverList = new NetworkList<ulong>();
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
        gameState.OnValueChanged += State_OnValueChanged;
        gameState.Value = GameState.WatingToStart; // ������ ���ÿ� Default�� ����. 
        if (IsServer)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
        }
        currentAlivePlayerCount.OnValueChanged += currentAlivePlayerCount_OnValueChanged;
        playerGameOverList.OnListChanged += PlayerGameOverList_OnListChanged;
    }

    private void PlayerGameOverList_OnListChanged(NetworkListEvent<ulong> changeEvent)
    {
        OnGameOverListChanged?.Invoke(this, EventArgs.Empty);
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
            CharacterClass playerClass = GameMultiplayer.Instance.GetPlayerDataFromClientId(clientId).playerClass;
            GameObject playerPrefab = GetCurrentPlayerCharacterPrefab(playerClass);
            GameObject player = Instantiate(playerPrefab);
            if (player != null)
                player.transform.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
            else
                Debug.Log($"SceneManager_OnLoadEventCompleted : player prefab load failed. prefab is null");
        }
    }

    private GameObject GetCurrentPlayerCharacterPrefab(CharacterClass playerClass)
    {
        GameObject playerPrefab = null;
        switch (playerClass)
        {
            case CharacterClass.Wizard:
                playerPrefab = GameAssets.instantiate.wizard_Male;
                break;
            case CharacterClass.Knight:
                playerPrefab = GameAssets.instantiate.knight_Male;
                break;
            default:
                break;
        }
        return playerPrefab;
    }


    private void State_OnValueChanged(GameState previousValue, GameState newValue)
    {
        OnGameStateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void currentAlivePlayerCount_OnValueChanged(int previousValue, int newValue)
    {
        OnAlivePlayerCountChanged?.Invoke(this, EventArgs.Empty);
    }

    // Update is called once per frame
    void Update()
    {
        // Update ��ſ� State �ٲ𶧸��� ȣ��Ǵ� Eventhandler ����ϱ�  <--- �������. �ð� ����� EventListener�� �и��ؼ� �����ϸ� ��.
        RunStateMachine();
    }

    private void RunStateMachine()
    {
        if(!IsServer) { return; }

        switch (gameState.Value)
        {
            case GameState.WatingToStart:
                break;
            case GameState.CountdownToStart:
                countdownToStartTimer.Value -= Time.deltaTime;
                if (countdownToStartTimer.Value < 0f)
                {
                    gameState.Value = GameState.GamePlaying;
                    gamePlayingTimer.Value = gamePlayingTimerMax;
                }
                break;
            case GameState.GamePlaying:
                gamePlayingTimer.Value -= Time.deltaTime;

                // ���� ������ Ÿ�Ӿƿ����� �켱��.
                // ���� ����
                if (currentAlivePlayerCount.Value == 1)
                {
                    gameState.Value = GameState.GameFinished;
                }
                // Ÿ�Ӿƿ�.
                else if (gamePlayingTimer.Value < 0f)
                {
                    gameState.Value = GameState.GameFinished;
                }

                break;
            case GameState.GameFinished:
                // ���� ������ Ÿ�Ӿƿ����� �켱��.
                // ���� ����
                if (currentAlivePlayerCount.Value == 1)
                {
                    // ���� �������� ���� ���. ������ Win ó��    <<<<<<<<----------------- �������!
                    
                }
                // Ÿ�Ӿƿ�.
                else if (gamePlayingTimer.Value < 0f)
                {
                    // Ÿ�Ӿƿ����� ���� ���. �����ڵ� Draw. ó��
                    
                }

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
            startedPlayerCount.Value = NetworkManager.ConnectedClients.Count;
            UpdateCurrentAlivePlayerCount();
            //Debug.Log($"allClientsReady state.Value:{state.Value}");
            gameState.Value = GameState.CountdownToStart;
            //Debug.Log($"allClientsReady state.Value:{state.Value}");
        }

        //Debug.Log($"SetPlayerReadyServerRpc game state:{state.Value}, allClientsReady: {allClientsReady}");
    }

    public void UpdateCurrentAlivePlayerCount()
    {
        // �������� �÷��̾� ����(���ӿ��� �ȴ��ϰ�)
        currentAlivePlayerCount.Value = startedPlayerCount.Value - playerGameOverList.Count;        
        Debug.Log($"UpdateCurrentAlivePlayerCount playerCount:{currentAlivePlayerCount.Value}");
    }

    // Ŭ���̾�Ʈ���� ȣ���ϴ� �޼ҵ�
    // ���� ���۽� ���̴� Ready UI ��ư�� Ŭ������ �� �����ϴ� �޼��� �Դϴ�.
    public void LocalPlayerReadyOnClient()
    {
        if (gameState.Value == GameState.WatingToStart)
        {
            //Debug.Log($"LocalPlayerReady game state:{state.Value}");
            isLocalPlayerReady = true;
            SetPlayerReadyServerRpc();
        }
    }

    /// <summary>
    /// �������� �����ϴ� �޼ҵ��Դϴ�. ������ ����Ǵ� ���ӿ����� ����� ������Ʈ �մϴ�.
    /// // Client ���ӿ�����
    /// 1. �������� ����.  ������ ��ųʸ��� ClientID ���ӿ��� true�� ���.
    /// 2. Ŭ���̾�Ʈ�� ��ųʸ� ����Ʈ���� �ش� ���� ����.
    /// 3. �ش� Ŭ���̾�Ʈ GameOverUIPopup�� Ŭ���̾�Ʈ�� ��ųʸ� ����Ʈ OnValueChanged �̺�Ʈ�ڵ鷯�� ���� Ȱ��ȭ��.
    /// 4. ������ ���ӿ��� False�� �ο� 1���� ��� �¸�.
    /// (�̷��� �ϸ� ������� �߰� ����. ���� �̱���. ���⼭ �߰��ϸ� ��.)
    /// </summary>
    /// <param name="serverRpcParams"></param>
    public void UpdatePlayerGameOverListOnServer(ulong clientId)
    {
        Debug.Log($"UpdatePlayerGameOverListOnServer. gameOver player : player{clientId}");

        if (NetworkManager.ConnectedClients.ContainsKey(clientId) && !playerGameOverList.Contains(clientId))
        {
            AddGameOverPlayer(clientId);
        }
    }
    public void AddGameOverPlayer(ulong clientId) {
        // GameOver �÷��̾� ����Ʈ ������Ʈ  (���ӿ�����Ų��� �г��� ������ ���� �̱���)
        playerGameOverList.Add(clientId);
        // ��� UI�� ���� AlivePlayersCount �� ������Ʈ
        UpdateCurrentAlivePlayerCount();
    }

    public bool IsLocalPlayerReady()
    {
        return isLocalPlayerReady;
    }

    public bool IsWatingToStart()
    {
        return gameState.Value == GameState.WatingToStart;
    }

    public bool IsGamePlaying()
    {
        return gameState.Value == GameState.GamePlaying;
    }

    public bool IsCountdownToStartActive()
    {
        return gameState.Value == GameState.CountdownToStart;
    }
    public bool IsGameFinished()
    {
        return gameState.Value == GameState.GameFinished;
    }

    public float GetCountdownToStartTimer()
    {
        return countdownToStartTimer.Value;
    }

    public int GetCurrentAlivePlayerCount()
    {
        return currentAlivePlayerCount.Value;
    }

    public float GetGamePlayingTimer()
    {
        if(gamePlayingTimer.Value == 0f) return 0f;

        return gamePlayingTimerMax - gamePlayingTimer.Value;
    }

    public ulong GetLastGameOverPlayer()
    {
        return playerGameOverList[playerGameOverList.Count - 1];
    }
}
