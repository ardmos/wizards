using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Matchmaker;
using Unity.Services.Matchmaker.Models;
using UnityEngine;
using static ComponentValidator;

/// <summary>
/// ��ġ����ŷ�� �����ϴ� Ŭ�����Դϴ�.
/// </summary>
public class MatchmakerClient : MonoBehaviour
{
    #region Public Fields
    // ��Ī �˻��� Ȱ��ȭ�Ǵ� UI ��Ʈ�ѷ�
    public PopupLookingForGameUIController popupLookingForGameController;
    #endregion

    #region Constants
    // ���� �޽��� ���
    private const string ERROR_AUTHENTICATION_SERVICE_NOT_SET = "MatchmakerClient AuthenticationService.Instance�� �������� �ʾҽ��ϴ�.";
    private const string ERROR_MATCHMAKER_SERVICE_NOT_SET = "MatchmakerClient MatchmakerService.Instance�� �������� �ʾҽ��ϴ�.";
    private const string ERROR_POPUP_LOOKING_FOR_GAME_NOT_SET = "MatchmakerClient popupLookingForGameController�� �������� �ʾҽ��ϴ�.";
    private const string ERROR_NETWORK_MANAGER_NOT_SET = "MatchmakerClient NetworkManager.Singleton�� �������� �ʾҽ��ϴ�.";
    private const string ERROR_CLIENT_NETWORK_CONNECTION_MANAGER_NOT_SET = "MatchmakerClient ClientNetworkConnectionManager.Instance�� �������� �ʾҽ��ϴ�.";
    private const string ERROR_TICKET_TIMED_OUT = "Failed to get ticket status. Ticket timed out.";
    private const string ERROR_GET_TICKET_FAILED = "Failed to get ticket status. Error:";
    // Unity Gaming Service Matchmaker�� ������ �츮 ������ ��Ī ť �̸�
    private const string QUEUE_NAME_BATTLE_ROYAL_MODE = "BattleRoyalMode";
    // ��Ī�� ������ �Ǵ� �÷��̾ �⺻ ���� ��ų��
    private const int DEFAULT_PLAYER_SKILL_VALUE = 100;
    #endregion

    #region Private Fields
    // ��Ī Ƽ�� ID
    private string ticketId;
    #endregion


    #region Public Methods
    /// <summary>
    /// ��ġ����ŷ�� �����մϴ�. ��ġ����ŷ ��ư UI�� Ŭ������ �� ȣ��˴ϴ�.
    /// </summary>
    public void StartMatchmaking()
    {
        CreateTicket();
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// ��ġ����ŷ Ƽ���� �����մϴ�.
    /// </summary>
    private async void CreateTicket()
    {
        if (!ValidateComponent(MatchmakerService.Instance, ERROR_MATCHMAKER_SERVICE_NOT_SET)) return;
        if (!ValidateComponent(popupLookingForGameController, ERROR_POPUP_LOOKING_FOR_GAME_NOT_SET)) return;

        var options = new CreateTicketOptions(QUEUE_NAME_BATTLE_ROYAL_MODE);
        var players = new List<Unity.Services.Matchmaker.Models.Player>
                        {
                            new Unity.Services.Matchmaker.Models.Player(
                                GetAuthenticationPlayerID(),
                                new MatchmakingPlayerData
                                    {
                                        Skill = DEFAULT_PLAYER_SKILL_VALUE,
                                    }
                            )
                        };

        var ticketResponse = await MatchmakerService.Instance.CreateTicketAsync(players, options);
        ticketId = ticketResponse.Id;
        Logger.Log($"Ticket ID: {ticketId}");
        popupLookingForGameController.Show();
        PollTicketStatus();
    }

    /// <summary>
    /// Ƽ�� ���¸� �ֱ������� Ȯ���մϴ�.
    /// </summary>
    private async void PollTicketStatus()
    {
        MultiplayAssignment multiplayAssignment = null;
        bool gotAssignment = false;
        do
        {
            await Task.Delay(TimeSpan.FromSeconds(1f));
            var ticketStatus = await MatchmakerService.Instance.GetTicketAsync(ticketId);
            if (ticketStatus == null) continue;
            if (ticketStatus.Type == typeof(MultiplayAssignment))
            {
                multiplayAssignment = ticketStatus.Value as MultiplayAssignment;
            }

            switch (multiplayAssignment.Status)
            {
                case MultiplayAssignment.StatusOptions.Timeout:
                    Logger.LogError(ERROR_TICKET_TIMED_OUT);

                    gotAssignment = true;
                    popupLookingForGameController.Hide();
                    break;
                case MultiplayAssignment.StatusOptions.Failed:
                    Logger.LogError($"{ERROR_GET_TICKET_FAILED}{multiplayAssignment.Message}");

                    gotAssignment = true;
                    popupLookingForGameController.Hide();
                    break;
                case MultiplayAssignment.StatusOptions.InProgress:
                    Logger.Log("Searching...");
                    break;
                case MultiplayAssignment.StatusOptions.Found:
                    gotAssignment = true;
                    popupLookingForGameController.Hide();
                    TicketAssigned(multiplayAssignment);
                    break;
                default:
                    throw new InvalidOperationException();
            }
        } while (!gotAssignment);
    }

    /// <summary>
    /// Ƽ���� �Ҵ�Ǿ��� �� ȣ��Ǵ� �޼����Դϴ�.
    /// </summary>
    /// <param name="assignment">�Ҵ�� ��Ƽ�÷��� ����</param>
    private void TicketAssigned(MultiplayAssignment assignment)
    {
        if (!ValidateComponent(NetworkManager.Singleton, ERROR_NETWORK_MANAGER_NOT_SET)) return;
        if (!ValidateComponent(ClientNetworkConnectionManager.Instance, ERROR_CLIENT_NETWORK_CONNECTION_MANAGER_NOT_SET)) return;

        Logger.Log($"Ticket Assigned: {assignment.Ip}:{assignment.Port}");
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(assignment.Ip, (ushort)assignment.Port);
        ClientNetworkConnectionManager.Instance.StartClient();
    }

    /// <summary>
    /// Authentication ���񽺿��� �÷��̾� ID�� �����ɴϴ�.
    /// </summary>
    /// <returns>�÷��̾� Authentication ����ID</returns>
    private string GetAuthenticationPlayerID()
    {
        if (!ValidateComponent(AuthenticationService.Instance, ERROR_AUTHENTICATION_SERVICE_NOT_SET)) return "";

        return AuthenticationService.Instance.PlayerId;
    }
    #endregion
}