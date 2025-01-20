using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// �ٸ� �÷��̾ ��ٸ� �� ǥ�õǴ� �˾��Դϴ�.
/// </summary>
public class PopupWatingForPlayersUIController : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI txtMessage;

    public override void OnNetworkSpawn()
    {
        /*// Singleplay��� Multiplay��� ���� ó��
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
            // �ڵ� ���� ����. ������ MultiplayerGameManager�� ������ �ð��� ��ٸ��� ���� 1�� ��� �� �����մϴ�. 
            StartCoroutine(PlayerConnectedReportAfter1Second());
    }

    public override void OnNetworkDespawn()
    {
        /*// Singleplay��� Multiplay��� ���� ó��
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
        /*       // Singleplay��� Multiplay��� ���� ó��
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
        /*        // Singleplay��� Multiplay��� ���� ó��
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
