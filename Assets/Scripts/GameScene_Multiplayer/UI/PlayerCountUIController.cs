using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PlayerCountUIController : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI txtAlivePlayerCount;

    private void Start()
    {
        // Singleplay모드 Multiplay모드 구분 처리
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
        // Singleplay모드 Multiplay모드 구분 처리
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
        // Singleplay모드 Multiplay모드 구분 처리
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
