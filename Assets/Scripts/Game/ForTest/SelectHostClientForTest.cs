using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

// �׽�Ʈ�� ��ũ��Ʈ. NetworkManagerUI.cs�� ����� ��� �����ϰ� �ִ�. NetworkManagerUI.cs ��ũ��Ʈ�� ��� �Ⱦ��� �δ� ��.
public class SelectHostClientForTest : NetworkBehaviour
{
    [SerializeField] private Button btnHost;
    [SerializeField] private Button btnClient;

    private void Awake()
    {
        btnHost.onClick.AddListener(() =>
        {
            GameMultiplayer.Instance.StartHost();           
            Hide();
        });
        btnClient.onClick.AddListener(() =>
        {
            GameMultiplayer.Instance.StartClient();
            Hide();
        });
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
