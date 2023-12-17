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
/// 룰 추가 필요.
/// 생존플레이어 한 명일 때 게임 끝. (테스트중 제외)
/// 아이템 루팅 관리는 다른곳에서?  각자 플레이어 스크립트에서??  서버권한방식으로 바꾸는게 안전할듯!
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

    // Test중. playerPrefab을 캐릭터 클래스에 맞는걸로 검색해다가 넣어주는것 구현중.
    //[SerializeField] private Transform playerPrefab;

    private NetworkVariable<State> state = new NetworkVariable<State>(State.WatingToStart); // 积己苞 悼矫俊 Default蔼 汲沥. 
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
        // Backfill을 지금은 ServerStartup에서 관리하고 있어서 잠시 주석 처리. 추후 필요시 재사용 예정.
        // 辑滚狼 敲饭捞绢 荐侩 惑怕 甫 厚荐侩 惑怕肺 函版
        //await MultiplayService.Instance.UnreadyServerAsync();

        // 咯扁辑档 捞芭 秦拎具 俊矾 救巢
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
    /// 로드 완료시 Player Character 생성
    /// </summary>
    private void SceneManager_OnLoadEventCompleted(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            // plyaerPrefab을 각자 ClientId에 맞는 프리팹으로 검색해서 넣어줘야합니다.            
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
        //얘는 어디서 쓰나???
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }

    // Update is called once per frame
    void Update()
    {
        // SceneManager_OnLoadEventCompleted 이벤트 핸들러 만들어두고, 왜 Update에서 체크하나???  EventHandler 쓰면 안되나?? 확인 요망. 

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
                // 捞 clientId 敲饭捞绢绰 饭叼 救茄 敲饭捞绢涝聪促
                allClientsReady = false;
                break;
            }
        }

        // 葛电 敲饭捞绢啊 饭叼 沁阑 版快. 墨款飘促款 矫累
        if (allClientsReady)
        {
            // 敲饭捞绢 墨款飘 笼拌 诀单捞飘
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
