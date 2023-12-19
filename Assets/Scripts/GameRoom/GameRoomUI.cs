using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GameRoomUI : MonoBehaviour
{
    [SerializeField] private Button btnReady;
    [SerializeField] private Button btnBack;

    private void Awake()
    {
        btnReady.onClick.AddListener(()=>
        {
            GameRoomReadyManager.Instance.SetPlayerReadyClientUI();
        });

        btnBack.onClick.AddListener(() =>
        {
            CleanUp();
            LoadingSceneManager.Load(LoadingSceneManager.Scene.LobbyScene);
        });
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
