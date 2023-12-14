using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Matchmaker;
using Unity.Services.Matchmaker.Models;
using UnityEngine;
using UnityEngine.UI;
using StatusOptions = Unity.Services.Matchmaker.Models.MultiplayAssignment.StatusOptions;

/// <summary>
/// �ϴ� ��
/// 1. ��ġ����ŷ ����.
/// </summary>
public class MatchmakerClient : MonoBehaviour
{
    public Button btnStartMatchmaking;
    // ���Ŀ� �� �˾��� ���ӷ��� ����ϰԵɰ���. ��ó��.
    public PopupLookingForGameUI popupLookingForGame;

    private string ticketId;

    private void Start()
    {
        btnStartMatchmaking.onClick.AddListener(StartClient);
    }

    private string GetPlayerID()
    {
        return AuthenticationService.Instance.PlayerId;
    }

    // ��ġ����ŷ ���� �޼���. (��ġ����ŷ ��ư UI�� Ŭ������ �� ȣ��ȴ�.)
    public void StartClient()
    {
        CreateATicket();
    }

    private async void CreateATicket()
    {
        // �츮 ť �̸�
        var options = new CreateTicketOptions("BattleRoyalMode");

        var players = new List<Unity.Services.Matchmaker.Models.Player>
        {
            new Unity.Services.Matchmaker.Models.Player(
                GetPlayerID(),
                new MatchmakingPlayerData
                {
                    Skill = 100,
                }
            )
        };

        var ticketResponse = await MatchmakerService.Instance.CreateTicketAsync(players, options);
        ticketId = ticketResponse.Id;
        Debug.Log($"Ticket ID: {ticketId}");

        popupLookingForGame.Show();
        PollTicketStatus();
    }

    private async void PollTicketStatus()
    {
        MultiplayAssignment multiplayAssignment = null;
        bool gotAssignment = false;
        do
        {
            await Task.Delay(TimeSpan.FromSeconds(1f));
            var ticketStatus = await MatchmakerService.Instance.GetTicketAsync(ticketId);
            if (ticketStatus == null) continue;
            if(ticketStatus.Type == typeof(MultiplayAssignment))
            {
                multiplayAssignment = ticketStatus.Value as MultiplayAssignment;
            }

            switch (multiplayAssignment.Status)
            {
                case StatusOptions.Timeout:
                    gotAssignment = true;
                    Debug.LogError("Failed to get ticket status. Ticket timed out.");
                    popupLookingForGame.Hide();
                    break;
                case StatusOptions.Failed:
                    gotAssignment = true;
                    Debug.LogError($"Faild to get ticket status. Error: {multiplayAssignment.Message}");
                    popupLookingForGame.Hide();
                    break;
                case StatusOptions.InProgress:
                    Debug.Log("Searching...");
                    break;
                case StatusOptions.Found:
                    gotAssignment = true;
                    popupLookingForGame.Hide();
                    TicketAssigned(multiplayAssignment);
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }while( !gotAssignment );
    }

    private void TicketAssigned(MultiplayAssignment assignment)
    {
        Debug.Log($"Ticket Assigned: {assignment.Ip}:{assignment.Port}");
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(assignment.Ip, (ushort)assignment.Port);
        //NetworkManager.Singleton.StartClient();
        GameMultiplayer.Instance.StartClient();
    }


    [Serializable]
    public class MatchmakingPlayerData
    {
        public int Skill;
    }
}
