using System.Collections;
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

        if (IsServer) Hide();
        else
            // 자동 레디 보고. 서버의 MultiplayerGameManager가 생성될 시간을 기다리기 위해 1초 대기 후 보고합니다. 
            StartCoroutine(PlayerConnectedReportAfter1Second());
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

    private IEnumerator PlayerConnectedReportAfter1Second()
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
        yield return new WaitForSeconds(1);

        MultiplayerGameManager.Instance.PlayerConnectedReportServerRPC();
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
