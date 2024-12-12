using System;
using Unity.Netcode;
using UnityEngine;
using static ComponentValidator;


/// <summary>
/// AI 마법사 Ruke의 서버 측 동작을 관리하는 클래스입니다.
/// </summary>
public class WizardRukeAIServer : NetworkBehaviour, ICharacter, ITargetable
{
    #region Constants & Fields
    // 에러 메시지 상수들
    private const string ERROR_STATE_MACHINE_NOT_SET = "WizardRukeAIServer stateMachine 설정이 안되어있습니다.";
    private const string ERROR_MULTIPLAYER_GAME_MANAGER_NOT_SET = "WizardRukeAIServer MultiplayerGameManager.Instance 설정이 안되어있습니다.";
    private const string ERROR_TARGET_NOT_SET = "WizardRukeAIServer target 설정이 안되어있습니다.";
    private const string ERROR_CURRENT_PLAYER_DATA_MANAGER_NOT_SET = "WizardRukeAIServer CurrentPlayerDataManager.Instance 설정이 안되어있습니다.";
    private const string ERROR_MOVEMENT_MANAGER_NOT_SET = "WizardRukeAIServer wizardRukeAIMovementManager 설정이 안되어있습니다.";
    private const string ERROR_BATTLE_MANAGER_NOT_SET = "WizardRukeAIServer wizardRukeAIBattleManager 설정이 안되어있습니다.";
    private const string ERROR_GAME_ASSET_MANAGER_NOT_SET = "WizardRukeAIServer GameAssetsManager.Instance 설정이 안되어있습니다.";
    private const string ERROR_VFX_PREFAB_NOT_SET = "WizardRukeAIServer vfxPrefab 설정이 안되어있습니다.";
    private const string ERROR_GAME_OVER_MANAGER_NOT_SET = "WizardRukeAIServer wizardRukeAIGameOverManager 설정이 안되어있습니다.";
    private const string ERROR_HP_MANAGER_NOT_SET = "WizardRukeAIServer wizardRukeAIHPManager 설정이 안되어있습니다.";
    private const string ERROR_CLIENT_NOT_SET = "WizardRukeAIServer wizardRukeAIClient 설정이 안되어있습니다.";
    private const string ERROR_SPELL_MANAGER_NOT_SET = "WizardRukeAIServer wizardRukeAISpellManager 설정이 안되어있습니다.";

    private const float MAX_DETECTION_DISTANCE = 12f;
    private const int MAX_SPELLS = 4;

    [SerializeField] private PlayerAnimator playerAnimator;
    [SerializeField] private WizardRukeAISpellManagerServer wizardRukeAISpellManager;
    [SerializeField] private WizardRukeAIClient wizardRukeAIClient;
    [SerializeField] private WizardRukeAIHPManagerServer wizardRukeAIHPManager;
    [SerializeField] private WizardRukeAIMovementManagerServer wizardRukeAIMovementManager;
    [SerializeField] private WizardRukeAIBattleManagerServer wizardRukeAIBattleManager;
    [SerializeField] private WizardRukeAIGameOverManagerServer wizardRukeAIGameOverManager;

    private AIStateMachine stateMachine;
    private AITargetingSystem targetingSystem;
    private GameObject target;
    private ulong aiClientId;
    private PlayerGameState aiGameState;
    #endregion

    #region ICharacter Properties
    public Character characterClass { get; set; }
    public sbyte hp { get; set; }
    public sbyte maxHp { get; set; }
    public float moveSpeed { get; set; }
    public SpellName[] spells { get; set; }
    #endregion

    #region Unity Lifecycle
    /// <summary>
    /// AI stateMachine을 매 프레임 업데이트 시킵니다.
    /// </summary>

    private void Update()
    {
        if (!IsServer) return;
        if (!ValidateComponent(stateMachine, ERROR_STATE_MACHINE_NOT_SET)) return;
        if (aiGameState == PlayerGameState.GameOver) return;

        stateMachine.UpdateState();
    }

