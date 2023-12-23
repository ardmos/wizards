using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PopupGameOverUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI txtRankNumber;
    [SerializeField] private TextMeshProUGUI txtScoreCount;
    [SerializeField] private Button btnPlayAgain;

    private void Start()
    {
        btnPlayAgain.onClick.AddListener(() =>
        {
            CleanUp();
            // �κ�� �̵�. 
            LoadingSceneManager.Load(LoadingSceneManager.Scene.LobbyScene);
        });

        // ���� ���� �ٲٸ鼭 �ٲ����� �κ�. GameOver�� State���� �Ǵ°� �ƴ϶�, HP�� ���� ���� ������°�.
        GameManager.Instance.OnStateChanged += OnGameManagerStateChanged;
        //Debug.Log("PopupGameOverUI Start");
        Hide();
    }

    private void OnGameManagerStateChanged(object sender, EventArgs e)
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
        InitGameOverUIData();
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    private void InitGameOverUIData()
    {
        txtRankNumber.text = "0"; //(GameManager.Instance.GetCurrentAlivePlayerCount()).ToString();
        txtScoreCount.text = "0"; //Player.LocalInstance.GetScore().ToString();
    }

    private void CleanUp()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
            Destroy(NetworkManager.Singleton.gameObject);
        }
        if (GameMultiplayer.Instance != null)
        {
            Destroy(GameMultiplayer.Instance.gameObject);
        }
    }
}
