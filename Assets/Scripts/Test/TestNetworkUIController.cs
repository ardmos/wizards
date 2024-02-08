using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
public class TestNetworkUIController : MonoBehaviour
{
    public CustomButton btnStartServer;
    public CustomButton btnStartClient;

    // Start is called before the first frame update
    void Start()
    {
        btnStartServer.AddClickListener(()=> {
            GameMultiplayer.Instance.StartServer();
            //LoadingSceneManager.LoadNetwork(LoadingSceneManager.Scene.GameRoomScene);
        });
        btnStartClient.AddClickListener(()=> {
            GameMultiplayer.Instance.StartClient();
        });
    }
}
