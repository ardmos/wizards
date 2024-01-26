using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
public class TestNetworkUIController : MonoBehaviour
{
    public Button btnStartServer;
    public Button btnStartClient;

    // Start is called before the first frame update
    void Start()
    {
        btnStartServer.onClick.AddListener(()=> {
            GameMultiplayer.Instance.StartServer();
            //LoadingSceneManager.LoadNetwork(LoadingSceneManager.Scene.GameRoomScene);
        });
        btnStartClient.onClick.AddListener(()=> {
            GameMultiplayer.Instance.StartClient();
        });
    }
}
