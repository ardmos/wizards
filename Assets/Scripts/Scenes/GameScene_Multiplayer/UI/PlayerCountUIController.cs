using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PlayerCountUIController : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI txtAlivePlayerCount;

    private void Start()
    {
        // Singleplay��� Multiplay��� ���� ó��
        if(IsHost)
        {
            SingleplayerGameManager.Instance.OnAlivePlayerCountChanged += OnAlivePlayerCountChanged;
        }
        else
        {
            MultiplayerGameManager.Instance.OnAlivePlayerCountChanged += OnAlivePlayerCountChanged;
        }
    }

    private void OnDestroy()
    {
        // Singleplay��� Multiplay��� ���� ó��
        if (IsHost)
        {
            SingleplayerGameManager.Instance.OnAlivePlayerCountChanged -= OnAlivePlayerCountChanged;
        }
        else
        {
            MultiplayerGameManager.Instance.OnAlivePlayerCountChanged -= OnAlivePlayerCountChanged;
        }   
    }

    private void OnAlivePlayerCountChanged(object sender, System.EventArgs e)
    {
        // Singleplay��� Multiplay��� ���� ó��
        if (IsHost)
        {
            txtAlivePlayerCount.text = SingleplayerGameManager.Instance.GetCurrentAlivePlayerCount().ToString();
        }
        else
        {
            txtAlivePlayerCount.text = MultiplayerGameManager.Instance.GetCurrentAlivePlayerCount().ToString();
        }        
    }
}
