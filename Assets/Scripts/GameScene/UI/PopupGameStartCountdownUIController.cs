using System;
using TMPro;
using UnityEngine;

public class PopupGameStartCountdownUIController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI txtCountdown;

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
        txtCountdown.text = Math.Ceiling(GameManager.Instance.GetCountdownToStartTimer()).ToString();
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
