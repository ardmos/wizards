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


    // Start is called before the first frame update
    void Start()
    {
        btnClose.onClick.AddListener(Hide);
        btnRestart.onClick.AddListener(() =>
        {
            // �̵� ���� UGS Multiplay �÷��� ���� ó��
            if (NetworkManager.Singleton != null) NetworkManager.Singleton.Shutdown();
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
            Destroy(NetworkManager.Singleton.gameObject);
        }
        if (GameMultiplayer.Instance != null)
        {
            Destroy(GameMultiplayer.Instance.gameObject);
        }
    }
}