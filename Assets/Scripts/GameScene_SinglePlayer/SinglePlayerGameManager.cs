using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 1. �÷��̾� ����
/// 2. AI �÷��̾� ����
/// 3. gameState ����
/// </summary>

public class SingleplayerGameManager : NetworkBehaviour
{
    public static SingleplayerGameManager Instance { get; private set; }

    public event EventHandler OnGameStateChanged;
    public event EventHandler OnAlivePlayerCountChanged;
    public event EventHandler<PlayerGameOverEventArgs> OnPlayerGameOver;

    public PlayerSpawnPointsController spawnPointsController;
    public PopupGameStartCountdownUIController popupGameStartCountdownUIController;

    [SerializeField] private NetworkVariable<GameState> gameState = new NetworkVariable<GameState>(GameState.WatingToStart);
    [SerializeField] private NetworkVariable<int> startedPlayerCount = new NetworkVariable<int>(0);
    [SerializeField] private NetworkVariable<int> currentAlivePlayerCount = new NetworkVariable<int>(0);
    [SerializeField] private NetworkVariable<float> countdownToStartTimer = new NetworkVariable<float>(3f);
    [SerializeField] private NetworkVariable<float> gamePlayingTimer = new NetworkVariable<float>(0f);

    [SerializeField] private ushort gameOverPlayerCount = 0;
    [SerializeField] private float gamePlayingTimerMax;
    [SerializeField] private bool isLocalPlayerReady;
    [SerializeField] private bool isBGMStarted;

    public List<Transform> spawnPoints; // Inspector���� 6���� ���� ����Ʈ�� �Ҵ��մϴ�.
    public List<Transform> selectedSpawnPoints; // ���õ� 4���� ���� ����Ʈ�� ������ ����Ʈ


    private void Awake()
    {
        Debug.Log("SingleplayerGameManager.Awake()");
        Instance = this;
        GameSingleplayer.Instance.StartHost();
    }

    public override void OnNetworkSpawn()
    {
        gameState.OnValueChanged += State_OnValueChanged;
        gameState.Value = GameState.WatingToStart; // ������ ���ÿ� Default�� ����. 
        currentAlivePlayerCount.OnValueChanged += currentAlivePlayerCount_OnValueChanged;
    }

    public override void OnNetworkDespawn()
    {
        gameState.OnValueChanged -= State_OnValueChanged;
        currentAlivePlayerCount.OnValueChanged -= currentAlivePlayerCount_OnValueChanged;
    }

    private void State_OnValueChanged(GameState previousValue, GameState newValue)
    {
        //Debug.Log($"GameState�� ����Ǿ����ϴ�! previousValue:{previousValue}, newValue{newValue}");
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

                foreach (PlayerInGameData playerData in GameSingleplayer.Instance.GetPlayerDataNetworkList())
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
                        GameSingleplayer.Instance.SetPlayerDataFromClientId(winPlayer.clientId, winPlayer);

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

        foreach (PlayerInGameData playerInGameData in GameSingleplayer.Instance.GetPlayerDataNetworkList())
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
            // �÷��̾� ī��Ʈ ���� ������Ʈ(�� ���� �������� �ο�.)
            //startedPlayerCount.Value = NetworkManager.ConnectedClients.Count;
            startedPlayerCount.Value = GameSingleplayer.Instance.GetPlayerDataNetworkList().Count;
            UpdateCurrentAlivePlayerCount();
            //Debug.Log($"allClientsReady gameState.Value:{gameState.Value}");
            gameState.Value = GameState.CountdownToStart;
            //Debug.Log($"allClientsReady gameState.Value:{gameState.Value}");
            popupGameStartCountdownUIController.OpenPopup();
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
        PlayerInGameData playerData = GameSingleplayer.Instance.GetPlayerDataFromClientId(clientWhoGameOver);
        if (playerData.playerGameState == PlayerGameState.GameOver)
        {
            Debug.Log($"player{clientWhoGameOver}�� �̹� ���ӿ���ó���� �÷��̾��Դϴ�.");
            return;
        }
        playerData.playerGameState = PlayerGameState.GameOver;
        GameSingleplayer.Instance.SetPlayerDataFromClientId(clientWhoGameOver, playerData);

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
    public void CleanUpObjects()
    {
        Debug.Log("MultiplayerGameManager CleanUpObjects called!");
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










    public void StartSinglePlayerGameManager()
    {
        SelectRandomSpawnPoints();

        SpawnPlayer();
        // ���� �ٷ� ����!
        SetGameStateStart();
    }

    private void SpawnPlayer()
    {
        // Player ����
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            Debug.Log($"Player {clientId} ����");

            Character playerClass = GameSingleplayer.Instance.GetPlayerDataFromClientId(clientId).characterClass;
            PlayerInGameData playerInGameData = PlayerDataManager.Instance.GetPlayerInGameData();
            // �÷��̾� �߰�
            GameSingleplayer.Instance.AddPlayer(new PlayerInGameData
            {
                clientId = clientId,
                // ���ӽð� ���
                connectionTime = DateTime.Now,
                characterClass = playerInGameData.characterClass,
                playerGameState = PlayerGameState.Playing,
                playerName = playerInGameData.playerName,
                isAI = false
                // HP�� ���� ���۵Ǹ� OnNetworkSpawn�� ���ڰ� SetPlayerHP�� ������.
            });

            GameObject player = Instantiate(GameAssetsManager.Instance.GetCharacterPrefab_SinglePlayerInGame(playerClass));
            if (player != null)
                player.transform.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
            else
                Debug.Log($"SceneManager_OnLoadEventCompleted : player prefab load failed. prefab is null");
        }
    }

    private void SelectRandomSpawnPoints()
    {
        // ���� ����Ʈ ����Ʈ�� �����մϴ�.
        List<Transform> tempList = new List<Transform>(spawnPointsController.spawnPoints);

        // Fisher-Yates ���� �˰����� ����Ͽ� ����Ʈ�� �����ϴ�.
        for (int i = tempList.Count - 1; i > 0; i--)
        {
            int randomIndex = UnityEngine.Random.Range(0, i + 1);
            Transform temp = tempList[i];
            tempList[i] = tempList[randomIndex];
            tempList[randomIndex] = temp;
        }

        // ���� ����Ʈ���� ó�� 4���� ��Ҹ� �����մϴ�.
        selectedSpawnPoints = tempList.GetRange(0, 4);
    }
}
