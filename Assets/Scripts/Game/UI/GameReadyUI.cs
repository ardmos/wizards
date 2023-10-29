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
        // 1. �����ϼ��� ���� ���� �ʿ�.
        // 2. ���� ���� ���� �� ���� ������Ʈ ����ȭ �ʿ�. 
        // �� �� ���� �ϸ� ��. ���Ĵ� �� ��ũ ���߱��. 


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
