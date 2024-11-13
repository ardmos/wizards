using System;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// AI 마법사 Ruke의 서버 측 동작을 관리하는 클래스입니다.
/// </summary>
public class WizardRukeAIServer : NetworkBehaviour, ICharacter, ITargetable
{
    private const float MAX_DETECTION_DISTANCE = 12f;
    private const int MAX_SPELLS = 4;

    [SerializeField] private PlayerAnimator playerAnimator;
    [SerializeField] private WizardRukeAISpellManagerServer wizardRukeAISpellManager;
    [SerializeField] private WizardRukeAIClient wizardRukeAIClient;
    [SerializeField] private WizardRukeAIHPManagerServer wizardRukeAIHPManager;
    [SerializeField] private WizardRukeAIMovementManagerServer wizardRukeAIMovementManager;
    [SerializeField] private WizardRukeAIBattleManagerServer wizardRukeAIBattleManager;
    [SerializeField] private WizardRukeAIGameOverManagerServer wizardRukeAIGameOverManager;
    [SerializeField] private AIState currentState;
    [SerializeField] private AIStateMachine stateMachine;
    [SerializeField] private AITargetingSystem targetingSystem;

    private GameObject target;
    private ulong aiClientId;
    private PlayerGameState aiGameState;

    // ICharacter 인터페이스 구현
    public Character characterClass { get; set; }
    public sbyte hp { get; set; }
    public sbyte maxHp { get; set; }
    public float moveSpeed { get; set; }
    public SpellName[] spells { get; set; }

    private void Update()
    {
        if (!IsServer) return;
        if (stateMachine == null) return;
        if (aiGameState == PlayerGameState.GameOver) return;

        stateMachine.UpdateState();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        if (MultiplayerGameManager.Instance == null) return;

        MultiplayerGameManager.Instance.OnGameStateChanged += GameManager_OnGameStateChanged;
        MultiplayerGameManager.Instance.OnPlayerGameOver += GameManager_OnPlayerGameOver;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer) return;
        if (MultiplayerGameManager.Instance == null) return;

