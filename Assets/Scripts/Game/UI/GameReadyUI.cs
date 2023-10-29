using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UI;

public class GameReadyUI : MonoBehaviour
{
    [SerializeField] private Button btnReady;
    [SerializeField] private TextMeshProUGUI txtWatingForPlayers;

    private void Start()
    {
        GameManager.Instance.OnStateChanged += GameManager_OnStateChanged;
        Hide();

        btnReady.onClick.AddListener(() =>
        {
            LocalPlayerReady();
        });
    }

    private void GameManager_OnStateChanged(object sender, System.EventArgs e)
    {
        if (GameManager.Instance.IsWatingToStart())
        {
            Show();
        }
        else
        {
            Hide();
        }
    }

    private void LocalPlayerReady()
    {
        // 1. 레디하세요 문구 노출 필요.
        // 2. 레디 여부 전달 및 게임 스테이트 동기화 필요. 
        // 이 둘 부터 하면 됨. 이후는 쭉 싱크 맞추기들. 


        // hide ready button UI
        btnReady.gameObject.SetActive(false);
        // report ready state to GameManager???? Complete this work when finish Sync Game State!

        // show "Wating for players" text
        txtWatingForPlayers.gameObject.SetActive(true);
        // # If every player get ready, the Game State will change.  
    }

    private void Show()
    {
        gameObject.SetActive(true);
        btnReady.gameObject.SetActive(true);
        txtWatingForPlayers.gameObject.SetActive(false);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

}
