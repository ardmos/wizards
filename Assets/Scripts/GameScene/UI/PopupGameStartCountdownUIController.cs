using System;
using TMPro;
using UnityEngine;

public class PopupGameStartCountdownUIController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI txtCountdown;

    private double tmpCountdownValue = 0;
    private void Start()
    {
        GameManager.Instance.OnGameStateChanged += GameManager_OnStateChanged;
        Hide();
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnGameStateChanged -= GameManager_OnStateChanged;
    }

    private void Update()
    {
        double countdownTime = Math.Ceiling(GameManager.Instance.GetCountdownToStartTimer());
        if (tmpCountdownValue == countdownTime) return;
        switch (countdownTime)
        {
            case 0:
            case 1:
            case 2:
            case 3:
                tmpCountdownValue = countdownTime;
                SoundManager.Instance?.PlayCountdownAnnouncer(countdownTime);
                break;
            default:
                break;
        }

        txtCountdown.text = countdownTime.ToString();
    }

    private void GameManager_OnStateChanged(object sender, EventArgs e)
    {
        //Debug.Log($"GameManager_OnStateChanged is count down to start active? {GameManager.Instance.IsCountdownToStartActive()}");
        if (GameManager.Instance.IsCountdownToStartActive())
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
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
