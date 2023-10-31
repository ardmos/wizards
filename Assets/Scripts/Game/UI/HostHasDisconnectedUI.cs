using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class HostHasDisconnectedUI : MonoBehaviour
{
    [SerializeField] private Button btnPlayAgain;

    private void Start()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
        Hide();

        btnPlayAgain.onClick.AddListener(() => 
        {
            NetworkManager.Singleton.Shutdown();
            // 로비로 이동.
            LoadingSceneManager.Load(LoadingSceneManager.Scene.LobbyScene);
        });                    
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
    {
        if (clientId == NetworkManager.ServerClientId)
        {
            // Server is shutting down
            Show();
        }
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
