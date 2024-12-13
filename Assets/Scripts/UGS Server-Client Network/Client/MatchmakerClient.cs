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
/// 매치메이킹을 관리하는 클래스입니다.
/// </summary>
public class MatchmakerClient : MonoBehaviour
{
    #region Public Fields
    // 매칭 검색중 활성화되는 UI 컨트롤러
    public PopupLookingForGameUIController popupLookingForGameController;
    #endregion

    #region Constants
    // 에러 메시지 상수
    private const string ERROR_AUTHENTICATION_SERVICE_NOT_SET = "MatchmakerClient AuthenticationService.Instance이 설정되지 않았습니다.";
    private const string ERROR_MATCHMAKER_SERVICE_NOT_SET = "MatchmakerClient MatchmakerService.Instance가 설정되지 않았습니다.";
    private const string ERROR_POPUP_LOOKING_FOR_GAME_NOT_SET = "MatchmakerClient popupLookingForGameController가 설정되지 않았습니다.";
    private const string ERROR_NETWORK_MANAGER_NOT_SET = "MatchmakerClient NetworkManager.Singleton이 설정되지 않았습니다.";
    private const string ERROR_CLIENT_NETWORK_CONNECTION_MANAGER_NOT_SET = "MatchmakerClient ClientNetworkConnectionManager.Instance가 설정되지 않았습니다.";
    private const string ERROR_TICKET_TIMED_OUT = "Failed to get ticket status. Ticket timed out.";
    private const string ERROR_GET_TICKET_FAILED = "Failed to get ticket status. Error:";
    // Unity Gaming Service Matchmaker에 설정된 우리 게임의 매칭 큐 이름
    private const string QUEUE_NAME_BATTLE_ROYAL_MODE = "BattleRoyalMode";
    // 매칭의 기준이 되는 플레이어별 기본 게임 스킬값
    private const int DEFAULT_PLAYER_SKILL_VALUE = 100;
    #endregion

    #region Private Fields
    // 매칭 티켓 ID
    private string ticketId;
    #endregion


    #region Public Methods
    /// <summary>
    /// 매치메이킹을 시작합니다. 매치메이킹 버튼 UI가 클릭됐을 때 호출됩니다.
    /// </summary>
    public void StartMatchmaking()
    {
        CreateTicket();
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// 매치메이킹 티켓을 생성합니다.
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
    /// 티켓 상태를 주기적으로 확인합니다.
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
    /// 티켓이 할당되었을 때 호출되는 메서드입니다.
    /// </summary>
    /// <param name="assignment">할당된 멀티플레이 정보</param>
    private void TicketAssigned(MultiplayAssignment assignment)
    {
        if (!ValidateComponent(NetworkManager.Singleton, ERROR_NETWORK_MANAGER_NOT_SET)) return;
        if (!ValidateComponent(ClientNetworkConnectionManager.Instance, ERROR_CLIENT_NETWORK_CONNECTION_MANAGER_NOT_SET)) return;

        Logger.Log($"Ticket Assigned: {assignment.Ip}:{assignment.Port}");
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(assignment.Ip, (ushort)assignment.Port);
        ClientNetworkConnectionManager.Instance.StartClient();
    }

    /// <summary>
    /// Authentication 서비스에서 플레이어 ID를 가져옵니다.
    /// </summary>
    /// <returns>플레이어 Authentication 서비스ID</returns>
    private string GetAuthenticationPlayerID()
    {
        if (!ValidateComponent(AuthenticationService.Instance, ERROR_AUTHENTICATION_SERVICE_NOT_SET)) return "";

        return AuthenticationService.Instance.PlayerId;
    }
    #endregion
}