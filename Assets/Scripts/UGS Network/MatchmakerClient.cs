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
/// 하는 일
/// 1. 매치메이킹 관리.
/// </summary>
public class MatchmakerClient : MonoBehaviour
{
    // 추후엔 이 팝업이 게임룸을 대신하게될거임. 롤처럼.
    public PopupLookingForGameUIController popupLookingForGame;

    private string ticketId;

    private string GetPlayerID()
    {
        return AuthenticationService.Instance.PlayerId;
    }
    
    // 진짜 매칭 시작 메서드. (매치메이킹 부분 코드 정리중. 02/22)
    // 매치메이킹 시작 메서드. (매치메이킹 버튼 UI가 클릭됐을 때 호출된다.)
    public void StartClient()
    {
        CreateATicket();
    }

    private async void CreateATicket()
    {
        // 우리 큐 이름
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


    // GameMultiplayer에서 가져온 코드들 (매치메이킹 부분 코드 정리중. 02/22)
    public void StartClient()
    {
        //OnTryingToJoinGame?.Invoke(this, EventArgs.Empty);
        NetworkManager.Singleton.OnClientConnectedCallback += Client_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += Client_OnClientDisconnectCallback;
        NetworkManager.Singleton.StartClient();
    }

    public void StopClient()
    {
        NetworkManager.Singleton.OnClientConnectedCallback -= Client_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback -= Client_OnClientDisconnectCallback;

        // 클라이언트측 네트워크매니저를 껐다가 다시 켤 때, OnNetworkSpawn도 호출됩니다. 그 때 아래 이벤트가 또다시 등록되기때문에 구독취소를 해주고있습니다.
        playerDataNetworkList.OnListChanged -= OnServerListChanged;
        NetworkManager.Singleton.Shutdown();
    }

    /// <summary>
    ///  클라이언트 측에서. 접속 성공시 할 일들
    ///  1. NetworkList인 PlayerDataList에 현재 선택중인 클래스 정보를 등록한다.
    /// </summary>
    private void Client_OnClientConnectedCallback(ulong clientId)
    {
        // 매칭 UI 실행을 위한 이벤트 핸들러 호출
        OnSucceededToJoinMatch?.Invoke(this, EventArgs.Empty);
        // 서버RPC를 통해 서버에 저장
        Debug.Log($"Client_OnClientConnectedCallback. clientId: {clientId}, class: {PlayerDataManager.Instance.GetCurrentPlayerClass()}");
        //ChangePlayerClass(PlayerProfileData.Instance.GetCurrentSelectedClass());
        SavePlayerInGameDataOnServer(PlayerDataManager.Instance.GetPlayerInGameData());
    }
    private void Client_OnClientDisconnectCallback(ulong obj)
    {
        Debug.Log($"OnClientDisconnectCallback : {obj}");
        // 매칭 UI 숨김을 위한 이벤트 핸들러 호출. 
        OnFailedToJoinMatch?.Invoke(this, EventArgs.Empty);
    }
    // (매치메이킹 부분 코드 정리중. 02/22)

    [Serializable]
    public class MatchmakingPlayerData
    {
        public int Skill;
    }
}
