using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using Unity.Services.Matchmaker;
using Unity.Services.Matchmaker.Models;
using StatusOptions = Unity.Services.Matchmaker.Models.MultiplayAssignment.StatusOptions;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
#if UNITY_EDITOR
using ParrelSync;
#endif

public class MatchmakerClient : MonoBehaviour
{
    private string ticketId;

    private void OnEnable()
    {
        ServerStartUp.ClientInstance += SignIn;
    }

    private void OnDisable()
    {
        ServerStartUp.ClientInstance -= SignIn;
    }

    private async void SignIn()
    {
        #region 테스트용!
        await ClientSignIn("WizardsandKnightsPlayer");
        #endregion
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private async Task ClientSignIn(string serviceProfileName = null)
    {
        #region 컴퓨터에서 테스트를 위한 처리. 한 컴퓨터에서 여러개의 클라이언트를 띄워주기 위한 준비 
        if(serviceProfileName != null)
        {
            #if UNITY_EDITOR
            serviceProfileName = $"{serviceProfileName}{GetCloneNumberSuffix()}";
            #endif
            var initOptions = new InitializationOptions();
            initOptions.SetProfile(serviceProfileName);
            await UnityServices.InitializeAsync(initOptions);
        }
        #endregion
        else
        {
            //Initialize Unity Services <-- 이건 원래 여기서 하는 일.
            await UnityServices.InitializeAsync();
        }

        Debug.Log($"Signed In Anonymously as {serviceProfileName}({PlayerID()})");
    }

    private string PlayerID()
    {
        return AuthenticationService.Instance.PlayerId;
    }

    #if UNITY_EDITOR
    private string GetCloneNumberSuffix()
    {
        string projectPath = ClonesManager.GetCurrentProjectPath();
        int lastUnderscore = projectPath.LastIndexOf('_');  
        string projectCloneSuffix = projectPath.Substring(lastUnderscore+1);
        if(projectCloneSuffix.Length != 1)
        {
            projectCloneSuffix = "";
        }
        return projectCloneSuffix;
    }
    #endif

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
                PlayerID(),
                new MatchmakingPlayerData
                {
                    Skill = 100,
                }
            )
        };

        var ticketResponse = await MatchmakerService.Instance.CreateTicketAsync(players, options);
        ticketId = ticketResponse.Id;
        Debug.Log($"Ticket ID: {ticketId}");
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
                    break;
                case StatusOptions.Failed:
                    gotAssignment = true;
                    Debug.LogError($"Faild to get ticket status. Error: {multiplayAssignment.Message}");
                    break;
                case StatusOptions.InProgress:
                    break;
                case StatusOptions.Found:
                    gotAssignment = true;
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
        NetworkManager.Singleton.StartClient();
    }


    [Serializable]
    public class MatchmakingPlayerData
    {
        public int Skill;
    }
}
