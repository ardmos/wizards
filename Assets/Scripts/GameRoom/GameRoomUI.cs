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
            NetworkManager.Singleton.Shutdown();
            LoadingSceneManager.Load(LoadingSceneManager.Scene.LobbyScene);
        });
    }

    private void OnDestroy()
    {
        CleanUp();
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
