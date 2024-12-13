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
}
