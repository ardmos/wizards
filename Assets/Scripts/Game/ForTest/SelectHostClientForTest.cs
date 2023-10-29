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
        // ��������Ʈ. 
        btnHost.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();
            GameManager.Instance.StartReady();
            Hide();
        });
        btnClient.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartClient();
            Hide();
        });
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
