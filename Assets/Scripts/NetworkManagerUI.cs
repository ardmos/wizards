using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManagerUI : NetworkBehaviour
{
    [SerializeField] private Button serverBtn;
    [SerializeField] private Button hostBtn;
    [SerializeField] private Button clientBtn;
    [SerializeField] private TextMeshProUGUI playersCountText;

    private NetworkVariable<int> playersNum = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone);

    private void Awake()
    {
        // ��������Ʈ. 
        serverBtn.onClick.AddListener(() =>
        {
            // ���� ����, ��Ʈ��ũ�Ŵ��� ����
            NetworkManager.Singleton.StartServer();
        });
        hostBtn.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();
        });
        clientBtn.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartClient();
        });
    }

    private void Update()
    {
        playersCountText.text = "Players: " + playersNum.Value.ToString();

        if (!IsServer) return;

        playersNum.Value = NetworkManager.Singleton.ConnectedClients.Count;  
    }
}
