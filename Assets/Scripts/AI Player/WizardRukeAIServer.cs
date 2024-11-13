using System;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// AI ������ Ruke�� ���� �� ������ �����ϴ� Ŭ�����Դϴ�.
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

    // ICharacter �������̽� ����
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
    /// ���� ���°� ����� �� ȣ��Ǵ� �̺�Ʈ �ڵ鷯�Դϴ�.
    /// </summary>
    private void GameManager_OnGameStateChanged(object sender, EventArgs e)
    {
        if (MultiplayerGameManager.Instance == null) return;

        if (MultiplayerGameManager.Instance.IsGamePlaying())
        {
            // ���ο� ���� ���°� Playing�̸� ������ �����մϴ�
            stateMachine.ChangeState(AIStateType.Patrol);
        }
    }

    /// <summary>
    /// �÷��̾ ���� ������ �� ȣ��Ǵ� �̺�Ʈ �ڵ鷯�Դϴ�.
    /// </summary>
    private void GameManager_OnPlayerGameOver(object sender, PlayerGameOverEventArgs e)
    {
        // ���ӿ����� �÷��̾ ���� target��� target�� �ʱ�ȭ�ϰ� ��˻��մϴ�.
        // ���� target�� �����Ǿ����� ���� ���¶�� �۾��� ���� �ʿ� �����ϴ�.
        if (!target) return;

        // ���ӿ����� �÷��̾ ���� Ÿ���̾����� Ÿ�� �ʱ�ȭ, �ٽ� �˻� ����. 
        if (e.clientIDWhoGameOver == GetTargetClientID())
        {
            Debug.Log("Target ���ӿ���! ���ο� Ÿ���� �˻��մϴ�");
            target = null;
            stateMachine.ChangeState(AIStateType.Patrol);
        }
    }

    /// <summary>
    /// AI �÷��̾ �ʱ�ȭ�մϴ�.
    /// </summary>
    /// <param name="AIClientId">AI �÷��̾��� Ŭ���̾�Ʈ ID</param>
    public void InitializeAIPlayerOnServer(ulong AIClientId)
    {
        if (CurrentPlayerDataManager.Instance == null) return;

        PlayerInGameData playerData = CurrentPlayerDataManager.Instance.GetPlayerDataByClientId(AIClientId);
        SetAIPlayerData(playerData);
        InitializeAIComponents(playerData);
        aiGameState = PlayerGameState.Playing;
    }

    /// <summary>
    /// AI ������Ʈ���� �ʱ�ȭ�մϴ�.
    /// </summary>
    /// <param name="playerInGameData">�÷��̾� �ΰ��� ������</param>
    private void InitializeAIComponents(PlayerInGameData playerInGameData)
    {
        if (!ComponentsValidationCheck()) return;

        wizardRukeAIMovementManager.SetMoveSpeed(playerInGameData.moveSpeed);
        wizardRukeAIHPManager.InitPlayerHP(this);
        // �÷��̾� �г���UI & ���͸��� �ʱ�ȭ
        wizardRukeAIClient.InitializeAIClientRPC(playerInGameData.playerName.ToString());
        // �÷��̾ ������ ��ų ��� ����
        wizardRukeAISpellManager.InitAIPlayerSpellInfoArrayOnServer(this.spells);
        wizardRukeAIGameOverManager.InitAIGameOverManager(this, wizardRukeAIClient);
        stateMachine = new AIStateMachine(this);
        stateMachine.Initialize(AIStateType.Idle);
        targetingSystem = new AITargetingSystem(MAX_DETECTION_DISTANCE, transform);
    }

    /// <summary>
    /// AI ������Ʈ���� �ùٸ��� �Ҵ�Ǿ����� Ȯ���մϴ�.
    /// </summary>
    /// <returns>��� ������Ʈ�� �Ҵ�Ǿ����� true, �׷��� ������ false</returns>
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
    /// Ÿ���� ���� �̵��մϴ�.
    /// </summary>
    public void MoveTowardsTarget()
    {
        if (target == null) return;
        if (wizardRukeAIMovementManager == null) return;

        wizardRukeAIMovementManager.MoveToTarget(target.transform);
    }

    /// <summary>
    /// Ÿ���� �����մϴ�.
    /// </summary>
    public void AttackTarget()
    {
        if (target == null) return;
        if (wizardRukeAIBattleManager == null) return;

        wizardRukeAIBattleManager.Attack();
    }

    #region RPC
    /// <summary>
    /// ��ũ�� ��� ȿ���� �����ϴ� VFX�� �����մϴ�.
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
    /// AI �÷��̾��� ���� ���� ó���� �մϴ�.
    /// </summary>
    /// <param name="attackerClientID">������ Ŭ���̾�Ʈ�� ID</param>
    public void GameOver(ulong attackerClientID)
    {
        if (aiGameState != PlayerGameState.Playing) return;
        if (wizardRukeAIGameOverManager == null) return;

        aiGameState = PlayerGameState.GameOver;
        wizardRukeAIGameOverManager.HandleGameOver(attackerClientID);
    }

    /// <summary>
    /// ���� Ÿ���� Ŭ���̾�ƮID�� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>Ÿ���� Ŭ���̾�ƮID</returns>
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
    /// AI �÷��̾� �����͸� �����մϴ�.
    /// </summary>
    /// <param name="playerData">�÷��̾� ������</param>
    public void SetAIPlayerData(PlayerInGameData playerData)
    {
        this.aiClientId = playerData.clientId;
        ICharacter aiCharacterData = CharacterSpecifications.GetCharacter(playerData.characterClass); 
        SetCharacterData(aiCharacterData);
    }
    /// <summary>
    /// ���ο� Ÿ���� �����մϴ�.
    /// </summary>
    /// <param name="newTarget">���ο� Ÿ�� ���� ������Ʈ</param>
    public void SetTarget(GameObject newTarget) => target = newTarget;
    public GameObject GetTarget() => target;
    public PlayerGameState GetAIGameState() => aiGameState;
    public AIStateMachine GetStateMachine() => stateMachine;
    public AITargetingSystem GetTargetingSystem() => targetingSystem;
    public WizardRukeAIMovementManagerServer GetMovementManager() => wizardRukeAIMovementManager;
    public WizardRukeAIBattleManagerServer GetBattleManager() => wizardRukeAIBattleManager;
    public PlayerAnimator GetPlayerAnimator() => playerAnimator;
    public float GetMaxDetectionDistance() => MAX_DETECTION_DISTANCE;

    #region ICharacter ����
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

    #region ITargetable ����
    public float GetHP() => hp;
    public GameObject GetGameObject() => gameObject;
    #endregion
}