    /// <summary>
    /// 네트워크 객체가 스폰될 때 호출되는 메서드입니다.
    /// </summary>
    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        if (!ValidateComponent(MultiplayerGameManager.Instance, ERROR_MULTIPLAYER_GAME_MANAGER_NOT_SET)) return;

        MultiplayerGameManager.Instance.OnGameStateChanged += GameManager_OnGameStateChanged;
        MultiplayerGameManager.Instance.OnPlayerGameOver += GameManager_OnPlayerGameOver;
    }

    /// <summary>
    /// 네트워크 객체가 디스폰될 때 호출되는 메서드입니다.
    /// </summary>
    public override void OnNetworkDespawn()
    {
        if (!IsServer) return;
        if (!ValidateComponent(MultiplayerGameManager.Instance, ERROR_MULTIPLAYER_GAME_MANAGER_NOT_SET)) return;

        MultiplayerGameManager.Instance.OnGameStateChanged -= GameManager_OnGameStateChanged;
        MultiplayerGameManager.Instance.OnPlayerGameOver -= GameManager_OnPlayerGameOver;
    }
    #endregion

    #region Initializations
    /// <summary>
    /// AI 플레이어를 초기화합니다.
    /// </summary>
    /// <param name="AIClientId">AI 플레이어의 클라이언트 ID</param>
    public void InitializeAIPlayerOnServer(ulong AIClientId)
    {
        if (!ValidateComponent(CurrentPlayerDataManager.Instance, ERROR_CURRENT_PLAYER_DATA_MANAGER_NOT_SET)) return;

        PlayerInGameData playerData = CurrentPlayerDataManager.Instance.GetPlayerDataByClientId(AIClientId);
        SetAIPlayerData(playerData);
        InitializeAIComponents(playerData);
        aiGameState = PlayerGameState.Playing;
    }

    /// <summary>
    /// AI 컴포넌트들을 초기화합니다.
    /// </summary>
    /// <param name="playerInGameData">플레이어 인게임 데이터</param>
    private void InitializeAIComponents(PlayerInGameData playerInGameData)
    {
        if (!ValidateAIComponents()) return;

        InitializeClient(playerInGameData);
        InitializeManagers(playerInGameData);
        InitializeStateMachineAndTargetingSystem();
    }

    /// <summary>
    /// AI 클라이언트를 초기화합니다.
    /// </summary>
    /// <param name="playerInGameData">플레이어 인게임 데이터</param>
    private void InitializeClient(PlayerInGameData playerInGameData)
    {
        wizardRukeAIClient.InitializeAIClientRPC(playerInGameData.playerName.ToString());
    }

    /// <summary>
    /// AI 매니저들을 초기화합니다.
    /// </summary>
    /// <param name="playerInGameData">플레이어 인게임 데이터</param>
    private void InitializeManagers(PlayerInGameData playerInGameData)
    {
        wizardRukeAIMovementManager.SetMoveSpeed(playerInGameData.moveSpeed);
        wizardRukeAIHPManager.InitPlayerHP(this);
        wizardRukeAISpellManager.InitAIPlayerSpellInfoArrayOnServer(this.spells);
        wizardRukeAIGameOverManager.InitAIGameOverManager(this, wizardRukeAIClient);
    }

    /// <summary>
    /// 상태 머신과 타겟팅 시스템을 초기화합니다.
    /// </summary>
    private void InitializeStateMachineAndTargetingSystem()
    {
        stateMachine = new AIStateMachine(this);
        stateMachine.InitializeStateMachine(AIStateType.Idle);
        targetingSystem = new AITargetingSystem(MAX_DETECTION_DISTANCE, transform, this);
    }
    #endregion

    #region AI Player Data Management
    /// <summary>
    /// AI 플레이어 데이터를 설정합니다.
    /// </summary>
    /// <param name="playerData">플레이어 데이터</param>
    public void SetAIPlayerData(PlayerInGameData playerData)
    {
        this.aiClientId = playerData.clientId;
        ICharacter aiCharacterData = CharacterSpecifications.GetCharacter(playerData.characterClass);
        SetCharacterData(aiCharacterData);
    }

    /// <summary>
    /// AI의 최대 감지 거리를 반환합니다.
    /// </summary>
    /// <returns>최대 감지 거리</returns>
    public float GetMaxDetectionDistance() => MAX_DETECTION_DISTANCE;
    #endregion

    #region AI Targeting Management
    /// <summary>
    /// 새로운 타겟을 설정합니다.
    /// </summary>
    /// <param name="newTarget">새로운 타겟 게임 오브젝트</param>
    public void SetTarget(GameObject newTarget) => target = newTarget;

    /// <summary>
    /// 현재 AI의 타겟을 반환합니다.
    /// </summary>
    /// <returns>현재 타겟 게임 오브젝트</returns>
    public GameObject GetTarget() => target;

    /// <summary>
    /// AI의 타겟팅 시스템을 반환합니다.
    /// </summary>
    /// <returns>AI 타겟팅 시스템</returns>
    public AITargetingSystem GetTargetingSystem() => targetingSystem;

    /// <summary>
    /// 현재 타겟의 클라이언트ID를 반환합니다.
    /// </summary>
    /// <returns>타겟의 클라이언트ID</returns>
    private ulong GetTargetClientID()
    {
        ulong targetClientID = 0;
        if (target.TryGetComponent<ICharacter>(out var character))
        {
            targetClientID = character.GetClientID();
        }

        return targetClientID;
    }
    #endregion

    #region AI State Management
    /// <summary>
    /// AI 플레이어의 게임 오버 처리를 합니다.
    /// </summary>
    /// <param name="attackerClientID">공격한 클라이언트의 ID</param>
    public void GameOver(ulong attackerClientID)
    {
        if (aiGameState != PlayerGameState.Playing) return;
        if (!ValidateComponent(wizardRukeAIGameOverManager, ERROR_GAME_OVER_MANAGER_NOT_SET)) return;

        aiGameState = PlayerGameState.GameOver;
        wizardRukeAIGameOverManager.HandleGameOver(attackerClientID);
    }

    /// <summary>
    /// AI의 현재 게임 상태를 반환합니다.
    /// </summary>
    /// <returns>AI의 현재 게임 상태</returns>
    public PlayerGameState GetAIGameState() => aiGameState;

    /// <summary>
    /// AI의 상태 머신을 반환합니다.
    /// </summary>
    /// <returns>AI 상태 머신</returns>
    public AIStateMachine GetStateMachine() => stateMachine;
    #endregion

    #region AI Movement Management
    /// <summary>
    /// 타겟을 향해 이동합니다.
    /// </summary>
    public void MoveTowardsTarget()
    {
        if (!ValidateComponent(target, ERROR_TARGET_NOT_SET)) return;
        if (!ValidateComponent(wizardRukeAIMovementManager, ERROR_MOVEMENT_MANAGER_NOT_SET)) return;

        wizardRukeAIMovementManager.MoveToTarget(target.transform);
    }

    /// <summary>
    /// AI의 이동 매니저를 반환합니다.
    /// </summary>
    /// <returns>AI 이동 매니저</returns>
    public WizardRukeAIMovementManagerServer GetMovementManager() => wizardRukeAIMovementManager;
    #endregion

    #region AI Attack Management
    /// <summary>
    /// 타겟을 공격합니다.
    /// </summary>
    public void AttackTarget()
    {
        if (!ValidateComponent(target, ERROR_TARGET_NOT_SET)) return;
        if (!ValidateComponent(wizardRukeAIBattleManager, ERROR_BATTLE_MANAGER_NOT_SET)) return;

        wizardRukeAIBattleManager.Attack();
    }
    /// <summary>
    /// AI의 전투 매니저를 반환합니다.
    /// </summary>
    /// <returns>AI 전투 매니저</returns>
    public WizardRukeAIBattleManagerServer GetBattleManager() => wizardRukeAIBattleManager;
    #endregion

    #region AI Animator Management
    /// <summary>
    /// AI의 플레이어 애니메이터를 반환합니다.
    /// </summary>
    /// <returns>플레이어 애니메이터</returns>
    public PlayerAnimator GetPlayerAnimator() => playerAnimator;
    #endregion

    #region AI VFX Control
    /// <summary>
    /// 스크롤 사용 효과를 적용하는 VFX를 시작합니다.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void StartApplyScrollVFXServerRPC()
    {
        if (!ValidateComponent(GameAssetsManager.Instance, ERROR_GAME_ASSET_MANAGER_NOT_SET)) return;
        GameObject vfxPrefab = GameAssetsManager.Instance.gameAssets.vfx_SpellUpgrade;
        if (!ValidateComponent(vfxPrefab, ERROR_VFX_PREFAB_NOT_SET)) return;

        GameObject vfxHeal = Instantiate(vfxPrefab, transform);
        if (vfxHeal.TryGetComponent<NetworkObject>(out NetworkObject networkObject))
        {
            networkObject.Spawn();
            vfxHeal.transform.SetParent(transform);
        }
    }
    #endregion

    #region Valitation Check
    /// <summary>
    /// AI 컴포넌트들이 올바르게 할당되었는지 확인합니다.
    /// </summary>
    /// <returns>모든 컴포넌트가 할당되었으면 true, 그렇지 않으면 false</returns>
    private bool ValidateAIComponents()
    {
        return ValidateComponent(wizardRukeAIMovementManager, ERROR_MOVEMENT_MANAGER_NOT_SET) &&
          ValidateComponent(wizardRukeAIHPManager, ERROR_HP_MANAGER_NOT_SET) &&
          ValidateComponent(wizardRukeAIClient, ERROR_CLIENT_NOT_SET) &&
          ValidateComponent(wizardRukeAISpellManager, ERROR_SPELL_MANAGER_NOT_SET) &&
          ValidateComponent(wizardRukeAIGameOverManager, ERROR_GAME_OVER_MANAGER_NOT_SET);
    }
    #endregion

    #region Event Handlers
    /// <summary>
    /// 게임 상태가 변경될 때 호출되는 이벤트 핸들러입니다.
    /// </summary>
    private void GameManager_OnGameStateChanged(object sender, EventArgs e)
    {
        if (!ValidateComponent(MultiplayerGameManager.Instance, ERROR_MULTIPLAYER_GAME_MANAGER_NOT_SET)) return;
        if (!ValidateComponent(stateMachine, ERROR_STATE_MACHINE_NOT_SET)) return;

        // 새로운 게임 상태가 Playing이면 순찰을 시작합니다
        if (MultiplayerGameManager.Instance.IsGamePlaying()) stateMachine.ChangeState(AIStateType.Patrol);
    }

    /// <summary>
    /// 플레이어가 게임 오버될 때 호출되는 이벤트 핸들러입니다.
    /// </summary>
    private void GameManager_OnPlayerGameOver(object sender, PlayerGameOverEventArgs e)
    {
        // 게임오버된 플레이어가 현재 target라면 target을 초기화하고 재검색합니다.
        // 현재 target이 설정되어있지 않은 상태라면 작업을 해줄 필요 없습니다.
        if (!ValidateComponent(target, ERROR_TARGET_NOT_SET)) return;
        if (!ValidateComponent(stateMachine, ERROR_STATE_MACHINE_NOT_SET)) return;

        // 게임오버된 플레이어가 현재 타겟이었으면 타겟 초기화, 다시 검색 시작.
        if (e.clientIDWhoGameOver == GetTargetClientID())
        {
            Logger.Log("Target 게임오버! 새로운 타겟을 검색합니다");
            target = null;
            stateMachine.ChangeState(AIStateType.Patrol);
        }
    }
    #endregion

    #region ICharacter 구현
    public ICharacter GetCharacterData() => this;
    public void SetCharacterData(ICharacter character)
    {
        characterClass = character.characterClass;
        hp = character.hp;
        maxHp = character.maxHp;
        moveSpeed = character.moveSpeed;
        spells = new SpellName[MAX_SPELLS];
        Array.Copy(character.spells, spells, Math.Min(character.spells.Length, MAX_SPELLS));
    }
    public ulong GetClientID() => aiClientId;
    #endregion

    #region ITargetable 구현
    public float GetHP() => hp;
    public GameObject GetGameObject() => gameObject;
    #endregion
}