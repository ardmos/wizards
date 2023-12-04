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

    private void Awake()
    {
        btnPlayAgain.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.Shutdown();
            // 로비로 이동 전에 NetworkManager, GameMultiplayManager 중복되지 않도록 깔끔하게 정리.
            CleanUp();
            // 로비로 이동. 
            LoadingSceneManager.Load(LoadingSceneManager.Scene.LobbyScene);
        });
    }

    void Start()
    {
        GameManager.Instance.OnStateChanged += GameManager_OnStateChanged;
        Hide();
    }

    private void GameManager_OnStateChanged(object sender, EventArgs e)
    {
        if (GameManager.Instance.IsGameOver())
        {
            Show();
            InitGameOverUIData();
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

    private void InitGameOverUIData()
    {
        txtRankNumber.text = (GameManager.Instance.GetCurrentAlivePlayerCount()).ToString();        
        txtScoreCount.text = Player.LocalInstance.GetScore().ToString();
    }

    private void CleanUp()
    {
        if (NetworkManager.Singleton != null)
        {
            Destroy(NetworkManager.Singleton.gameObject);
        }
        if (GameMultiplayer.Instance != null)
        {
            Destroy(GameMultiplayer.Instance.gameObject);
        }
    }
}
