using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PopupHostHasDisconnectedUI : MonoBehaviour
{
    [SerializeField] private Button btnPlayAgain;

    private void Awake()
    {
        btnPlayAgain.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.Shutdown();           
            // �κ�� �̵�.
            LoadingSceneManager.Load(LoadingSceneManager.Scene.LobbyScene);
        });
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
    }

    private void Start()
    {
        Hide();              
    }

    private void OnDestroy()
    {        
        //NetworkManager.Singleton.OnClientDisconnectCallback
        NetworkManager.Singleton.OnClientDisconnectCallback -= NetworkManager_OnClientDisconnectCallback;
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
    {
        // ������ �������� ������ ���
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
