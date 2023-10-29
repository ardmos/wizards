using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// ��Ʈ��ũ�� �ʿ��� UI�� ���⼭ ���� 
/// </summary>
public class NetworkManagerUI : NetworkBehaviour
{
    [SerializeField] private Button btnHost;
    [SerializeField] private Button btnClient;

    private void Awake()
    {
        // ��������Ʈ. 
        btnHost.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();
        });
        btnClient.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartClient();
        });
    }
}
