using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using System.Linq;
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

public class MultiplayerGameManager : NetworkBehaviour
{
    public static MultiplayerGameManager Instance { get; private set; }

    public event EventHandler OnGameStateChanged;
    public event EventHandler OnAlivePlayerCountChanged;
    public event EventHandler<PlayerGameOverEventArgs> OnPlayerGameOver;

    public AudioListener audioListenerServerOnly;
    public PlayerSpawnPointsController spawnPointsController;

    [SerializeField] private NetworkVariable<GameState> gameState = new NetworkVariable<GameState>(GameState.WatingToStart);
    [SerializeField] private NetworkVariable<int> startedPlayerCount = new NetworkVariable<int>(0);
    [SerializeField] private NetworkVariable<int> currentAlivePlayerCount = new NetworkVariable<int>(0);
    [SerializeField] private NetworkVariable<float> countdownToStartTimer = new NetworkVariable<float>(3f);
    [SerializeField] private NetworkVariable<float> gamePlayingTimer = new NetworkVariable<float>(0f);

    [SerializeField] private ushort gameOverPlayerCount = 0;
    [SerializeField] private float gamePlayingTimerMax;
    [SerializeField] private Dictionary<ulong, bool> playerReadyList;
    [SerializeField] private bool isLocalPlayerReady;
    [SerializeField] private bool isBGMStarted;

    void Awake()
    {
        Instance = this;
        playerReadyList = new Dictionary<ulong, bool>();
    }

    // Start is called before the first frame update
    async void Start()
    {
#if UNITY_SERVER //|| UNITY_EDITOR
        // Game�� ����!  ���̻� �÷��̾� ������ �ʿ����� �ʽ��ϴ�. ������ �÷��̾� ���� ���¸� ����� ���·� �����մϴ�.
        await MultiplayService.Instance.UnreadyServerAsync();    
#endif
        // ������ ������ ����� �����ʸ� Ȱ��ȭ�մϴ�
        audioListenerServerOnly.enabled = IsServer;
    }

