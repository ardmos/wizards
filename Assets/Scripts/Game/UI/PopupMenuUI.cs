using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PopupMenuUI : MonoBehaviour
{
    // �޴��˾� �ݱ�
    [SerializeField] private Button btnClose;
    // �κ��
    [SerializeField] private Button btnRestart;
    // ���� ����. �����ϱ� ( �����ý��� ���� ���� )

    private void Awake()
    {
        btnClose.onClick.AddListener(Hide);
        btnRestart.onClick.AddListener(() =>
        {
            CleanUp();
            // �κ������ �̵�
            LoadingSceneManager.Load(LoadingSceneManager.Scene.LobbyScene);
        });
    }

    public void Show()
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
        if (GameMultiplayer.Instance != null)
        {
            Destroy(GameMultiplayer.Instance.gameObject);           
        }
    }
}
