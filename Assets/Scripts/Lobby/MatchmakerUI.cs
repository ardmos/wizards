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


// 필요한 것
// 1. UGS 서버 이니셜라이즈 되기 전까지 버튼 비활성화 <- TitleScene에서 서버 이니셜라이즈 이미 한걸로 해결.
// 2. 매치메이킹 서버 설정
// 3. 클라이언트 설정

public class MatchmakerUI : MonoBehaviour
{
    // 내가 UGS 대쉬보드에서 설정해둔 큐 이름 
    public const string DEFAULT_QUEUE = "default-queue";

    // Find Match 버튼
    [SerializeField] private Button btnFindGame;
    [SerializeField] private Transform lookingForGameTransform;

    // UGS Ticket 응답 저장
    private CreateTicketResponse createTicketResponse;
    // UGS doc 권장을 따름 (ticket poll은 1초 이상이어야 한다)
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
            // ticket이 있을 경우 1.1초마다 poll ticket 요청
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
            // Extra Data 현재는 매칭 풀 나누는데 필요한 Skill 값만 보내고 있다. UGS 대쉬보드에서 입력해둔 변수명 'Skill'
            new MatchmakingPlayerData
            {
                Skill = 100,
            })
        }, new CreateTicketOptions { QueueName = DEFAULT_QUEUE });

        // 잠깐 기다렸다 poll 한다
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
            // 업데이트된 내용이 없음. 계속 기다린다
            Debug.Log("keep wating");
            return;
        }

        // 업데이트된 내용이 있음. 업데이트된 ticktStatusResponse의 타입을 확인한다.
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
