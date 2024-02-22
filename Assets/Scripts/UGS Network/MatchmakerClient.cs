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
    // ���Ŀ� �� �˾��� ���ӷ��� ����ϰԵɰ���. ��ó��.
    public PopupLookingForGameUIController popupLookingForGame;

    private string ticketId;

    private string GetPlayerID()
    {
        return AuthenticationService.Instance.PlayerId;
    }
    
    // ��¥ ��Ī ���� �޼���. (��ġ����ŷ �κ� �ڵ� ������. 02/22)
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


    // GameMultiplayer���� ������ �ڵ�� (��ġ����ŷ �κ� �ڵ� ������. 02/22)
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

        // Ŭ���̾�Ʈ�� ��Ʈ��ũ�Ŵ����� ���ٰ� �ٽ� �� ��, OnNetworkSpawn�� ȣ��˴ϴ�. �� �� �Ʒ� �̺�Ʈ�� �Ǵٽ� ��ϵǱ⶧���� ������Ҹ� ���ְ��ֽ��ϴ�.
        playerDataNetworkList.OnListChanged -= OnServerListChanged;
        NetworkManager.Singleton.Shutdown();
    }

    /// <summary>
    ///  Ŭ���̾�Ʈ ������. ���� ������ �� �ϵ�
    ///  1. NetworkList�� PlayerDataList�� ���� �������� Ŭ���� ������ ����Ѵ�.
    /// </summary>
    private void Client_OnClientConnectedCallback(ulong clientId)
    {
        // ��Ī UI ������ ���� �̺�Ʈ �ڵ鷯 ȣ��
        OnSucceededToJoinMatch?.Invoke(this, EventArgs.Empty);
        // ����RPC�� ���� ������ ����
        Debug.Log($"Client_OnClientConnectedCallback. clientId: {clientId}, class: {PlayerDataManager.Instance.GetCurrentPlayerClass()}");
        //ChangePlayerClass(PlayerProfileData.Instance.GetCurrentSelectedClass());
        SavePlayerInGameDataOnServer(PlayerDataManager.Instance.GetPlayerInGameData());
    }
    private void Client_OnClientDisconnectCallback(ulong obj)
    {
        Debug.Log($"OnClientDisconnectCallback : {obj}");
        // ��Ī UI ������ ���� �̺�Ʈ �ڵ鷯 ȣ��. 
        OnFailedToJoinMatch?.Invoke(this, EventArgs.Empty);
    }
    // (��ġ����ŷ �κ� �ڵ� ������. 02/22)

    [Serializable]
    public class MatchmakingPlayerData
    {
        public int Skill;
    }
}