        MultiplayerGameManager.Instance.OnGameStateChanged -= GameManager_OnGameStateChanged;
        MultiplayerGameManager.Instance.OnPlayerGameOver -= GameManager_OnPlayerGameOver;
    }

    /// <summary>
    /// 게임 상태가 변경될 때 호출되는 이벤트 핸들러입니다.
    /// </summary>
    private void GameManager_OnGameStateChanged(object sender, EventArgs e)
    {
        if (MultiplayerGameManager.Instance == null) return;

        if (MultiplayerGameManager.Instance.IsGamePlaying())
        {
            // 새로운 게임 상태가 Playing이면 순찰을 시작합니다
            stateMachine.ChangeState(AIStateType.Patrol);
        }
    }

    /// <summary>
    /// 플레이어가 게임 오버될 때 호출되는 이벤트 핸들러입니다.
    /// </summary>
    private void GameManager_OnPlayerGameOver(object sender, PlayerGameOverEventArgs e)
    {
        // 게임오버된 플레이어가 현재 target라면 target을 초기화하고 재검색합니다.
        // 현재 target이 설정되어있지 않은 상태라면 작업을 해줄 필요 없습니다.
        if (!target) return;

        // 게임오버된 플레이어가 현재 타겟이었으면 타겟 초기화, 다시 검색 시작. 
        if (e.clientIDWhoGameOver == GetTargetClientID())
        {
            Debug.Log("Target 게임오버! 새로운 타겟을 검색합니다");
            target = null;
            stateMachine.ChangeState(AIStateType.Patrol);
        }
    }

    /// <summary>
    /// AI 플레이어를 초기화합니다.
    /// </summary>
    /// <param name="AIClientId">AI 플레이어의 클라이언트 ID</param>
    public void InitializeAIPlayerOnServer(ulong AIClientId)
    {
        if (CurrentPlayerDataManager.Instance == null) return;

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
        if (!ComponentsValidationCheck()) return;

        wizardRukeAIMovementManager.SetMoveSpeed(playerInGameData.moveSpeed);
        wizardRukeAIHPManager.InitPlayerHP(this);
        // 플레이어 닉네임UI & 메터리얼 초기화
        wizardRukeAIClient.InitializeAIClientRPC(playerInGameData.playerName.ToString());
        // 플레이어가 보유한 스킬 목록 저장
        wizardRukeAISpellManager.InitAIPlayerSpellInfoArrayOnServer(this.spells);
        wizardRukeAIGameOverManager.InitAIGameOverManager(this, wizardRukeAIClient);
        stateMachine = new AIStateMachine(this);
        stateMachine.Initialize(AIStateType.Idle);
        targetingSystem = new AITargetingSystem(MAX_DETECTION_DISTANCE, transform);
    }

    /// <summary>
    /// AI 컴포넌트들이 올바르게 할당되었는지 확인합니다.
    /// </summary>
    /// <returns>모든 컴포넌트가 할당되었으면 true, 그렇지 않으면 false</returns>
    private bool ComponentsValidationCheck()
    {
        bool checkResult = true;
        if (wizardRukeAIMovementManager == null) checkResult = false;
        if (wizardRukeAIHPManager == null) checkResult = false;
        if (wizardRukeAIClient == null) checkResult = false;
        if (wizardRukeAISpellManager == null) checkResult = false;
        if (wizardRukeAIGameOverManager == null) checkResult = false;

        return checkResult;
    }

    /// <summary>
    /// 타겟을 향해 이동합니다.
    /// </summary>
    public void MoveTowardsTarget()
    {
        if (target == null) return;
        if (wizardRukeAIMovementManager == null) return;

        wizardRukeAIMovementManager.MoveToTarget(target.transform);
    }

    /// <summary>
    /// 타겟을 공격합니다.
    /// </summary>
    public void AttackTarget()
    {
        if (target == null) return;
        if (wizardRukeAIBattleManager == null) return;

        wizardRukeAIBattleManager.Attack();
    }

    #region RPC
    /// <summary>
    /// 스크롤 사용 효과를 적용하는 VFX를 시작합니다.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void StartApplyScrollVFXServerRPC()
    {
        if (GameAssetsManager.Instance == null) return;
        GameObject vfxPrefab = GameAssetsManager.Instance.gameAssets.vfx_SpellUpgrade;
        if (vfxPrefab == null) return;

        GameObject vfxHeal = Instantiate(vfxPrefab, transform);
        if(vfxHeal.TryGetComponent<NetworkObject>(out NetworkObject networkObject))
        {
            networkObject.Spawn();
            vfxHeal.transform.SetParent(transform);
        }
    }
    #endregion

    /// <summary>
    /// AI 플레이어의 게임 오버 처리를 합니다.
    /// </summary>
    /// <param name="attackerClientID">공격한 클라이언트의 ID</param>
    public void GameOver(ulong attackerClientID)
    {
        if (aiGameState != PlayerGameState.Playing) return;
        if (wizardRukeAIGameOverManager == null) return;

        aiGameState = PlayerGameState.GameOver;
        wizardRukeAIGameOverManager.HandleGameOver(attackerClientID);
    }

    /// <summary>
    /// 현재 타겟의 클라이언트ID를 반환합니다.
    /// </summary>
    /// <returns>타겟의 클라이언트ID</returns>
    private ulong GetTargetClientID()
    {
        if (target == null) return 0;

        ulong targetClientID = 0;
        if (target.TryGetComponent<ICharacter>(out var character))
        {
            targetClientID = character.GetClientID();
        }

        return targetClientID;
    }

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
    /// 새로운 타겟을 설정합니다.
    /// </summary>
    /// <param name="newTarget">새로운 타겟 게임 오브젝트</param>
    public void SetTarget(GameObject newTarget) => target = newTarget;
    public GameObject GetTarget() => target;
    public PlayerGameState GetAIGameState() => aiGameState;
    public AIStateMachine GetStateMachine() => stateMachine;
    public AITargetingSystem GetTargetingSystem() => targetingSystem;
    public WizardRukeAIMovementManagerServer GetMovementManager() => wizardRukeAIMovementManager;
    public WizardRukeAIBattleManagerServer GetBattleManager() => wizardRukeAIBattleManager;
    public PlayerAnimator GetPlayerAnimator() => playerAnimator;
    public float GetMaxDetectionDistance() => MAX_DETECTION_DISTANCE;

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