    public override void OnNetworkSpawn()
    {
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
        if (IsServer)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= SceneManager_OnLoadEventCompleted;
        }
        gameState.OnValueChanged -= State_OnValueChanged;
        currentAlivePlayerCount.OnValueChanged -= currentAlivePlayerCount_OnValueChanged;
    }

    /// <summary>
    /// Game Scene �ε� �Ϸ�� ����˴ϴ�.
    /// ���� �÷��̾���� ��ȯ�մϴ�.
    /// </summary>
    private void SceneManager_OnLoadEventCompleted(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        Debug.Log("MultiplayerGameManager.SceneManager_OnLoadEventCompleted() Called");

        foreach (PlayerInGameData playerData in CurrentPlayerDataManager.Instance.GetCurrentPlayers())
        {
            if (!playerData.isAI)
            {
                SpawnPlayer(playerData.clientId);
            }
            else
            {
                SpawnAIPlayer(playerData.clientId);
            }
        }
    }

    private void SpawnPlayer(ulong clientId)
    {
        Debug.Log($"Player {clientId} ����");
        Character playerClass = CurrentPlayerDataManager.Instance.GetPlayerDataByClientId(clientId).characterClass;
        GameObject player = Instantiate(GameAssetsManager.Instance.GetCharacterPrefab_MultiPlayerInGame(playerClass));

        if (player == null) return;
        player.transform.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);

        if (spawnPointsController == null) return;
        player.transform.position = spawnPointsController.GetSpawnPoint();
    }

    private void SpawnAIPlayer(ulong aiClientId)
    {
        Debug.Log($"AI Player {aiClientId} ����");

        if (spawnPointsController == null) return;
        Vector3 spawnPoint = spawnPointsController.GetSpawnPoint();

        // ���� ���� �߰��� ���⼭ AI Ŭ���� �о�ͼ� ������ �˻� �� ����ؾ��մϴ�. ������ ������ �ϳ��̱� ������ �̷��� �մϴ�. GameMultiplayer���� ���� ��ӷ��� �ʹ� ����. ����. 
        GameObject aiPlayer = Instantiate(GameAssetsManager.Instance.GetAIPlayerCharacterPrefab(), spawnPoint, Quaternion.identity);
        if (aiPlayer == null) return;

        aiPlayer.transform.TryGetComponent<NetworkObject>(out NetworkObject aiPlayerNetworkObject);
        if (aiPlayerNetworkObject != null)
        {
            aiPlayerNetworkObject.Spawn();
            aiPlayer.transform.TryGetComponent<WizardRukeAIServer>(out WizardRukeAIServer wizardRukeAIServer);
            if (wizardRukeAIServer != null)
            {
                wizardRukeAIServer.InitializeAIPlayerOnServer(aiClientId);
            }
        }

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
        // BGM ����
        //Debug.Log($"GetGamePlayingTimer(): {GetGamePlayingTimer()}, isBGMStarted:{isBGMStarted}");
        if (GetGamePlayingTimer() > 2f && !isBGMStarted)
        {
            isBGMStarted = true;
            SoundManager.Instance?.PlayMusic(LoadSceneManager.Scene.GameScene_MultiPlayer.ToString());
        }

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
                if (countdownToStartTimer.Value < -1f)
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
                    // Finished ������Ʈ�� �Ѱ��ֱ�
                    gameState.Value = GameState.GameFinished;
                }
                // Ÿ�Ӿƿ�.
                else if (gamePlayingTimer.Value < 0f)
                {
                    // ���ھ� ������� GameOveró�� �Ǵ��ϱ�. 
                    DecideWinner();

                    // Finished ������Ʈ�� �Ѱ��ֱ�
                    gameState.Value = GameState.GameFinished;
                }

                break;
            case GameState.GameFinished:

                foreach (PlayerInGameData playerData in CurrentPlayerDataManager.Instance.GetCurrentPlayers())
                {
                    if (playerData.isAI) continue;

                    // Play���� �÷��̾�� ��� �¸� ó�� �մϴ�
                    if (playerData.playerGameState == PlayerGameState.Playing)
                    {
                        PlayerInGameData winPlayer = playerData;

                        if (!NetworkManager.ConnectedClients.ContainsKey(winPlayer.clientId)) continue;
                        // �̹� �� �� ó���� ���� ��ó�� �����ݴϴ� <<<-- �����ʿ�
                        if (winPlayer.playerGameState == PlayerGameState.Win) continue;

                        // ������ State Win ���� ����
                        winPlayer.playerGameState = PlayerGameState.Win;
                        CurrentPlayerDataManager.Instance.SetPlayerDataByClientId(winPlayer.clientId, winPlayer);

                        // ������ ȭ�鿡 Win �˾� ����
                        NetworkClient networkClient = NetworkManager.ConnectedClients[winPlayer.clientId];
                        networkClient.PlayerObject.GetComponent<PlayerClient>().SetPlayerGameWinClientRPC();
                    }
                    // GameOver �÷��̾� ���ӿ��� ����
                    else if (playerData.playerGameState == PlayerGameState.GameOver)
                    {
                        NetworkClient networkClient = NetworkManager.ConnectedClients[playerData.clientId];
                        networkClient?.PlayerObject.GetComponent<PlayerClient>().SetPlayerGameOverClientRPC();
                    }
                }
                break;
            default:
                break;
        }
        //Debug.Log(state);
    }

    /// <summary>
    /// ���ھ� ������� ����ó���� ���ִ� �޼��� �Դϴ�. 
    /// Ÿ�Ӿƿ��� ȣ��Ǿ� �۵��մϴ�.
    /// </summary>
    private void DecideWinner()
    {
        // 1. ���ھ� �ֻ��� �����ϸ� �� ���ӿ��� ó��. 
        // 2. ���� 1���� ���, 2����� ���ӿ��� ó��.

        // ���� �����ڵ� ���ھ �� ����
        List<PlayerInGameData> playersDataList = new List<PlayerInGameData>();

        foreach (PlayerInGameData playerInGameData in CurrentPlayerDataManager.Instance.GetCurrentPlayers())
        {
            playersDataList.Add(playerInGameData);
        }

        // score�� �������� �������� ����
        List<PlayerInGameData> sortedList = playersDataList.OrderByDescending(data => data.score).ToList();

        int topScore = sortedList[0].score;
        foreach (PlayerInGameData playerInGameData in sortedList)
        {
            // �ֻ��� ���ھ��ڿ� ���� 1���� �ƴ� ��� ���� ���ӿ��� ó��.
            if (playerInGameData.score != topScore)
            {
                UpdatePlayerGameOverOnServer(playerInGameData.clientId);
            }
        }
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
            startedPlayerCount.Value = CurrentPlayerDataManager.Instance.GetCurrentPlayerCount();
            UpdateCurrentAlivePlayerCount();
            gameState.Value = GameState.CountdownToStart;
        }
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
            //Debug.Log($"LocalPlayerReady game state:{gameState.Value}");
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
    public void UpdatePlayerGameOverOnServer(ulong clientWhoGameOver, ulong clientWhoAttacked = 100)
    {
        // ������ ����� PlayerDataList���� �÷��̾� ���� ������Ʈ
        PlayerInGameData playerData = CurrentPlayerDataManager.Instance.GetPlayerDataByClientId(clientWhoGameOver);
        if (playerData.playerGameState == PlayerGameState.GameOver)
        {
            Debug.Log($"player{clientWhoGameOver}�� �̹� ���ӿ���ó���� �÷��̾��Դϴ�.");
            return;
        }
        playerData.playerGameState = PlayerGameState.GameOver;
        CurrentPlayerDataManager.Instance.SetPlayerDataByClientId(clientWhoGameOver, playerData);

        // �������� ��� Client�鿡�� ���ӿ��� �ҽ��� ��ε�ĳ��Ʈ���ݴϴ�. AI���� ���ο� Ÿ���� ã�Ƴ������̰�, �÷��̾���� UI�� ������ �����ų���Դϴ�.
        OnPlayerGameOver?.Invoke(this, new PlayerGameOverEventArgs { clientIDWhoGameOver = clientWhoGameOver, clientIDWhoAttacked = clientWhoAttacked });

        // ��� UI�� ���� AlivePlayersCount �� ������Ʈ
        gameOverPlayerCount++;
        UpdateCurrentAlivePlayerCount();
    }

    /// <summary>
    /// ���Ӿ� ����� Ŭ���� �޼���. 
    /// �������� ����մϴ�. 
    /// ���� ������Ʈ�����͵��� �������ݴϴ�
    /// </summary>
    public void CleanUpChildObjects()
    {
        Debug.Log("MultiplayerGameManager CleanUpChildObjects called!");
        // ���� GameObject�� ��� �ڽ� GameObject�� �ı�
        transform.Cast<Transform>().ToList().ForEach(child => Destroy(child.gameObject));
    }

    public void SetGameStateStart()
    {
        gameState.Value = GameState.CountdownToStart;
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
