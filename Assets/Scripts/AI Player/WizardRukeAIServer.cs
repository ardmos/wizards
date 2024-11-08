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

    // ICharacter �������̽� ����
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
            // ���� ī��Ʈ�ٿ��� ��! ���� ����! ������ �����մϴ�!
            stateMachine.ChangeState(AIStateType.Patrol);
        }
    }

    private void GameManager_OnPlayerGameOver(object sender, PlayerGameOverEventArgs e)
    {
        // ���ӿ����� �÷��̾ ���� target��� target�� �ʱ�ȭ�ϰ� ��˻��մϴ�.
        // ���� target�� �����Ǿ����� ���� ���¶�� �۾��� ���� �ʿ� �����ϴ�.
        if(!target) return;

        // ���ӿ����� �÷��̾ ���� Ÿ���̾����� Ÿ�� �ʱ�ȭ, �ٽ� �˻� ����. 
        if (GetTargetClientID() == e.clientIDWhoGameOver)
        {
            Debug.Log("Target ���ӿ���! ���ο� Ÿ���� �˻��մϴ�");
            target = null;
            stateMachine.ChangeState(AIStateType.Patrol);
        }
    }

    private ulong GetTargetClientID()
    {
        ulong targetClientID = 0;
        // AI�� ���
        if (target.TryGetComponent<WizardRukeAIServer>(out WizardRukeAIServer wizardRukeAIServer))
        {
            targetClientID = wizardRukeAIServer.aiClientId;
        }
        // Player�� ���
        else if (target.TryGetComponent<PlayerServer>(out PlayerServer playerServer))
        {
            targetClientID = playerServer.OwnerClientId;
        }

        return targetClientID;
    }

    /// <summary>
    /// AI�÷��̾ �ʱ�ȭ���ݴϴ�
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
        // NavMesh�� ����ϱ� �빮�� �ӵ� ������ ���� ì���ݴϴ�
        wizardRukeAIMovementSystem.SetMoveSpeed(playerInGameData.moveSpeed);
        // HP �ʱ�ȭ. ���� HP ���� �� UI�� �ݿ�
        wizardRukeAIHPManager.InitPlayerHP(this);
        // �÷��̾� �г���UI & ���͸��� �ʱ�ȭ
        wizardRukeAIClient.InitializeAIClientRPC(playerInGameData.playerName.ToString());
        // �÷��̾ ������ ��ų ��� ����
        wizardRukeAISpellManager.InitAIPlayerSpellInfoArrayOnServer(this.skills);
        stateMachine = new AIStateMachine(this);
        stateMachine.Initialize(AIStateType.Idle);
        targetingSystem = new AITargetingSystem(maxDetectionDistance, transform);
    }

    public void MoveTowardsTarget()
    {
        if (!target) return;

        wizardRukeAIMovementSystem.MoveToTarget(target.transform);

        // �̵� �ִϸ��̼� ����
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

    // ��ũ�� Ȱ��. ��ų ��ȭ VFX ����
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
        
        // ���� ���߱�
        wizardRukeAIMovementSystem.StopMove();
        // �����浹 ����
        rb.isKinematic = true;
        aiCollider.enabled = false;
        // �÷��̾� �̸� & HP UI off
        wizardRukeAIClient.OffPlayerUIClientRPC();
        // ���������� �ʵ��� Tag ����
        tag = "GameOver";
        Debug.Log($"AI Player{aiClientId} is GameOver, new tag : {tag}");

        // ���� ���
        CalcScore(clientWhoAttacked);
        // GameOver �ִϸ��̼� ����
        playerAnimator.UpdatePlayerAnimationOnServer(PlayerMoveAnimState.GameOver);
        // ���ӿ��� �÷��̾� ����� ������ ���.
        MultiplayerGameManager.Instance.UpdatePlayerGameOverOnServer(aiClientId, clientWhoAttacked);

        // ������ ���
        wizardRukeAIItemDropManager.DropItem(transform.position);
    }

    private void CalcScore(ulong clientWhoAttacked)
    {
        // ������ ���ӿ��� ���� ���, ���� �� ��� �÷��̾�鿡�� ������ �ݴϴ�. 
        if (clientWhoAttacked == OwnerClientId)
        {
            foreach (PlayerInGameData playerInGameData in CurrentPlayerDataManager.Instance.GetCurrentPlayers())
            {
                CurrentPlayerDataManager.Instance.AddPlayerScore(playerInGameData.clientId, DEFAULT_SCORE);
            }
        }
        // �Ϲ����� ��� ��� �÷��̾� 300���ھ� ȹ��
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

