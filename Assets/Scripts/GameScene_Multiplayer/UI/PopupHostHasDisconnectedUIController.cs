using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PopupHostHasDisconnectedUIController : MonoBehaviour
{
    [SerializeField] private CustomClickSoundButton btnPlayAgain;

    private void Start()
    {
        btnPlayAgain.AddClickListener(() =>
        {
            CleanUp();
            // �κ�� �̵�.
            LoadSceneManager.Load(LoadSceneManager.Scene.LobbyScene);
        });

        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;

        Hide();
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
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

    private void CleanUp()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
            Destroy(NetworkManager.Singleton.gameObject);
        }
        if (ServerNetworkManager.Instance != null)
        {
            Destroy(ServerNetworkManager.Instance.gameObject);
        }
        if (ClientNetworkManager.Instance != null)
        {
            Destroy(ClientNetworkManager.Instance.gameObject);
        }
    }
}
