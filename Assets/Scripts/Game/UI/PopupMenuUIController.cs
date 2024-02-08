using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PopupMenuUIController : MonoBehaviour
{
    // �޴��˾� �ݱ�
    [SerializeField] private CustomButton btnContinue;
    // �κ��
    [SerializeField] private CustomButton btnHome;
    // ���� ����. �����ϱ� ( �����ý��� ���� ���� )

    private void Start()
    {
        btnContinue.AddClickListener(Hide);
        btnHome.AddClickListener(() =>
        {
            CleanUp();
            // �κ������ �̵�
            LoadingSceneManager.Load(LoadingSceneManager.Scene.LobbyScene);
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
        if (GameMultiplayer.Instance != null)
        {
            Destroy(GameMultiplayer.Instance.gameObject);           
        }
    }
}
