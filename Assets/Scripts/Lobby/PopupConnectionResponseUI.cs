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

        // NetworkManager.Singleton.DisconnectReason <- 이건 GameMultiplayer의 StartHost()부분에서 입력해주고 있음!
        string reasonMessage = NetworkManager.Singleton.DisconnectReason;
        // 세 가지 경우.
        // 1. "Game has already started";
        // 2. "Game is full";
        // 3. "Network connection failed";
        // 비어있는 경우는 연결 실패
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
        // GameMultiplayer와 현 스크립트의 오브젝트는 라이프사이클이 다르기 때문에 손수 이벤트 구독을 해제해준다
        GameMultiplayer.Instance.OnFailedToJoinGame -= GameMultiplayer_OnFailedToJoinGame;
    }
}
