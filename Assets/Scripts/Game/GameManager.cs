using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
#if UNITY_SERVER || UNITY_EDITOR
using Unity.Services.Multiplay;
#endif


/// <summary>
/// GameScene 게임 흐름 관리
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
    public event EventHandler OnGameOverListChanged;

    public enum State
    {
        WatingToStart,
        CountdownToStart,
        GamePlaying,
        GameOver,
    }

    [SerializeField] private NetworkList<ulong> playerGameOverList;
    [SerializeField] private NetworkVariable<State> state = new NetworkVariable<State>(State.WatingToStart);
    [SerializeField] private NetworkVariable<int> startedPlayerCount = new NetworkVariable<int>(0);
    [SerializeField] private NetworkVariable<int> currentAlivePlayerCount = new NetworkVariable<int>(0);
    [SerializeField] private NetworkVariable<float> countdownToStartTimer = new NetworkVariable<float>(3f);
    [SerializeField] private NetworkVariable<float> gamePlayingTimer = new NetworkVariable<float>(0f);

    [SerializeField] private float gamePlayingTimerMax = 1000f;
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
        // 서버의 플레이어 수용 상태 를 비수용 상태로 변경
        //await MultiplayService.Instance.UnreadyServerAsync();

        // 여기서도 이거 해줘야 에러 안남
        //Camera.main.enabled = false;
#endif
    }

    public override void OnNetworkSpawn()
    {
        state.OnValueChanged += State_OnValueChanged;
        state.Value = State.WatingToStart; // 생성과 동시에 Default값 설정. 
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


    private void State_OnValueChanged(State previousValue, State newValue)
    {
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void currentAlivePlayerCount_OnValueChanged(int previousValue, int newValue)
    {
        OnAlivePlayerCountChanged?.Invoke(this, EventArgs.Empty);
    }

    // Update is called once per frame
    void Update()
    {
        // Update 대신에 State 바뀔때마다 호출되는 Eventhandler 사용하면 될것같은데! 추후 검토
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

    // 레디상태 서버에 보고
    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerReadyServerRpc(ServerRpcParams serverRpcParams = default)
    {
        playerReadyList[serverRpcParams.Receive.SenderClientId] = true;

        bool allClientsReady = true;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if(!playerReadyList.ContainsKey(clientId) || !playerReadyList[clientId])
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
            state.Value = State.CountdownToStart;
            //Debug.Log($"allClientsReady state.Value:{state.Value}");
        }

        //Debug.Log($"SetPlayerReadyServerRpc game state:{state.Value}, allClientsReady: {allClientsReady}");
    }

    public void UpdateCurrentAlivePlayerCount()
    {
        // 게임중인 플레이어 숫자(게임오버 안당하고)
        currentAlivePlayerCount.Value = startedPlayerCount.Value - playerGameOverList.Count;        
        Debug.Log($"UpdateCurrentAlivePlayerCount playerCount:{currentAlivePlayerCount.Value}");
    }

    // 게임 시작시 보이는 Ready UI 버튼을 클릭했을 때 동작하는 메서드 입니다.
    public void LocalPlayerReady()
    {
        if (state.Value == State.WatingToStart)
        {
            //Debug.Log($"LocalPlayerReady game state:{state.Value}");
            isLocalPlayerReady = true;
            SetPlayerReadyServerRpc();
        }
    }

    /// <summary>
    /// 서버에서 동작하는 메소드입니다. 서버에 저장되는 게임오버자 명단을 업데이트 합니다.
    /// // Client 게임오버시
    /// 1. 서버에게 보고.  서버는 딕셔너리에 ClientID 게임오버 true로 기록.
    /// 2. 클라이언트쪽 딕셔너리 리스트에도 해당 내용 공유.
    /// 3. 해당 클라이언트 GameOverUIPopup은 클라이언트쪽 딕셔너리 리스트 OnValueChanged 이벤트핸들러를 통해 활성화됨.
    /// 4. 서버측 게임오버 False인 인원 1명일 경우 승리.
    /// (이렇게 하면 관전기능 추가 가능. 아직 미구현. 여기서 추가하면 됨.)
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
        // GameOver 플레이어 리스트 업데이트  (게임오버시킨사람 닉네임 공유는 아직 미구현)
        playerGameOverList.Add(clientId);
        // 상단 UI를 위한 AlivePlayersCount 값 업데이트
        UpdateCurrentAlivePlayerCount();
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
