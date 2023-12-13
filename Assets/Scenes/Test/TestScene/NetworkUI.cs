using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkUI : MonoBehaviour
{
    [SerializeField] private Button btnServer;
    [SerializeField] private Button btnHost;
    [SerializeField] private Button btnClient;

    private void Awake()
    {
        btnServer.onClick.AddListener(() => 
        {
            NetworkManager.Singleton.StartServer();
        });
        btnHost.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();
            //GameMultiplayer.Instance.StartHost();
            //LoadingSceneManager.Load(LoadingSceneManager.Scene.GameRoomScene);
            LoadingSceneManager.LoadNetwork(LoadingSceneManager.Scene.GameRoomScene);
        });

        btnClient.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartClient();
            //GameMultiplayer.Instance.StartClient();
        });
    }
}
