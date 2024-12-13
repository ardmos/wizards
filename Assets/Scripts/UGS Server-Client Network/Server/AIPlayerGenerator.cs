using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Random = System.Random;

/// <summary>
/// AI �÷��̾��� ������ �����ϴ� ��ũ��Ʈ �Դϴ�.
/// Server������ �����մϴ�.
/// </summary>
public class AIPlayerGenerator : NetworkBehaviour, ICleanable
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
            SceneCleanupManager.RegisterCleanableObject(this);
        }
        else Destroy(gameObject);

        isAIPlayerAdded = false;
    }

    private void Update()
    {
        if (!IsServer) return;
        CheckFirstPlayerConnectionTime();
    }

    public override void OnDestroy()
    {
        SceneCleanupManager.UnregisterCleanableObject(this);
    }

    private void CheckFirstPlayerConnectionTime()
    {
        if (CurrentPlayerDataManager.Instance.GetCurrentPlayerCount() > 0 && !isAIPlayerAdded)
        {
            var currentPlayers = CurrentPlayerDataManager.Instance.GetCurrentPlayers();
            PlayerInGameData firstPlayer = currentPlayers[0];
            var passedTime = DateTime.Now - firstPlayer.connectionTime;

            // 10�ʰ� ������ ���ڸ��� AI�÷��̾�� ä���ݴϴ�
            if (passedTime > TimeSpan.FromSeconds(DEFAULT_AI_GENERATE_DELAY_TIME))
            {
                AddAIPlayers();
            }
        }
    }

    /// <summary>
    /// AI �÷��̾� �߰� ���� �޼���
    /// </summary>
    /// <param name="playerData"></param>
    private void AddAIPlayers()
    {    
        isAIPlayerAdded = true;

        ulong availablePlayerSlots = (ulong)(ConnectionApprovalHandler.MaxPlayers - NetworkManager.Singleton.ConnectedClients.Count);
        Debug.Log($"���� ������ �÷��̾� {NetworkManager.Singleton.ConnectedClients.Count}��. �ִ� {ConnectionApprovalHandler.MaxPlayers}���� {availablePlayerSlots}���� ���ڶ��ϴ�. ���ڶ���ŭ AI �÷��̾ �����մϴ�.");

        for (ulong aiClientId = FIRST_AI_PLAYER_CLIENT_ID; aiClientId < FIRST_AI_PLAYER_CLIENT_ID + availablePlayerSlots; aiClientId++)
        {
            // AI �÷��̾� ���� ����
            PlayerInGameData aiPlayerInGameData_WizardRuke = GenerateAIPlayerData(aiClientId);
            // ���� �÷��̾� ����Ʈ�� �߰�
            CurrentPlayerDataManager.Instance.AddPlayer(aiPlayerInGameData_WizardRuke);
            // �߰��� AI �÷��̾�� �����Ѱɷ� ��� ��Ű��
            GameMatchReadyManagerServer.Instance.SetAIPlayerReady(aiClientId);
        }
    }

    private PlayerInGameData GenerateAIPlayerData(ulong aiClientId)
    {
        ICharacter aiSpecifications = CharacterSpecifications.GetCharacter(Character.WizardAI);
        return new PlayerInGameData()
        {
            clientId = aiClientId,
            playerGameState = PlayerGameState.Playing,
            playerName = GenerateRandomAIPlayerName(),
            characterClass = aiSpecifications.characterClass,
            hp = aiSpecifications.hp,
            maxHp = aiSpecifications.maxHp,
            moveSpeed = aiSpecifications.moveSpeed,
            skillAttack1 = aiSpecifications.spells[0],
            skillAttack2 = aiSpecifications.spells[1],
            skillAttack3 = aiSpecifications.spells[2],
            skillDefence = aiSpecifications.spells[3],
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

    public void Cleanup()
    {
        Destroy(gameObject);
    }
}
