using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PopupMenuUIController : MonoBehaviour
{
    // �޴��˾� �ݱ�
    [SerializeField] private CustomClickSoundButton btnContinue;
    // �κ��
    [SerializeField] private CustomClickSoundButton btnHome;
    // ���� ����. �����ϱ� ( �����ý��� ���� ���� )

    private void Start()
    {
        btnContinue.AddClickListener(Hide);
        btnHome.AddClickListener(() =>
        {
            CleanUp();
            // �κ������ �̵�
            LoadSceneManager.Load(LoadSceneManager.Scene.LobbyScene);
        });
        Hide();
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
