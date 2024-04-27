using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
#if UNITY_SERVER || UNITY_EDITOR
using Unity.Services.Multiplay;
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

    [SerializeField] private NetworkVariable<GameState> gameState = new NetworkVariable<GameState>(GameState.WatingToStart);
    [SerializeField] private NetworkVariable<int> startedPlayerCount = new NetworkVariable<int>(0);
    [SerializeField] private NetworkVariable<int> currentAlivePlayerCount = new NetworkVariable<int>(0);
    [SerializeField] private NetworkVariable<float> countdownToStartTimer = new NetworkVariable<float>(3f);
    [SerializeField] private NetworkVariable<float> gamePlayingTimer = new NetworkVariable<float>(0f);

    [SerializeField] private ushort gameOverPlayerCount = 0;
    [SerializeField] private float gamePlayingTimerMax = 10000f;
    [SerializeField] private Dictionary<ulong, bool> playerReadyList;
    [SerializeField] private bool isLocalPlayerReady;


    void Awake()
    {
        Instance = this;
        playerReadyList = new Dictionary<ulong, bool>();
    }

    // Start is called before the first frame update
    async void Start()
    {
#if UNITY_SERVER || UNITY_EDITOR
        // Game�� ����!  ���̻� �÷��̾� ������ �ʿ����� �ʽ��ϴ�. ������ �÷��̾� ���� ���¸� ����� ���·� �����մϴ�.
        await MultiplayService.Instance.UnreadyServerAsync();
#endif
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log($"GameManager. OnNetworkSpawn");
        if (IsServer)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
        }
        gameState.OnValueChanged += State_OnValueChanged;
        gameState.Value = GameState.WatingToStart; // ������ ���ÿ� Default�� ����. 
        currentAlivePlayerCount.OnValueChanged += currentAlivePlayerCount_OnValueChanged;
    }

    public override void OnNetworkDespawn()
    {
        Debug.Log($"GameManager. OnNetworkDespawn");
        if (IsServer)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= SceneManager_OnLoadEventCompleted;
        }
        gameState.OnValueChanged -= State_OnValueChanged;
        currentAlivePlayerCount.OnValueChanged -= currentAlivePlayerCount_OnValueChanged;
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
            Character playerClass = GameMultiplayer.Instance.GetPlayerDataFromClientId(clientId).characterClass;
            GameObject playerPrefab = GetCurrentPlayerCharacterPrefab(playerClass);
            GameObject player = Instantiate(playerPrefab);
            if (player != null)
                player.transform.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
            else
                Debug.Log($"SceneManager_OnLoadEventCompleted : player prefab load failed. prefab is null");
        }
    }

    private GameObject GetCurrentPlayerCharacterPrefab(Character playerClass)
    {
        GameObject playerPrefab = null;
        switch (playerClass)
        {
            case Character.Wizard:
                playerPrefab = GameAssetsManager.Instance.gameAssets.wizard_Male;
                break;
            case Character.Knight:
                playerPrefab = GameAssetsManager.Instance.gameAssets.knight_Male;
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
        if (!IsServer) { return; }

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
                    // ������ PlayerData
                    PlayerInGameData winPlayer = new PlayerInGameData();
                    foreach (PlayerInGameData playerData in GameMultiplayer.Instance.GetPlayerDataNetworkList())
                    {
                        if (playerData.playerGameState == PlayerGameState.Playing)
                        {
                            winPlayer = playerData;
                        }
                    }

                    if (!NetworkManager.ConnectedClients.ContainsKey(winPlayer.clientId)) return;

                    if (winPlayer.playerGameState == PlayerGameState.Win) return;

                    // ������ State Win ���� ����
                    winPlayer.playerGameState = PlayerGameState.Win;
                    GameMultiplayer.Instance.SetPlayerDataFromClientId(winPlayer.clientId, winPlayer);

                    // ������ ȭ�鿡 Win �˾� ����
                    NetworkClient networkClient = NetworkManager.ConnectedClients[winPlayer.clientId];
                    networkClient.PlayerObject.GetComponent<PlayerClient>().SetPlayerGameWinClientRPC();
                }
                // Ÿ�Ӿƿ�.
                else if (gamePlayingTimer.Value < 0f)
                {
                    // Ÿ�Ӿƿ����� ���� ���. �����ڵ� Draw. ó��
                    // �����ڵ� State Draw�� ����

                    // �����ڵ� ȭ�鿡 Draw �˾� ����

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
            if (!playerReadyList.ContainsKey(clientId) || !playerReadyList[clientId])
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

        Debug.Log($"SetPlayerReadyServerRpc Requested player client ID: {serverRpcParams.Receive.SenderClientId}, playerReadyList.Count: {playerReadyList.Count}");
        Debug.Log($"SetPlayerReadyServerRpc game state:{gameState.Value}, allClientsReady: {allClientsReady}");
    }

    /// <summary>
    /// ���������� �÷��̾� ���� ī��Ʈ ������Ʈ
    /// </summary>
    public void UpdateCurrentAlivePlayerCount()
    {
        // �������� �÷��̾� ����(���ӿ��� �ȴ��ϰ�)
        currentAlivePlayerCount.Value = startedPlayerCount.Value - gameOverPlayerCount;
        //Debug.Log($"UpdateCurrentAlivePlayerCount playerCount:{currentAlivePlayerCount.Value}");
    }

    // Ŭ���̾�Ʈ���� ȣ���ϴ� �޼ҵ�
    // ���� ���۽� ���̴� Ready UI ��ư�� Ŭ������ �� �����ϴ� �޼��� �Դϴ�.
    public void LocalPlayerReadyOnClient()
    {
        if (gameState.Value == GameState.WatingToStart)
        {
            Debug.Log($"LocalPlayerReady game state:{gameState.Value}");
            isLocalPlayerReady = true;
            SetPlayerReadyServerRpc();
        }
    }

    /// <summary>
    /// �������� �����ϴ� �޼ҵ��Դϴ�. ���� ���� ���¸� ����մϴ�.
    /// // Client ���ӿ�����
    /// 1. PlayerDataNetworkList���� �ش� ClientId�� PlayerGameState ���� GameOver�� �����մϴ�.
    /// 2. ���� �÷������� �÷��̾���ڸ� �����ִ� ��� UI�� ���� AlivePlayersCount ���� ������Ʈ �մϴ�.
    /// 3. NotifyUI�� ���� ���ӿ��� �� �÷��̾�� ���ӿ��� ��Ų �÷��̾��� �г����� �������ݴϴ�. (���ӿ��� ��Ų �÷��̾��� �г��� ������ ���� �̱����Դϴ�.)
    /// </summary>
    /// <param name="serverRpcParams"></param>
    public void UpdatePlayerGameOverOnServer(ulong clientId)
    {
        //Debug.Log($"UpdatePlayerGameOverOnServer. gameOver player : player{clientId}");

        if (NetworkManager.ConnectedClients.ContainsKey(clientId))
        {
            // ������ ����� PlayerDataList���� �÷��̾� ���� ������Ʈ
            PlayerInGameData playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(clientId);

            if (playerData.playerGameState == PlayerGameState.GameOver)
            {
                Debug.Log($"player{clientId}�� �̹� ���ӿ���ó���� �÷��̾��Դϴ�.");
                return;
            }

            playerData.playerGameState = PlayerGameState.GameOver;
            Debug.Log($"UpdatePlayerGameOverOnServer. player.clientId:{clientId}. playerGameState:{playerData.playerGameState}");
            GameMultiplayer.Instance.SetPlayerDataFromClientId(clientId, playerData);

            // �������� ��� Client���� NotifyUI�� ���� ���ӿ��� �� �÷��̾��� �г����� ��ε�ĳ��Ʈ���ݴϴ�.(���ӿ�����Ų��� �г��� ���������� ���� �̱���)
            GameSceneUIManager.Instance.notifyUIController.ShowGameOverPlayerClientRPC(playerData.playerName.ToString());

            // ��� UI�� ���� AlivePlayersCount �� ������Ʈ
            gameOverPlayerCount++;
            UpdateCurrentAlivePlayerCount();
        }
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
        if (gamePlayingTimer.Value == 0f) return 0f;

        return gamePlayingTimerMax - gamePlayingTimer.Value;
    }
}
