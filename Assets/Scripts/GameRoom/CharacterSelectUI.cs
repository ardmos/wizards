using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectUI : MonoBehaviour
{
    [SerializeField] private Button btnReady;
    [SerializeField] private Button btnBack;

    private void Awake()
    {
        btnReady.onClick.AddListener(()=>
        {
            CharacterSelectReady.Instance.SetPlayerReady();
        });

        btnBack.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.Shutdown();
            CleanUp();

            LoadingSceneManager.Load(LoadingSceneManager.Scene.LobbyScene);
        });
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
