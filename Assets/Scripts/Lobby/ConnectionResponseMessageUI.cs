using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ConnectionResponseMessageUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI txtMessage;
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
        // ����ִ� ���� ���� ����
        if (reasonMessage == "")
        {
            reasonMessage = "Failed to connect";
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
