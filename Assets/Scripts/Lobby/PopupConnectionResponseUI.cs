using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PopupConnectionResponseUI : MonoBehaviour
{
    [SerializeField] private GameObject objectIconError;
    [SerializeField] private GameObject objectIconInfo;
    [SerializeField] private TextMeshProUGUI txtMessage;
    [SerializeField] private TextMeshProUGUI txtInfo;
    [SerializeField] private Color colorWarning;
    [SerializeField] private Color colorInfo;
    [SerializeField] private Button btnClose;

    private void Start()
    {
        GameMultiplayer.Instance.OnFailedToJoinGame += GameMultiplayer_OnFailedToJoinGame;

        btnClose.onClick.AddListener(Hide);

        Hide();
    }

    private void GameMultiplayer_OnFailedToJoinGame(object sender, System.EventArgs e)
    {
        Show();

        // NetworkManager.Singleton.DisconnectReason <- �̰� GameMultiplayer�� StartHost()�κп��� �Է����ְ� ����!
        string reasonMessage = NetworkManager.Singleton.DisconnectReason;
        // �� ���� ���.
        // 1. "Game has already started";
        // 2. "Game is full";
        // 3. "Network connection failed";
        // ����ִ� ���� ���� ����
        if (reasonMessage == "")
        {
            objectIconError.SetActive(true);
            objectIconInfo.SetActive(false);
            txtInfo.enabled = true;
            reasonMessage = "Network connection failed.";
            txtMessage.color = colorWarning;
        }
        else {
            objectIconError.SetActive(false);
            objectIconInfo.SetActive(true);
            txtInfo.enabled = false;
            txtMessage.color = colorInfo;
        }

        txtMessage.text = reasonMessage;
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        // GameMultiplayer�� �� ��ũ��Ʈ�� ������Ʈ�� ����������Ŭ�� �ٸ��� ������ �ռ� �̺�Ʈ ������ �������ش�
        GameMultiplayer.Instance.OnFailedToJoinGame -= GameMultiplayer_OnFailedToJoinGame;
    }
}
