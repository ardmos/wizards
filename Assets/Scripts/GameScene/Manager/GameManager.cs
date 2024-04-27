using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
#if UNITY_SERVER || UNITY_EDITOR
using Unity.Services.Multiplay;
#endif


/// <summary>
/// Server에서 동작하는 게임 매니저.  
/// Server Auth 방식에서의 게임 매니저는 당연히 서버에서 돌아가야한다. 
/// 혹시 서버Auth방식 아닌 부분들 있으면 정리해야함.
/// 
/// GameScene 게임 흐름 관리
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
        // Game씬 진입!  더이상 플레이어 진입은 필요하지 않습니다. 서버의 플레이어 수용 상태를 비수용 상태로 변경합니다.
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
        gameState.Value = GameState.WatingToStart; // 생성과 동시에 Default값 설정. 
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
    /// Game Scene 로드 완료시 실행됩니다.
    /// 1. Player Character 로드
    /// </summary>
    private void SceneManager_OnLoadEventCompleted(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            // Player Character Client 저장 방식
            //GameObject player = Instantiate(GetCurrentPlayerCharacterPrefab(GameMultiplayer.Instance.GetPlayerDataFromClientId(clientId).playerClass));
            // Player Character Server 저장 방식
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
                if (countdownToStartTimer.Value < 0f)
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
                    gameState.Value = GameState.GameFinished;
                }
                // 타임아웃.
                else if (gamePlayingTimer.Value < 0f)
                {
                    gameState.Value = GameState.GameFinished;
                }

                break;
            case GameState.GameFinished:
                // 최후 생존이 타임아웃보다 우선됨.
                // 최후 생존
                if (currentAlivePlayerCount.Value == 1)
                {
                    // 최후 생존으로 끝난 경우. 생존자 Win 처리    <<<<<<<<----------------- 여기부터!
                    // 생존자 PlayerData
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

                    // 생존자 State Win 으로 변경
                    winPlayer.playerGameState = PlayerGameState.Win;
                    GameMultiplayer.Instance.SetPlayerDataFromClientId(winPlayer.clientId, winPlayer);

                    // 생존자 화면에 Win 팝업 실행
                    NetworkClient networkClient = NetworkManager.ConnectedClients[winPlayer.clientId];
                    networkClient.PlayerObject.GetComponent<PlayerClient>().SetPlayerGameWinClientRPC();
                }
                // 타임아웃.
                else if (gamePlayingTimer.Value < 0f)
                {
                    // 타임아웃으로 끝난 경우. 생존자들 Draw. 처리
                    // 생존자들 State Draw로 변경

                    // 생존자들 화면에 Draw 팝업 실행

                }

                break;
            default:
                break;
        }
        //Debug.Log(state);
    }

    // 레디상태 서버에 보고
    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerReadyServerRpc(ServerRpcParams serverRpcParams = default)
    {
        playerReadyList[serverRpcParams.Receive.SenderClientId] = true;

        bool allClientsReady = true;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!playerReadyList.ContainsKey(clientId) || !playerReadyList[clientId])
            {
                // 이 clientId 플레이어는 레디 안한 플레이어입니다
                allClientsReady = false;
                break;
            }
        }

        // 모든 플레이어가 레디 했을 경우. 카운트다운 시작
        if (allClientsReady)
        {
            // 플레이어 카운트 집계 업데이트(이 순간 접속중인 인원.)
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
            Debug.Log($"LocalPlayerReady game state:{gameState.Value}");
            isLocalPlayerReady = true;
            SetPlayerReadyServerRpc();
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
    public void UpdatePlayerGameOverOnServer(ulong clientId)
    {
        //Debug.Log($"UpdatePlayerGameOverOnServer. gameOver player : player{clientId}");

        if (NetworkManager.ConnectedClients.ContainsKey(clientId))
        {
            // 서버에 저장된 PlayerDataList상의 플레이어 상태 업데이트
            PlayerInGameData playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(clientId);

            if (playerData.playerGameState == PlayerGameState.GameOver)
            {
                Debug.Log($"player{clientId}는 이미 게임오버처리된 플레이어입니다.");
                return;
            }

            playerData.playerGameState = PlayerGameState.GameOver;
            Debug.Log($"UpdatePlayerGameOverOnServer. player.clientId:{clientId}. playerGameState:{playerData.playerGameState}");
            GameMultiplayer.Instance.SetPlayerDataFromClientId(clientId, playerData);

            // 접속중인 모든 Client들의 NotifyUI에 현재 게임오버 된 플레이어의 닉네임을 브로드캐스트해줍니다.(게임오버시킨사람 닉네임 공유까지는 아직 미구현)
            GameSceneUIManager.Instance.notifyUIController.ShowGameOverPlayerClientRPC(playerData.playerName.ToString());

            // 상단 UI를 위한 AlivePlayersCount 값 업데이트
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
