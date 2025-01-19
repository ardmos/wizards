using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PopupGameStartCountdownUIController : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI txtCountdown;

    private double tmpCountdownValue = 0;
    private bool[] isAnnounced = new bool[4] { false, false, false, false };

    private void Start()
    {
        //Debug.Log($"IsHost:{IsHost}");      
        // Singleplay모드 Multiplay모드 구분 처리
        /*        if (IsHost)
                {
                    SingleplayerGameManager.Instance.OnGameStateChanged += OnGameStateChanged;
                }
                else
                {
                    MultiplayerGameManager.Instance.OnGameStateChanged += OnGameStateChanged;
                }*/

        MultiplayerGameManager.Instance.OnGameStateChanged += OnGameStateChanged;

        Hide();
    }

    public override void OnDestroy()
    {
        // Singleplay모드 Multiplay모드 구분 처리
        /*        if (IsHost)
                {
                    SingleplayerGameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
                }
                else
                {
                    MultiplayerGameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
                }    */
        MultiplayerGameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
    }

    private void FixedUpdate()
    {
        double countdownTime = 0;
        /*        if (IsHost)
                {
                    countdownTime = Math.Ceiling(SingleplayerGameManager.Instance.GetCountdownToStartTimer());
                }
                else
                {
                    countdownTime = Math.Ceiling(MultiplayerGameManager.Instance.GetCountdownToStartTimer());
                }*/
        countdownTime = Math.Ceiling(MultiplayerGameManager.Instance.GetCountdownToStartTimer());

        if (tmpCountdownValue == countdownTime) return;
        switch (countdownTime)
        {
            case 0:
            case 1:
            case 2:
            case 3:
                tmpCountdownValue = countdownTime;
                if (!isAnnounced[(int)countdownTime])
                SoundManager.Instance?.PlayCountdownAnnouncer(countdownTime);
                isAnnounced[(int)countdownTime] = true;
                break;
            default:
                break;
        }

        Debug.Log($"countdownTime:{countdownTime}");
        txtCountdown.text = countdownTime.ToString();
    }

    private void OnGameStateChanged(object sender, EventArgs e)
    {
        /*        // Singleplay모드 Multiplay모드 구분 처리
                if (IsHost)
                {
                    //Debug.Log($"PopupGameStartCountdownUIController, IsCountdownToStartActive:{SingleplayerGameManager.Instance.IsCountdownToStartActive()}");
                    if (SingleplayerGameManager.Instance.IsCountdownToStartActive())
                    {
                        Show();
                    }
                    else
                    {
                        Hide();
                    }
                    //Debug.Log($"팝업 열려있나:{gameObject.activeSelf}");
                }
                else
                {
                    if (MultiplayerGameManager.Instance.IsCountdownToStartActive())
                    {
                        Show();
                    }
                    else
                    {
                        Hide();
                    }
                }*/
        if (MultiplayerGameManager.Instance.IsCountdownToStartActive())
        {
            Show();
        }
        else
        {
            Hide();
        }
    }

    private void Show()
    {
        gameObject.SetActive(true);
        //Debug.Log("Show()");
    }

    private void Hide()
    {
        gameObject.SetActive(false);
        //Debug.Log("Hide()");
    }

    public void OpenPopup()
    {
        Show();
    }
}
