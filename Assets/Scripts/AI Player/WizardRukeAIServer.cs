using System;
using Unity.Netcode;
using UnityEngine;
using static ComponentValidator;


/// <summary>
/// AI ������ Ruke�� ���� �� ������ �����ϴ� Ŭ�����Դϴ�.
/// </summary>
public class WizardRukeAIServer : NetworkBehaviour, ICharacter, ITargetable
{
    #region Constants & Fields
    // ���� �޽��� �����
    private const string ERROR_STATE_MACHINE_NOT_SET = "WizardRukeAIServer stateMachine ������ �ȵǾ��ֽ��ϴ�.";
    private const string ERROR_MULTIPLAYER_GAME_MANAGER_NOT_SET = "WizardRukeAIServer MultiplayerGameManager.Instance ������ �ȵǾ��ֽ��ϴ�.";
    private const string ERROR_TARGET_NOT_SET = "WizardRukeAIServer target ������ �ȵǾ��ֽ��ϴ�.";
    private const string ERROR_CURRENT_PLAYER_DATA_MANAGER_NOT_SET = "WizardRukeAIServer CurrentPlayerDataManager.Instance ������ �ȵǾ��ֽ��ϴ�.";
    private const string ERROR_MOVEMENT_MANAGER_NOT_SET = "WizardRukeAIServer wizardRukeAIMovementManager ������ �ȵǾ��ֽ��ϴ�.";
    private const string ERROR_BATTLE_MANAGER_NOT_SET = "WizardRukeAIServer wizardRukeAIBattleManager ������ �ȵǾ��ֽ��ϴ�.";
    private const string ERROR_GAME_ASSET_MANAGER_NOT_SET = "WizardRukeAIServer GameAssetsManager.Instance ������ �ȵǾ��ֽ��ϴ�.";
    private const string ERROR_VFX_PREFAB_NOT_SET = "WizardRukeAIServer vfxPrefab ������ �ȵǾ��ֽ��ϴ�.";
    private const string ERROR_GAME_OVER_MANAGER_NOT_SET = "WizardRukeAIServer wizardRukeAIGameOverManager ������ �ȵǾ��ֽ��ϴ�.";
    private const string ERROR_HP_MANAGER_NOT_SET = "WizardRukeAIServer wizardRukeAIHPManager ������ �ȵǾ��ֽ��ϴ�.";
    private const string ERROR_CLIENT_NOT_SET = "WizardRukeAIServer wizardRukeAIClient ������ �ȵǾ��ֽ��ϴ�.";
    private const string ERROR_SPELL_MANAGER_NOT_SET = "WizardRukeAIServer wizardRukeAISpellManager ������ �ȵǾ��ֽ��ϴ�.";

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
    /// AI stateMachine�� �� ������ ������Ʈ ��ŵ�ϴ�.
    /// </summary>

    private void Update()
    {
        if (!IsServer) return;
        if (!ValidateComponent(stateMachine, ERROR_STATE_MACHINE_NOT_SET)) return;
        if (aiGameState == PlayerGameState.GameOver) return;

        stateMachine.UpdateState();
    }

    /// <summary>
    /// ��Ʈ��ũ ��ü�� ������ �� ȣ��Ǵ� �޼����Դϴ�.
    /// </summary>
    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        if (!ValidateComponent(MultiplayerGameManager.Instance, ERROR_MULTIPLAYER_GAME_MANAGER_NOT_SET)) return;

