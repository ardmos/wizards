using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI txtRankNumber;
    [SerializeField] private TextMeshProUGUI txtScoreCount;

    void Start()
    {
        GameManager.Instance.OnStateChanged += GameManager_OnStateChanged;
        Hide();
    }

    private void Update()
    {
        txtRankNumber.text = (GameManager.Instance.GetCurrentAlivePlayerCount()).ToString();
        // and call minus current Player count method!!!

        txtScoreCount.text = GameManager.Instance.ownerPlayerObject.GetComponent<Player>().GetScore().ToString();
    }

    private void GameManager_OnStateChanged(object sender, EventArgs e)
    {
        if (GameManager.Instance.IsGameOver())
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
