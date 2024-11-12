using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class WizardRukeAIServer : NetworkBehaviour, ICharacter
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

    private void GameManager_OnGameStateChanged(object sender, EventArgs e)
    {
        if (MultiplayerGameManager.Instance == null) return;

        if (MultiplayerGameManager.Instance.IsGamePlaying())
        {
            // 이제 카운트다운은 끝! 게임 시작! 순찰을 시작합니다!
            stateMachine.ChangeState(AIStateType.Patrol);
        }
    }

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
    /// AI플레이어를 초기화해줍니다
    /// </summary>
    /// <param name="AIClientId"></param>
    public void InitializeAIPlayerOnServer(ulong AIClientId)
    {
        aiGameState = PlayerGameState.Playing;

        if (CurrentPlayerDataManager.Instance == null) return;

        PlayerInGameData playerData = CurrentPlayerDataManager.Instance.GetPlayerDataByClientId(AIClientId);
        SetAIPlayerData(playerData);
        InitializeAIComponents(playerData);
    }

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

    private bool ComponentsValidationCheck()
    {
        bool checkResault = true;
        if (wizardRukeAIMovementManager == null) checkResault = false;
        if (wizardRukeAIHPManager == null) checkResault = false;
        if (wizardRukeAIClient == null) checkResault = false;
        if (wizardRukeAISpellManager == null) checkResault = false;
        if (wizardRukeAIGameOverManager == null) checkResault = false;

        return checkResault;
    }

    public void MoveTowardsTarget()
    {
        if (target == null) return;
        if (wizardRukeAIMovementManager == null) return;

        wizardRukeAIMovementManager.MoveToTarget(target.transform);
    }

    public void AttackTarget()
    {
        if (target == null) return;
        if (wizardRukeAIBattleManager == null) return;

        wizardRukeAIBattleManager.Attack();
    }

    // 스크롤 활용. 스킬 강화 VFX 실행
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

    public void GameOver(ulong clientWhoAttacked)
    {
        if (aiGameState != PlayerGameState.Playing) return;
        if (wizardRukeAIGameOverManager == null) return;

        aiGameState = PlayerGameState.GameOver;
        wizardRukeAIGameOverManager.HandleGameOver(clientWhoAttacked);
    }

    public void SetAIPlayerData(PlayerInGameData playerData)
    {
        this.aiClientId = playerData.clientId;
        ICharacter aiCharacterData = CharacterSpecifications.GetCharacter(playerData.characterClass); 
        SetCharacterData(aiCharacterData);
    }
    public void SetTarget(GameObject newTarget) => target = newTarget;
    public GameObject GetTarget() => target;
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

    public PlayerGameState GetAIGameState() => aiGameState;
    public AIStateMachine GetStateMachine() => stateMachine;
    public AITargetingSystem GetTargetingSystem() => targetingSystem;
    public WizardRukeAIMovementManagerServer GetMovementManager() => wizardRukeAIMovementManager;
    public WizardRukeAIBattleManagerServer GetBattleManager() => wizardRukeAIBattleManager;
    public PlayerAnimator GetPlayerAnimator() => playerAnimator;
    public float GetMaxDetectionDistance() => MAX_DETECTION_DISTANCE;
    public sbyte GetPlayerHP() => wizardRukeAIHPManager.GetHP();

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
}