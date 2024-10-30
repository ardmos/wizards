using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
public class TestNetworkUIController : MonoBehaviour
{
    public CustomClickSoundButton btnStartServer;
    public CustomClickSoundButton btnStartClient;

    // Start is called before the first frame update
    void Start()
    {
        btnStartServer.AddClickListener(()=> {
            ServerNetworkManager.Instance.StartServer();
            //LoadingSceneManager.LoadNetwork(LoadingSceneManager.Scene.GameRoomScene);
        });
        btnStartClient.AddClickListener(()=> {
            ClientNetworkManager.Instance.StartClient();
        });
    }
}
