using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class WizardRukeAIServer : NetworkBehaviour, ICharacter
{
    private const int DEFAULT_SCORE = 300;

    [SerializeField] private PlayerAnimator playerAnimator;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Collider aiCollider;
    [SerializeField] private WizardRukeAISpellManagerServer wizardRukeAISpellManager;
    [SerializeField] private WizardRukeAIClient wizardRukeAIClient;
    [SerializeField] private WizardRukeAIHPManagerServer wizardRukeAIHPManager;
    [SerializeField] private WizardRukeAIMovementSystemServer wizardRukeAIMovementSystem;
    [SerializeField] private WizardRukeAIBattleSystemServer wizardRukeAIBattleSystem;
    [SerializeField] private WizardRukeAIItemDropManagerServer wizardRukeAIItemDropManager;
    [SerializeField] private AIState currentState;
    [SerializeField] private AIStateMachine stateMachine;
    [SerializeField] private AITargetingSystem targetingSystem;

    private float maxDetectionDistance = 12f;
    private float attackRange = 8f;
    private float attackCooldown = 2f;
    private float lastAttackTime = 0f;
    private GameObject target;
    private ulong aiClientId;
    private PlayerGameState aiGameState;

    // ICharacter 인터페이스 구현
    public Character characterClass { get; set; }
    public sbyte hp { get; set; }
    public sbyte maxHp { get; set; }
    public float moveSpeed { get; set; }
    public SpellName[] skills { get; set; } = new SpellName[]{
                SpellName.FireBallLv1,
                SpellName.WaterBallLv1,
                SpellName.BlizzardLv1,
                SpellName.MagicShieldLv1
                };

    private void Update()
    {
        if (!IsServer) return;
        if (aiGameState == PlayerGameState.GameOver) return;

        stateMachine.UpdateState();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        MultiplayerGameManager.Instance.OnGameStateChanged += GameManager_OnGameStateChanged;
        MultiplayerGameManager.Instance.OnPlayerGameOver += GameManager_OnPlayerGameOver;
    }

    public override void OnNetworkDespawn()
    {
        MultiplayerGameManager.Instance.OnGameStateChanged -= GameManager_OnGameStateChanged;
        MultiplayerGameManager.Instance.OnPlayerGameOver -= GameManager_OnPlayerGameOver;
    }

    private void GameManager_OnGameStateChanged(object sender, EventArgs e)
    {
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
        if(!target) return;

        // 게임오버된 플레이어가 현재 타겟이었으면 타겟 초기화, 다시 검색 시작. 
        if (GetTargetClientID() == e.clientIDWhoGameOver)
        {
            Debug.Log("Target 게임오버! 새로운 타겟을 검색합니다");
            target = null;
            stateMachine.ChangeState(AIStateType.Patrol);
        }
    }

    private ulong GetTargetClientID()
    {
        ulong targetClientID = 0;
        // AI인 경우
        if (target.TryGetComponent<WizardRukeAIServer>(out WizardRukeAIServer wizardRukeAIServer))
        {
            targetClientID = wizardRukeAIServer.aiClientId;
        }
        // Player인 경우
        else if (target.TryGetComponent<PlayerServer>(out PlayerServer playerServer))
        {
            targetClientID = playerServer.OwnerClientId;
        }

        return targetClientID;
    }

    /// <summary>
    /// AI플레이어를 초기화해줍니다
    /// </summary>
    /// <param name="AIClientId"></param>
    public void InitializeAIPlayerOnServer(ulong AIClientId)
    {
        aiGameState = PlayerGameState.Playing;

        if (GameAssetsManager.Instance == null) return;

        PlayerInGameData playerInGameData = CurrentPlayerDataManager.Instance.GetPlayerDataByClientId(AIClientId);
        SetCharacterData(playerInGameData);
        InitializeAISystems(playerInGameData);
    }

    private void InitializeAISystems(PlayerInGameData playerInGameData)
    {
        // NavMesh를 사용하기 대문에 속도 설정을 따로 챙겨줍니다
        wizardRukeAIMovementSystem.SetMoveSpeed(playerInGameData.moveSpeed);
        // HP 초기화. 현재 HP 설정 및 UI에 반영
        wizardRukeAIHPManager.InitPlayerHP(this);
        // 플레이어 닉네임UI & 메터리얼 초기화
        wizardRukeAIClient.InitializeAIClientRPC(playerInGameData.playerName.ToString());
        // 플레이어가 보유한 스킬 목록 저장
        wizardRukeAISpellManager.InitAIPlayerSpellInfoArrayOnServer(this.skills);
        stateMachine = new AIStateMachine(this);
        stateMachine.Initialize(AIStateType.Idle);
        targetingSystem = new AITargetingSystem(maxDetectionDistance, transform);
    }

    public void MoveTowardsTarget()
    {
        if (!target) return;

        wizardRukeAIMovementSystem.MoveToTarget(target.transform);

        // 이동 애니메이션 실행
        playerAnimator.UpdatePlayerAnimationOnServer(PlayerMoveAnimState.Walking);
    }

    public void AttackTarget()
    {
        if (Time.time > lastAttackTime + attackCooldown)
        {
            wizardRukeAIBattleSystem.Attack();
            lastAttackTime = Time.time;
        }
    }

    public sbyte GetPlayerHP()
    {
        return wizardRukeAIHPManager.GetHP();
    }

    // 스크롤 활용. 스킬 강화 VFX 실행
    [ServerRpc(RequireOwnership = false)]
    public void StartApplyScrollVFXServerRPC()
    {
        GameObject vfxHeal = Instantiate(GameAssetsManager.Instance.gameAssets.vfx_SpellUpgrade, transform);
        vfxHeal.GetComponent<NetworkObject>().Spawn();
        vfxHeal.transform.SetParent(transform);
    }

    public ICharacter GetCharacterData()
    {
        return this;
    }

    public void SetCharacterData(PlayerInGameData characterData)
    {
        this.aiClientId = characterData.clientId;
        this.characterClass = characterData.characterClass;
        this.hp = characterData.hp;
        this.maxHp = characterData.maxHp;
        this.moveSpeed = characterData.moveSpeed;
    }

    public void GameOver(ulong clientWhoAttacked)
    {
        if (aiGameState != PlayerGameState.Playing) return;
        aiGameState = PlayerGameState.GameOver;
        
        // 추적 멈추기
        wizardRukeAIMovementSystem.StopMove();
        // 물리충돌 해제
        rb.isKinematic = true;
        aiCollider.enabled = false;
        // 플레이어 이름 & HP UI off
        wizardRukeAIClient.OffPlayerUIClientRPC();
        // 유도당하지 않도록 Tag 변경
        tag = "GameOver";
        Debug.Log($"AI Player{aiClientId} is GameOver, new tag : {tag}");

        // 점수 계산
        CalcScore(clientWhoAttacked);
        // GameOver 애니메이션 실행
        playerAnimator.UpdatePlayerAnimationOnServer(PlayerMoveAnimState.GameOver);
        // 게임오버 플레이어 사실을 서버에 기록.
        MultiplayerGameManager.Instance.UpdatePlayerGameOverOnServer(aiClientId, clientWhoAttacked);

        // 아이템 드랍
        wizardRukeAIItemDropManager.DropItem(transform.position);
    }

    private void CalcScore(ulong clientWhoAttacked)
    {
        // 스스로 게임오버 당한 경우, 게임 내 모든 플레이어들에게 점수를 줍니다. 
        if (clientWhoAttacked == OwnerClientId)
        {
            foreach (PlayerInGameData playerInGameData in CurrentPlayerDataManager.Instance.GetCurrentPlayers())
            {
                CurrentPlayerDataManager.Instance.AddPlayerScore(playerInGameData.clientId, DEFAULT_SCORE);
            }
        }
        // 일반적인 경우 상대 플레이어 300스코어 획득
        else
        {
            CurrentPlayerDataManager.Instance.AddPlayerScore(clientWhoAttacked, DEFAULT_SCORE);
        }
    }

    public void SetTarget(GameObject newTarget) => target = newTarget;
    public GameObject GetTarget() => target;

    public void SetAIClientId(ulong aiClientId) => this.aiClientId = aiClientId;
    public ulong GetAIClientId() => aiClientId;

    public void SetAIGameState(PlayerGameState gameState) => aiGameState = gameState;
    public PlayerGameState GetAIGameState() => aiGameState;

    public AIStateMachine GetStateMachine() => stateMachine;
    public AITargetingSystem GetTargetingSystem() => targetingSystem;
    public WizardRukeAIMovementSystemServer GetMovementSystem() => wizardRukeAIMovementSystem;
    public float GetAttackRange() => attackRange;
    public float GetMaxDetectionDistance() => maxDetectionDistance;
}

