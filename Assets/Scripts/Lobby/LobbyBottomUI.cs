using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyBottomUI : MonoBehaviour
{
    [SerializeField] private Button btnCreateGame;
    [SerializeField] private Button btnJoinGame;

    private void Awake()
    {
        btnCreateGame.onClick.AddListener(()=>
        {
            GameMultiplayer.Instance.StartHost();
            //LoadingSceneManager.Load(LoadingSceneManager.Scene.GameRoomScene);
            LoadingSceneManager.LoadNetwork(LoadingSceneManager.Scene.GameRoomScene);
        });

        btnJoinGame.onClick.AddListener(() =>
        {
            GameMultiplayer.Instance.StartClient();
        });
    }
}
