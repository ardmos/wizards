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
            // 로비로 이동 전에 NetworkManager, GameMultiplayManager 중복되지 않도록 깔끔하게 정리.
            CleanUp();
            // 로비로 이동.
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
        // 강퇴당했을 시 NetworkManager와 현 스크립트의 오브젝트는 라이프사이클이 다르기 때문에 손수 이벤트 구독을 해제해준다
        NetworkManager.Singleton.OnClientDisconnectCallback -= NetworkManager_OnClientDisconnectCallback;
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
    {
        Debug.Log($"누군가 나갔습니다. clientId : {clientId}");
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
