using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

// 테스트용 스크립트. NetworkManagerUI.cs의 기능을 잠시 따라하고 있다. NetworkManagerUI.cs 스크립트는 잠시 안쓰고 두는 중.
public class SelectHostClientForTest : NetworkBehaviour
{
    [SerializeField] private Button btnHost;
    [SerializeField] private Button btnClient;

    private void Awake()
    {
        // 델리게이트. 
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
