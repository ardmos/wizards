using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

// �׽�Ʈ�� ��ũ��Ʈ. NetworkManagerUI.cs�� ����� ��� �����ϰ� �ִ�. NetworkManagerUI.cs ��ũ��Ʈ�� ��� �Ⱦ��� �δ� ��.
public class SelectHostClientForTest : NetworkBehaviour
{
    [SerializeField] private Button hostBtn;
    [SerializeField] private Button clientBtn;

    private void Awake()
    {
        // ��������Ʈ. 
        hostBtn.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();
            Hide();
        });
        clientBtn.onClick.AddListener(() =>
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
