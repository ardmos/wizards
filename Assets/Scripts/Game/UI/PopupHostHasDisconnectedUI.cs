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
            // �κ�� �̵� ���� NetworkManager, GameMultiplayManager �ߺ����� �ʵ��� ����ϰ� ����.
            CleanUp();
            // �κ�� �̵�.
            LoadingSceneManager.Load(LoadingSceneManager.Scene.LobbyScene);
        });
    }

    private void Start()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
        Hide();              
    }

    private void OnDestroy()
    {
        // ��������� �� NetworkManager�� �� ��ũ��Ʈ�� ������Ʈ�� ����������Ŭ�� �ٸ��� ������ �ռ� �̺�Ʈ ������ �������ش�
        NetworkManager.Singleton.OnClientDisconnectCallback -= NetworkManager_OnClientDisconnectCallback;
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
    {
        Debug.Log($"������ �������ϴ�. clientId : {clientId}");
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
            Destroy(NetworkManager.Singleton.gameObject);
        }
        if (GameMultiplayer.Instance != null)
        {
            Destroy(GameMultiplayer.Instance.gameObject);
        }
    }
}