        MultiplayerGameManager.Instance.OnGameStateChanged += GameManager_OnGameStateChanged;
        MultiplayerGameManager.Instance.OnPlayerGameOver += GameManager_OnPlayerGameOver;
    }

    /// <summary>
    /// ��Ʈ��ũ ��ü�� ������ �� ȣ��Ǵ� �޼����Դϴ�.
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
    /// AI �÷��̾ �ʱ�ȭ�մϴ�.
    /// </summary>
    /// <param name="AIClientId">AI �÷��̾��� Ŭ���̾�Ʈ ID</param>
    public void InitializeAIPlayerOnServer(ulong AIClientId)
    {
        if (!ValidateComponent(CurrentPlayerDataManager.Instance, ERROR_CURRENT_PLAYER_DATA_MANAGER_NOT_SET)) return;

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
        if (!ValidateAIComponents()) return;

        InitializeClient(playerInGameData);
        InitializeManagers(playerInGameData);
        InitializeStateMachineAndTargetingSystem();
    }

    /// <summary>
    /// AI Ŭ���̾�Ʈ�� �ʱ�ȭ�մϴ�.
    /// </summary>
    /// <param name="playerInGameData">�÷��̾� �ΰ��� ������</param>
    private void InitializeClient(PlayerInGameData playerInGameData)
    {
        wizardRukeAIClient.InitializeAIClientRPC(playerInGameData.playerName.ToString());
    }

    /// <summary>
    /// AI �Ŵ������� �ʱ�ȭ�մϴ�.
    /// </summary>
    /// <param name="playerInGameData">�÷��̾� �ΰ��� ������</param>
    private void InitializeManagers(PlayerInGameData playerInGameData)
    {
        wizardRukeAIMovementManager.SetMoveSpeed(playerInGameData.moveSpeed);
        wizardRukeAIHPManager.InitPlayerHP(this);
        wizardRukeAISpellManager.InitAIPlayerSpellInfoArrayOnServer(this.spells);
        wizardRukeAIGameOverManager.InitAIGameOverManager(this, wizardRukeAIClient);
    }

    /// <summary>
    /// ���� �ӽŰ� Ÿ���� �ý����� �ʱ�ȭ�մϴ�.
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
    /// AI�� �ִ� ���� �Ÿ��� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>�ִ� ���� �Ÿ�</returns>
    public float GetMaxDetectionDistance() => MAX_DETECTION_DISTANCE;
    #endregion

    #region AI Targeting Management
    /// <summary>
    /// ���ο� Ÿ���� �����մϴ�.
    /// </summary>
    /// <param name="newTarget">���ο� Ÿ�� ���� ������Ʈ</param>
    public void SetTarget(GameObject newTarget) => target = newTarget;

    /// <summary>
    /// ���� AI�� Ÿ���� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>���� Ÿ�� ���� ������Ʈ</returns>
    public GameObject GetTarget() => target;

    /// <summary>
    /// AI�� Ÿ���� �ý����� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>AI Ÿ���� �ý���</returns>
    public AITargetingSystem GetTargetingSystem() => targetingSystem;

    /// <summary>
    /// ���� Ÿ���� Ŭ���̾�ƮID�� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>Ÿ���� Ŭ���̾�ƮID</returns>
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
    /// AI �÷��̾��� ���� ���� ó���� �մϴ�.
    /// </summary>
    /// <param name="attackerClientID">������ Ŭ���̾�Ʈ�� ID</param>
    public void GameOver(ulong attackerClientID)
    {
        if (aiGameState != PlayerGameState.Playing) return;
        if (!ValidateComponent(wizardRukeAIGameOverManager, ERROR_GAME_OVER_MANAGER_NOT_SET)) return;

        aiGameState = PlayerGameState.GameOver;
        wizardRukeAIGameOverManager.HandleGameOver(attackerClientID);
    }

    /// <summary>
    /// AI�� ���� ���� ���¸� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>AI�� ���� ���� ����</returns>
    public PlayerGameState GetAIGameState() => aiGameState;

    /// <summary>
    /// AI�� ���� �ӽ��� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>AI ���� �ӽ�</returns>
    public AIStateMachine GetStateMachine() => stateMachine;
    #endregion

    #region AI Movement Management
    /// <summary>
    /// Ÿ���� ���� �̵��մϴ�.
    /// </summary>
    public void MoveTowardsTarget()
    {
        if (!ValidateComponent(target, ERROR_TARGET_NOT_SET)) return;
        if (!ValidateComponent(wizardRukeAIMovementManager, ERROR_MOVEMENT_MANAGER_NOT_SET)) return;

        wizardRukeAIMovementManager.MoveToTarget(target.transform);
    }

    /// <summary>
    /// AI�� �̵� �Ŵ����� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>AI �̵� �Ŵ���</returns>
    public WizardRukeAIMovementManagerServer GetMovementManager() => wizardRukeAIMovementManager;
    #endregion

    #region AI Attack Management
    /// <summary>
    /// Ÿ���� �����մϴ�.
    /// </summary>
    public void AttackTarget()
    {
        if (!ValidateComponent(target, ERROR_TARGET_NOT_SET)) return;
        if (!ValidateComponent(wizardRukeAIBattleManager, ERROR_BATTLE_MANAGER_NOT_SET)) return;

        wizardRukeAIBattleManager.Attack();
    }
    /// <summary>
    /// AI�� ���� �Ŵ����� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>AI ���� �Ŵ���</returns>
    public WizardRukeAIBattleManagerServer GetBattleManager() => wizardRukeAIBattleManager;
    #endregion

    #region AI Animator Management
    /// <summary>
    /// AI�� �÷��̾� �ִϸ����͸� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>�÷��̾� �ִϸ�����</returns>
    public PlayerAnimator GetPlayerAnimator() => playerAnimator;
    #endregion

    #region AI VFX Control
    /// <summary>
    /// ��ũ�� ��� ȿ���� �����ϴ� VFX�� �����մϴ�.
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
    /// AI ������Ʈ���� �ùٸ��� �Ҵ�Ǿ����� Ȯ���մϴ�.
    /// </summary>
    /// <returns>��� ������Ʈ�� �Ҵ�Ǿ����� true, �׷��� ������ false</returns>
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
    /// ���� ���°� ����� �� ȣ��Ǵ� �̺�Ʈ �ڵ鷯�Դϴ�.
    /// </summary>
    private void GameManager_OnGameStateChanged(object sender, EventArgs e)
    {
        if (!ValidateComponent(MultiplayerGameManager.Instance, ERROR_MULTIPLAYER_GAME_MANAGER_NOT_SET)) return;
        if (!ValidateComponent(stateMachine, ERROR_STATE_MACHINE_NOT_SET)) return;

        // ���ο� ���� ���°� Playing�̸� ������ �����մϴ�
        if (MultiplayerGameManager.Instance.IsGamePlaying()) stateMachine.ChangeState(AIStateType.Patrol);
    }

    /// <summary>
    /// �÷��̾ ���� ������ �� ȣ��Ǵ� �̺�Ʈ �ڵ鷯�Դϴ�.
    /// </summary>
    private void GameManager_OnPlayerGameOver(object sender, PlayerGameOverEventArgs e)
    {
        // ���ӿ����� �÷��̾ ���� target��� target�� �ʱ�ȭ�ϰ� ��˻��մϴ�.
        // ���� target�� �����Ǿ����� ���� ���¶�� �۾��� ���� �ʿ� �����ϴ�.
        if (!ValidateComponent(target, ERROR_TARGET_NOT_SET)) return;
        if (!ValidateComponent(stateMachine, ERROR_STATE_MACHINE_NOT_SET)) return;

        // ���ӿ����� �÷��̾ ���� Ÿ���̾����� Ÿ�� �ʱ�ȭ, �ٽ� �˻� ����.
        if (e.clientIDWhoGameOver == GetTargetClientID())
        {
            Logger.Log("Target ���ӿ���! ���ο� Ÿ���� �˻��մϴ�");
            target = null;
            stateMachine.ChangeState(AIStateType.Patrol);
        }
    }
    #endregion

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