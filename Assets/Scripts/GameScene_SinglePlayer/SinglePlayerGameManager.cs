using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 1. 플레이어 스폰
/// 2. AI 플레이어 스폰
/// 3. gameState 관리
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

    public List<Transform> spawnPoints; // Inspector에서 6개의 스폰 포인트를 할당합니다.
    public List<Transform> selectedSpawnPoints; // 선택된 4개의 스폰 포인트를 저장할 리스트


    private void Awake()
    {
        Debug.Log("SingleplayerGameManager.Awake()");
        Instance = this;
        GameSingleplayer.Instance.StartHost();
    }

    public override void OnNetworkSpawn()
    {
        gameState.OnValueChanged += State_OnValueChanged;
        gameState.Value = GameState.WatingToStart; // 생성과 동시에 Default값 설정. 
        currentAlivePlayerCount.OnValueChanged += currentAlivePlayerCount_OnValueChanged;
    }

    public override void OnNetworkDespawn()
    {
        gameState.OnValueChanged -= State_OnValueChanged;
        currentAlivePlayerCount.OnValueChanged -= currentAlivePlayerCount_OnValueChanged;
    }

    private void State_OnValueChanged(GameState previousValue, GameState newValue)
    {
        //Debug.Log($"GameState가 변경되었습니다! previousValue:{previousValue}, newValue{newValue}");
        OnGameStateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void currentAlivePlayerCount_OnValueChanged(int previousValue, int newValue)
    {
        OnAlivePlayerCountChanged?.Invoke(this, EventArgs.Empty);
    }

    // Update is called once per frame
    void Update()
    {
        // BGM 실행
        //Debug.Log($"GetGamePlayingTimer(): {GetGamePlayingTimer()}, isBGMStarted:{isBGMStarted}");
        if (GetGamePlayingTimer() > 2f && !isBGMStarted)
        {
            isBGMStarted = true;
            SoundManager.Instance?.PlayMusic(LoadSceneManager.Scene.GameScene_MultiPlayer.ToString());
        }

        // Update 대신에 State 바뀔때마다 호출되는 Eventhandler 사용하기  <--- 여기부터. 시간 계산기랑 EventListener랑 분리해서 구현하면 됨.
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

                // 최후 생존이 타임아웃보다 우선됨.
                // 최후 생존
                if (currentAlivePlayerCount.Value == 1)
                {
                    // Finished 스테이트로 넘겨주기
                    gameState.Value = GameState.GameFinished;
                }
                // 타임아웃.
                else if (gamePlayingTimer.Value < 0f)
                {
                    // 스코어 기반으로 GameOver처리 판단하기. 
                    DecideWinner();

                    // Finished 스테이트로 넘겨주기
                    gameState.Value = GameState.GameFinished;
                }

                break;
            case GameState.GameFinished:

                foreach (PlayerInGameData playerData in GameSingleplayer.Instance.GetPlayerDataNetworkList())
                {
                    if (playerData.isAI) continue;

                    // Play중인 플레이어는 모두 승리 처리 합니다
                    if (playerData.playerGameState == PlayerGameState.Playing)
                    {
                        PlayerInGameData winPlayer = playerData;

                        if (!NetworkManager.ConnectedClients.ContainsKey(winPlayer.clientId)) continue;
                        // 이미 한 번 처리된 경우는 재처리 안해줍니다 <<<-- 수정필요
                        if (winPlayer.playerGameState == PlayerGameState.Win) continue;

                        // 생존자 State Win 으로 변경
                        winPlayer.playerGameState = PlayerGameState.Win;
                        GameSingleplayer.Instance.SetPlayerDataFromClientId(winPlayer.clientId, winPlayer);

                        // 생존자 화면에 Win 팝업 실행
                        NetworkClient networkClient = NetworkManager.ConnectedClients[winPlayer.clientId];
                        networkClient.PlayerObject.GetComponent<PlayerClient>().SetPlayerGameWinClientRPC();
                    }
                    // GameOver 플레이어 게임오버 공지
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
    /// 스코어 기반으로 승패처리를 해주는 메서드 입니다. 
    /// 타임아웃시 호출되어 작동합니다.
    /// </summary>
    private void DecideWinner()
    {
        // 1. 스코어 최상위 제외하면 다 게임오버 처리. 
        // 2. 공동 1등인 경우, 2등부터 게임오버 처리.

        // 게임 참가자들 스코어를 비교 정렬
        List<PlayerInGameData> playersDataList = new List<PlayerInGameData>();

        foreach (PlayerInGameData playerInGameData in GameSingleplayer.Instance.GetPlayerDataNetworkList())
        {
            playersDataList.Add(playerInGameData);
        }

        // score를 기준으로 내림차순 정렬
        List<PlayerInGameData> sortedList = playersDataList.OrderByDescending(data => data.score).ToList();

        int topScore = sortedList[0].score;
        foreach (PlayerInGameData playerInGameData in sortedList)
        {
            // 최상위 스코어자와 공동 1등이 아닌 경우 전부 게임오버 처리.
            if (playerInGameData.score != topScore)
            {
                UpdatePlayerGameOverOnServer(playerInGameData.clientId);
            }
        }
    }

    /// <summary>
    /// 생존상태인 플레이어 숫자 카운트 업데이트
    /// </summary>
    public void UpdateCurrentAlivePlayerCount()
    {
        // 게임중인 플레이어 숫자(게임오버 안당하고)
        currentAlivePlayerCount.Value = startedPlayerCount.Value - gameOverPlayerCount;
        //Debug.Log($"UpdateCurrentAlivePlayerCount playerCount:{currentAlivePlayerCount.Value}");
    }

    // 클라이언트에서 호출하는 메소드
    // 게임 시작시 보이는 Ready UI 버튼을 클릭했을 때 동작하는 메서드 입니다.
    public void LocalPlayerReadyOnClient()
    {
        if (gameState.Value == GameState.WatingToStart)
        {
            // 플레이어 카운트 집계 업데이트(이 순간 접속중인 인원.)
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
    /// 서버에서 동작하는 메소드입니다. 게임 오버 상태를 기록합니다.
    /// // Client 게임오버시
    /// 1. PlayerDataNetworkList상의 해당 ClientId의 PlayerGameState 값을 GameOver로 변경합니다.
    /// 2. 현재 플레이중인 플레이어숫자를 보여주는 상단 UI를 위한 AlivePlayersCount 값을 업데이트 합니다.
    /// 3. NotifyUI에 현재 게임오버 된 플레이어와 게임오버 시킨 플레이어의 닉네임을 공유해줍니다. (게임오버 시킨 플레이어의 닉네임 공유는 아직 미구현입니다.)
    /// </summary>
    /// <param name="serverRpcParams"></param>
    public void UpdatePlayerGameOverOnServer(ulong clientWhoGameOver, ulong clientWhoAttacked = 100)
    {
        // 서버에 저장된 PlayerDataList상의 플레이어 상태 업데이트
        PlayerInGameData playerData = GameSingleplayer.Instance.GetPlayerDataFromClientId(clientWhoGameOver);
        if (playerData.playerGameState == PlayerGameState.GameOver)
        {
            Debug.Log($"player{clientWhoGameOver}는 이미 게임오버처리된 플레이어입니다.");
            return;
        }
        playerData.playerGameState = PlayerGameState.GameOver;
        GameSingleplayer.Instance.SetPlayerDataFromClientId(clientWhoGameOver, playerData);

        // 접속중인 모든 Client들에게 게임오버 소식을 브로드캐스트해줍니다. AI들은 새로운 타겟을 찾아나설것이고, 플레이어들은 UI에 정보를 노출시킬것입니다.
        OnPlayerGameOver?.Invoke(this, new PlayerGameOverEventArgs { clientIDWhoGameOver = clientWhoGameOver, clientIDWhoAttacked = clientWhoAttacked });

        // 상단 UI를 위한 AlivePlayersCount 값 업데이트
        gameOverPlayerCount++;
        UpdateCurrentAlivePlayerCount();
    }

    /// <summary>
    /// 게임씬 종료용 클린업 메서드. 
    /// 서버에서 사용합니다. 
    /// 마법 오브젝트같은것들을 정리해줍니다
    /// </summary>
    public void CleanUpObjects()
    {
        Debug.Log("MultiplayerGameManager CleanUpObjects called!");
        // 현재 GameObject의 모든 자식 GameObject를 파괴
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
        // 게임 바로 시작!
        SetGameStateStart();
    }

    private void SpawnPlayer()
    {
        // Player 스폰
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            Debug.Log($"Player {clientId} 스폰");

            Character playerClass = GameSingleplayer.Instance.GetPlayerDataFromClientId(clientId).characterClass;
            PlayerInGameData playerInGameData = PlayerDataManager.Instance.GetPlayerInGameData();
            // 플레이어 추가
            GameSingleplayer.Instance.AddPlayer(new PlayerInGameData
            {
                clientId = clientId,
                // 접속시간 기록
                connectionTime = DateTime.Now,
                characterClass = playerInGameData.characterClass,
                playerGameState = PlayerGameState.Playing,
                playerName = playerInGameData.playerName,
                isAI = false
                // HP는 게임 시작되면 OnNetworkSpawn때 각자가 SetPlayerHP로 보고함.
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
        // 스폰 포인트 리스트를 복사합니다.
        List<Transform> tempList = new List<Transform>(spawnPointsController.spawnPoints);

        // Fisher-Yates 셔플 알고리즘을 사용하여 리스트를 섞습니다.
        for (int i = tempList.Count - 1; i > 0; i--)
        {
            int randomIndex = UnityEngine.Random.Range(0, i + 1);
            Transform temp = tempList[i];
            tempList[i] = tempList[randomIndex];
            tempList[randomIndex] = temp;
        }

        // 섞인 리스트에서 처음 4개의 요소를 선택합니다.
        selectedSpawnPoints = tempList.GetRange(0, 4);
    }
}
