using TMPro;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 다른 플레이어를 기다릴 때 표시되는 팝업입니다.
/// </summary>
public class PopupWatingForPlayersUIController : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI txtMessage;

    public override void OnNetworkSpawn()
    {
        /*// Singleplay모드 Multiplay모드 구분 처리
        if (IsHost)
        {
            SingleplayerGameManager.Instance.OnGameStateChanged += OnGameStateChanged;
        }
        else
        {
            MultiplayerGameManager.Instance.OnGameStateChanged += OnGameStateChanged;
        }*/

        MultiplayerGameManager.Instance.OnGameStateChanged += OnGameStateChanged;

        txtMessage.text = "Wating for players...";
        // 자동 레디 보고
        PlayerConnected();
    }

    public override void OnNetworkDespawn()
    {
        /*// Singleplay모드 Multiplay모드 구분 처리
        if (IsHost)
        {
            SingleplayerGameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
        }
        else
        {
            MultiplayerGameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
        }*/
        MultiplayerGameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
    }

    private void OnGameStateChanged(object sender, System.EventArgs e)
    {
        /*       // Singleplay모드 Multiplay모드 구분 처리
               if (IsHost)
               {           
                   if (SingleplayerGameManager.Instance.IsWatingToStart())
                   {
                       Show();
                   }            
                   if (SingleplayerGameManager.Instance.IsCountdownToStartActive())
                   {
                       Hide();
                   }
               }
               else
               {
                   if (MultiplayerGameManager.Instance.IsWatingToStart())
                   {
                       Show();
                   }
                   if (MultiplayerGameManager.Instance.IsCountdownToStartActive())
                   {
                       Hide();
                   }
               }*/
        if (MultiplayerGameManager.Instance.IsWatingToStart())
        {
            Show();
        }
        if (MultiplayerGameManager.Instance.IsCountdownToStartActive())
        {
            Hide();
        }
    }

    private void PlayerConnected()
    {
        /*        // Singleplay모드 Multiplay모드 구분 처리
                if (IsHost)
                {
                    SingleplayerGameManager.Instance.LocalPlayerReadyOnClient();
                }
                else
                {
                    MultiplayerGameManager.Instance.LocalPlayerReadyOnClient();
                }  */
        Debug.Log($"PlayerConnected()");
        MultiplayerGameManager.Instance.PlayerConnectedReportOnClient();
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }
    private void Hide()
    {
        gameObject.SetActive(false);
    }

}
