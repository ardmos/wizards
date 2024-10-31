using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Random = System.Random;

/// <summary>
/// AI 플레이어의 생성을 관리하는 스크립트 입니다.
/// Server측에서 동작합니다.
/// </summary>
public class AIPlayerGenerator : NetworkBehaviour
{
    private const ulong FIRST_AI_PLAYER_CLIENT_ID = 10000;
    private const ulong DEFAULT_AI_GENERATE_DELAY_TIME = 10;

    public static AIPlayerGenerator Instance { get; private set; }

    [SerializeField] private bool isAIPlayerAdded;  

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);

        isAIPlayerAdded = false;
    }

    private void Update()
    {
        if (!IsServer) return;
        CheckFirstPlayerConnectionTime();
    }

    private void CheckFirstPlayerConnectionTime()
    {
        if (CurrentPlayerDataManager.Instance.GetCurrentPlayerCount() > 0 && !isAIPlayerAdded)
        {
            var currentPlayers = CurrentPlayerDataManager.Instance.GetCurrentPlayers();
            PlayerInGameData firstPlayer = currentPlayers[0];
            var passedTime = DateTime.Now - firstPlayer.connectionTime;

            // 10초가 지나면 빈자리는 AI플레이어로 채워줍니다
            if (passedTime > TimeSpan.FromSeconds(DEFAULT_AI_GENERATE_DELAY_TIME))
            {
                AddAIPlayers();
            }
        }
    }

    /// <summary>
    /// AI 플레이어 추가 전용 메서드
    /// </summary>
    /// <param name="playerData"></param>
    private void AddAIPlayers()
    {    
        isAIPlayerAdded = true;

        ulong availablePlayerSlots = (ulong)(ConnectionApprovalHandler.MaxPlayers - NetworkManager.Singleton.ConnectedClients.Count);
        Debug.Log($"현재 접속한 플레이어 {NetworkManager.Singleton.ConnectedClients.Count}명. 최대 {ConnectionApprovalHandler.MaxPlayers}명에서 {availablePlayerSlots}명이 모자랍니다. 모자란만큼 AI 플레이어를 생성합니다.");

        for (ulong aiClientId = FIRST_AI_PLAYER_CLIENT_ID; aiClientId < FIRST_AI_PLAYER_CLIENT_ID + availablePlayerSlots; aiClientId++)
        {
            // AI 플레이어 정보 생성
            PlayerInGameData aiPlayerInGameData_WizardRuke = GenerateAIPlayerData(aiClientId);
            // 현재 플레이어 리스트에 추가
            CurrentPlayerDataManager.Instance.AddPlayer(aiPlayerInGameData_WizardRuke);
            // 추가한 AI 플레이어들 레디한걸로 등록 시키기
            GameMatchReadyManagerServer.Instance.SetAIPlayerReady(aiClientId);
        }
    }

    private PlayerInGameData GenerateAIPlayerData(ulong aiClientId)
    {
        return new PlayerInGameData()
        {
            clientId = aiClientId,
            playerGameState = PlayerGameState.Playing,
            playerName = GenerateRandomAIPlayerName(),
            characterClass = Character.Wizard,
            hp = 5,
            maxHp = 5,
            moveSpeed = 4,
            isAI = true,
        };
    }

    private string GenerateRandomAIPlayerName()
    {
        AIPlayerName randomName = GetRandomEnumValue<AIPlayerName>();
        AIPlayerTitle randomTitle = GetRandomEnumValue<AIPlayerTitle>();

        return $"{randomTitle} {randomName}";
    }

    private T GetRandomEnumValue<T>()
    {
        Array values = Enum.GetValues(typeof(T));
        Random random = new Random();
        return (T)values.GetValue(random.Next(values.Length));
    }

    public void ResetAIPlayerGenerator()
    {
        isAIPlayerAdded = false;
    }
}
