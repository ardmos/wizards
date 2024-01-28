using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

/// 이제 사용 안함. 삭제 가능
public class GameRoomUI : MonoBehaviour
{
    [SerializeField] private Button btnReady;
    [SerializeField] private Button btnBack;

    private void Awake()
    {
        btnReady.onClick.AddListener(()=>
        {
            GameMatchReadyManager.Instance.SetPlayerReadyServerRpc();
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
