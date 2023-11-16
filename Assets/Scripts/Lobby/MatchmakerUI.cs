using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Matchmaker;
using Unity.Services.Matchmaker.Models;


// �ʿ��� ��
// 1. UGS ���� �̴ϼȶ����� �Ǳ� ������ ��ư ��Ȱ��ȭ <- TitleScene���� ���� �̴ϼȶ����� �̹� �Ѱɷ� �ذ�.
// 2. ��ġ����ŷ ���� ����
// 3. Ŭ���̾�Ʈ ����

public class MatchmakerUI : MonoBehaviour
{
    // ���� UGS �뽬���忡�� �����ص� ť �̸� 
    public const string DEFAULT_QUEUE = "default-queue";

    // Find Match ��ư
    [SerializeField] private Button btnFindGame;
    [SerializeField] private Transform lookingForGameTransform;

    // UGS Ticket ���� ����
    private CreateTicketResponse createTicketResponse;
    // UGS doc ������ ���� (ticket poll�� 1�� �̻��̾�� �Ѵ�)
    private float pollTicketTimer;
    private float pollTicketTimerMax = 1.1f;

    private void Awake()
    {
        lookingForGameTransform.gameObject.SetActive(false);

        btnFindGame.onClick.AddListener(() =>
        {
            FindMatch();
        });
    }

    private void Start()
    {
        Debug.Log($"gUnityServices State : {UnityServices.State}");
    }

    private void Update()
    {
        if (createTicketResponse != null)
        {
            // ticket�� ���� ��� 1.1�ʸ��� poll ticket ��û
            pollTicketTimer -= Time.deltaTime;
            if (pollTicketTimer <= 0f)
            {
                pollTicketTimer = pollTicketTimerMax;

                PollMatchmakerTicket();
            }
        }
    }

    private async void FindMatch()
    {
        Debug.Log("FindMatch");

        lookingForGameTransform.gameObject.SetActive(true);

        createTicketResponse = await MatchmakerService.Instance.CreateTicketAsync(new List<Unity.Services.Matchmaker.Models.Player>
        {
            new Unity.Services.Matchmaker.Models.Player(AuthenticationService.Instance.PlayerId, 
            // Extra Data ����� ��Ī Ǯ �����µ� �ʿ��� Skill ���� ������ �ִ�. UGS �뽬���忡�� �Է��ص� ������ 'Skill'
            new MatchmakingPlayerData
            {
                Skill = 100,
            })
        }, new CreateTicketOptions { QueueName = DEFAULT_QUEUE });

        // ��� ��ٷȴ� poll �Ѵ�
        pollTicketTimer = pollTicketTimerMax;
    }

    [Serializable]
    public class MatchmakingPlayerData
    {
        public int Skill;
    }

    private async void PollMatchmakerTicket()
    {
        Debug.Log("PollMatchmakerTicket");

        TicketStatusResponse ticketStatusResponse = await MatchmakerService.Instance.GetTicketAsync(createTicketResponse.Id);

        if (ticketStatusResponse == null)
        {
            // ������Ʈ�� ������ ����. ��� ��ٸ���
            Debug.Log("keep wating");
            return;
        }

        // ������Ʈ�� ������ ����. ������Ʈ�� ticktStatusResponse�� Ÿ���� Ȯ���Ѵ�.
        if (ticketStatusResponse.Type == typeof(MultiplayAssignment))
        {
            //Get Multiplay Assignment
            MultiplayAssignment multiplayAssignment = ticketStatusResponse.Value as MultiplayAssignment;

            Debug.Log("multiplayAssignment.Status " + multiplayAssignment.Status);
            switch (multiplayAssignment.Status)
            {
                case MultiplayAssignment.StatusOptions.Timeout:
                    createTicketResponse = null;
                    Debug.Log("Multiplay Timeout!");
                    lookingForGameTransform.gameObject.SetActive(false);
                    break;
                case MultiplayAssignment.StatusOptions.Failed:
                    createTicketResponse = null;
                    Debug.Log("Failed to create Multiplay server!");
                    lookingForGameTransform.gameObject.SetActive(false);
                    break;
                case MultiplayAssignment.StatusOptions.InProgress:
                    // Still wating...
                    Debug.Log("Still searching...");
                    break;
                case MultiplayAssignment.StatusOptions.Found:
                    createTicketResponse = null;
                    Debug.Log("Found Server! server IP : " + multiplayAssignment.Ip + ", Port : " + multiplayAssignment.Port);
                    string ipv4Address = multiplayAssignment.Ip;
                    ushort port = (ushort)multiplayAssignment.Port;
                    NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(ipv4Address, port);
                    GameMultiplayer.Instance.StartClient();
                    break;
                default:
                    break;
            }
        }
    }
}
