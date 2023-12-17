using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
#if UNITY_SERVER || UNITY_EDITOR
using Unity.Services.Multiplay;
#endif


/// <summary>
/// Singleton
/// EventSystem(Observer Pattern)
/// Game State Machine
/// ownerPlayerObject
///
/// �0�7 �4�1�7�5 �6�9�3�9.
/// �1�6�3�9�5�3�0�5�3�3�2�5 �6�3 �0�2�3�1 �9�5 �7�5�3�9 �8�2. (�5�3�2�1�5�7�3�6 �3�1�3�1)
/// �2�3�3�3�5�9 �0�9�5�4 �7�1�0�5�8�9 �9�9�0�3�7�8�2�3�1�9?  �7�6�3�1 �5�3�0�5�3�3�2�5 �2�1�5�9�0�2�5�7�2�3�1�9??  �1�9�0�3�7�1�6�3�0�2�2�8�3�7�0�9 �0�1�8�7�8�9�7�5 �2�7�3�7�6�7�9�2!
/// 
/// </summary>

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    public event EventHandler OnStateChanged;
    public event EventHandler OnAlivePlayerCountChanged;

    private enum State
    {
        WatingToStart,
        CountdownToStart,
        GamePlaying,
        GameOver,
    }

    // Test�3�6. playerPrefab�3�5 �4�3�0�6�5�5 �5�7�9�3�2�1�2�3 �0�5�8�9�7�5�0�9 �7�3�1�6�6�7�9�9�7�5 �8�0�2�5�3�5�8�9�7�6 �7�9�6�1�3�6.
    //[SerializeField] private Transform playerPrefab;

    private NetworkVariable<State> state = new NetworkVariable<State>(State.WatingToStart); // ������ ���ÿ� Default�� ����. 
    private bool isLocalPlayerReady;
    private NetworkVariable<float> countdownToStartTimer = new NetworkVariable<float>(3f);
    private NetworkVariable<float> gamePlayingTimer = new NetworkVariable<float>(0f);
    private float gamePlayingTimerMax = 10f;
    private Dictionary<ulong, bool> playerReadyDictionary;

    [SerializeField] private int currentAlivePlayerCount;     

    void Awake()
    {
        Instance = this;
        playerReadyDictionary = new Dictionary<ulong, bool>();
    }

    // Start is called before the first frame update
    async void Start()
    {
#if DEDICATED_SERVER
        // Backfill�3�5 �3�1�8�5�3�1 ServerStartup�2�3�1�9 �7�1�0�5�6�9�7�9 �3�3�2�5�1�9 �3�7�2�7 �3�5�1�0 �4�9�0�5. �4�1�6�3 �6�9�3�9�2�7 �3�9�1�7�3�0 �2�9�3�4.
        // ������ �÷��̾� ���� ���� �� ����� ���·� ����
        //await MultiplayService.Instance.UnreadyServerAsync();

        // ���⼭�� �̰� ����� ���� �ȳ�
        //Camera.main.enabled = false;
#endif
    }

    public override void OnNetworkSpawn()
    {
        state.OnValueChanged += State_OnValueChanged;

        if (IsServer)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
        }
    }

    /// <summary>
    /// �0�9�9�3 �2�9�0�1�2�7 Player Character �1�6�1�0
    /// </summary>
    private void SceneManager_OnLoadEventCompleted(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            // plyaerPrefab�3�5 �7�6�3�1 ClientId�2�3 �0�5�8�9 �5�5�0�5�5�6�3�7�0�9 �7�3�1�6�6�7�1�9 �8�0�2�5�3�3�2�9�6�6�9�1�9�9.            
            //Transform playerTransform = Instantiate(playerPrefab);
            GameObject player = Instantiate(GameMultiplayer.Instance.GetPlayerClassPrefabByPlayerIndex_ForGameSceneObject(
                GameMultiplayer.Instance.GetPlayerDataIndexFromClientId(clientId)));
            if (player != null)
                player.transform.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
            else
                Debug.Log($"SceneManager_OnLoadEventCompleted : player prefab load failed. prefab is null");
        }
    }

    private void State_OnValueChanged(State previousValue, State newValue)
    {
        //�2�7�8�9 �2�5�9�9�1�9 �2�9�8�1???
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }

    // Update is called once per frame
    void Update()
    {
        // SceneManager_OnLoadEventCompleted �3�3�1�5�5�7 �6�1�9�1�0�7 �0�7�9�1�2�5�9�3�7�9, �2�3 Update�2�3�1�9 �4�7�5�9�6�9�8�1???  EventHandler �2�9�0�5 �2�7�9�7�8�1?? �6�2�3�7 �3�9�0�4. 

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

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerReadyServerRpc(ServerRpcParams serverRpcParams = default)
    {
        playerReadyDictionary[serverRpcParams.Receive.SenderClientId] = true;

        bool allClientsReady = true;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if(!playerReadyDictionary.ContainsKey(clientId) || !playerReadyDictionary[clientId])
            {
                // �� clientId �÷��̾�� ���� ���� �÷��̾��Դϴ�
                allClientsReady = false;
                break;
            }
        }

        // ��� �÷��̾ ���� ���� ���. ī��Ʈ�ٿ� ����
        if (allClientsReady)
        {
            // �÷��̾� ī��Ʈ ���� ������Ʈ
            UpdateCurrentAlivePlayerCount();
            state.Value = State.CountdownToStart;
        }
    }

    public void UpdateCurrentAlivePlayerCount()
    {
        OnAlivePlayerCountChanged?.Invoke(this, EventArgs.Empty);
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
