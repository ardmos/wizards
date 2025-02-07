using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class TimerUIController : NetworkBehaviour
{
    [SerializeField] private bool isTimerOn = false;
    [SerializeField] private TextMeshProUGUI txtTimer;
    private float totalPassedTime;
    int h, m, s;

    // Start is called before the first frame update
    void Start()
    {
        StartTimer();
    }

    // Update is called once per frame
    void Update()
    {
        if (isTimerOn)
        {
            // Singleplay모드 Multiplay모드 구분 처리
            if (IsHost)
            {
                totalPassedTime = SingleplayerGameManager.Instance.GetGamePlayingTimer();
            }
            else
            {
                totalPassedTime = MultiplayerGameManager.Instance.GetGamePlayingTimer();
            }
      
            //h = ((int)totalPassedTime / 3600);
            m = ((int)totalPassedTime / 60 % 60);
            s = ((int)totalPassedTime % 60);
            txtTimer.text = $"{m, 2:D2} : {s, 02:D2}";
        }
    }

    public void StartTimer()
    {
        isTimerOn = true;
    }
    
}
