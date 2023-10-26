using System.Collections;
using System.Collections.Generic;
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

    private enum State
    {
        WatingToStart,
        CountdownToStart,
        GamePlaying,
        GameOver,
    }

    private State state;
    private float watingToStartTimer = 1f;
    private float countdownToStartTimer = 3f;
    private float gamePlayingTimer = 1f;

    public GameObject ownerPlayerObject { get; set; }

    void Awake()
    {
        Instance = this;
        state = State.WatingToStart;
    }

    // Start is called before the first frame update
    void Start()
    {
        
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
                watingToStartTimer -= Time.deltaTime;
                if (watingToStartTimer < 0f)
                {
                    state = State.CountdownToStart;
                }
                break;
            case State.CountdownToStart:
                countdownToStartTimer -= Time.deltaTime;
                if (countdownToStartTimer < 0f)
                {
                    state = State.GamePlaying;
                }
                break;
            case State.GamePlaying:
                gamePlayingTimer -= Time.deltaTime;
                if (gamePlayingTimer < 0f)
                {
                    state = State.GameOver;
                }
                break;
            case State.GameOver:
                break;
            default:
                break;
        }
        Debug.Log(state);
    }

}
