using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Singleton
/// EventSystem(Observer Pattern)
/// Game State Machine
/// ownerPlayerObject
/// </summary>

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    public event EventHandler OnStateChanged;

    private enum State
    {
        WatingToStart,
        CountdownToStart,
        GamePlaying,
        GameOver,
    }

    [SerializeField] private Transform playerPrefab;

    private NetworkVariable<State> state = new NetworkVariable<State>(State.WatingToStart); // 생성과 동시에 Default값 설정. 
    private bool isLocalPlayerReady;
    private NetworkVariable<float> countdownToStartTimer = new NetworkVariable<float>(3f);
    private NetworkVariable<float> gamePlayingTimer = new NetworkVariable<float>(0f);
    private float gamePlayingTimerMax = 10f;
    private Dictionary<ulong, bool> playerReadyDictionary;

    [SerializeField] private int currentAlivePlayerCount; 

    public GameObject ownerPlayerObject { get; set; }

    void Awake()
    {
        Instance = this;
        playerReadyDictionary = new Dictionary<ulong, bool>();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    public override void OnNetworkSpawn()
    {
        state.OnValueChanged += State_OnValueChanged;

        if (IsServer)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
        }
    }

    private void SceneManager_OnLoadEventCompleted(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            Transform playerTransform = Instantiate(playerPrefab);
            playerTransform.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
        }
    }

    private void State_OnValueChanged(State previousValue, State newValue)
    {
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }

    // Update is called once per frame
    void Update()
    {
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
        Debug.Log(state);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerReadyServerRpc(ServerRpcParams serverRpcParams = default)
    {
        playerReadyDictionary[serverRpcParams.Receive.SenderClientId] = true;

        bool allClientsReady = true;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if(!playerReadyDictionary.ContainsKey(clientId) || !playerReadyDictionary[clientId])
            {
                // 이 clientId 플레이어는 레디 안한 플레이어입니다
                allClientsReady = false;
                break;
            }
        }

        // 모든 플레이어가 레디 했을 경우. 카운트다운 시작
        if (allClientsReady)
        {
            state.Value = State.CountdownToStart;
        }
    }

    public void LocalPlayerReady()
    {
        if (state.Value == State.WatingToStart)
        {
            isLocalPlayerReady = true;
            SetPlayerReadyServerRpc();
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
