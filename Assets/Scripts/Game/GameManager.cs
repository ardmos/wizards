using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Singleton
/// EventSystem(Observer Pattern)
/// Game State Machine
/// ownerPlayerObject
/// </summary>

public class GameManager : MonoBehaviour
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

    private State state;
    private bool isLocalPlayerReady;
    private float countdownToStartTimer = 3f;
    private float gamePlayingTimer = 0;
    private float gamePlayingTimerMax = 300f;

    [SerializeField] private int currentAlivePlayerCount; 

    public GameObject ownerPlayerObject { get; set; }

    void Awake()
    {
        Instance = this;
        state = State.WatingToStart;
    }

    // Start is called before the first frame update
    void Start()
    {
        // WatingToStart Ÿ�̸� ���� �κ� ��������. Ÿ�̸� ���, �÷��̾�� ���� ���η� �����Ϸ��� ��. 
        // ���� GameState ��ġ��Ű�� �����ؼ� �����ϸ� ��. ������� ����. 
        // ���ķ� ���� ��ũ ���߱� ���� �ϸ� �� . ������ SetlectHostClientForTest.cs ���� �ϼ���. ���� �� ��ư ������ ĳ���� ���������� ��.
        // ĳ���� �������� ���� ������Ʈ �������� ���� �ȵ�.
        //GameInput.Insatan
    }

    // Update is called once per frame
    void Update()
    {
        RunStateMachine();
    }

    private void RunStateMachine()
    {
        switch (state)
        {
            case State.WatingToStart:
                break;
            case State.CountdownToStart:
                countdownToStartTimer -= Time.deltaTime;
                if (countdownToStartTimer < 0f)
                {
                    state = State.GamePlaying;
                    gamePlayingTimer = gamePlayingTimerMax;
                    OnStateChanged?.Invoke(this, EventArgs.Empty);
                }
                break;
            case State.GamePlaying:
                gamePlayingTimer -= Time.deltaTime;
                if (gamePlayingTimer < 0f)
                {
                    state = State.GameOver;
                    OnStateChanged?.Invoke(this, EventArgs.Empty);
                }
                break;
            case State.GameOver:
                break;
            default:
                break;
        }
        Debug.Log(state);
    }

    // test code
    public void StartReady()
    {
        state = State.WatingToStart;
        OnStateChanged?.Invoke(this, EventArgs.Empty); 
    }

    public bool IsWatingToStart()
    {
        return state == State.WatingToStart;
    }

    public bool IsGamePlaying()
    {
        return state == State.GamePlaying;
    }

    public bool IsCountdownToStartActive()
    {
        return state == State.CountdownToStart;
    }
    public bool IsGameOver()
    {
        return state == State.GameOver;
    }

    public float GetCountdownToStartTimer()
    {
        return countdownToStartTimer;
    }

    public int GetCurrentAlivePlayerCount()
    {
        return currentAlivePlayerCount;
    }

    public float GetGamePlayingTimer()
    {
        if(gamePlayingTimer == 0f) return 0f;

        return gamePlayingTimerMax - gamePlayingTimer;
    }
}
