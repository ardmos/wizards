using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.Services.Matchmaker.Models;
using UnityEngine;
using UnityEngine.AI;

public class WizardRukeAIServer : NetworkBehaviour, ICharacter
{
    private const int DEFAULT_SCORE = 300;

    public GameObject target;
    public PlayerAnimator playerAnimator;

    [Header("���� ����")]
    public Rigidbody rb;
    public Collider _collider;

   // PlayerInGameData�ϰ� ICharacter�ϰ�, �÷��̾� ������ �ʱ�ȭ�� �� ȥ���� �ִ�. �̰� �ϳ��� ������ �ʿ䰡 ����. Ȯ���ϱ�.
    // InitializeAIPlayerOnServer ���� ���� �Ҵ����ݴϴ�.
    public ulong AIClientId;
    public Character characterClass { get; set; }
    public sbyte hp { get; set; }
    public sbyte maxHp { get; set; }
    public float moveSpeed { get; set; }
    // �̰� ���⼭ ���� ���ݴϴ�. �̰͵� �ϰ����� ����.  ���� �ʿ�
    public SkillName[] skills { get; set; } = new SkillName[]{
                SkillName.FireBallLv1,
                SkillName.WaterBallLv1,
                SkillName.BlizzardLv1,
                SkillName.MagicShieldLv1
                };

    [Header("���� ����")]
    public PlayerGameState gameState;

    [Header("���� ����")]
    public float maxDistanceDetect;

    [Header("���� ����")]
    public float attackRange;
    public float attackCooldown;
    [SerializeField]private float lastAttackTime;

    [SerializeField] private AIState currentState;

    [Header("Wizard Ruke AI�� ������Ʈ��")]
    public WizardRukeAISpellManagerServer wizardRukeAISpellManagerServer;
    public WizardRukeAIClient wizardRukeAIClient;
    public WizardRukeAIHPManagerServer wizardRukeAIHPManagerServer;
    public WizardRukeAIMovementServer wizardRukeAIMovementServer;
    public WizardRukeAIBattleSystemServer wizardRukeAIBattleSystemServer;

    // GC �۾��� �ּ�ȭ�� ���� ĳ��
    private IdleState aiIdleState;
    private PatrolState aiPatrolState;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        aiIdleState = new IdleState(this);
        aiPatrolState = new PatrolState(this);

        // ���� ���� �� ���!
        SetState(aiIdleState);

        GameManager.Instance.OnGameStateChanged += GameManager_OnGameStateChanged;
        GameManager.Instance.OnPlayerGameOver += GameManager_OnPlayerGameOver;
    }

    private void GameManager_OnPlayerGameOver(object sender, PlayerGameOverEventArgs e)
    {
        // ���ӿ����� �÷��̾ ���� target��� target�� �ʱ�ȭ�ϰ� ��˻��մϴ�.

        // ���� target�� �����Ǿ����� ���� ���¶�� �۾��� ���� �ʿ� �����ϴ�.
        if(!target) return;

        ulong targetClientID = 0;
        // AI�� ���
        if (target.TryGetComponent<WizardRukeAIServer>(out WizardRukeAIServer wizardRukeAIServer)) {
            targetClientID = wizardRukeAIServer.AIClientId;
        }
        // Player�� ���
        else if(target.TryGetComponent<PlayerServer>(out PlayerServer playerServer))
        {
            targetClientID = playerServer.OwnerClientId;
        }

        //Debug.Log($"���� �� �÷��̾� ���ӿ����� �ν�! targetClientID: {targetClientID}, clientIDWhoGameOver: {e.clientIDWhoGameOver}, // {GameMultiplayer.Instance.GetPlayerDataFromClientId(e.clientIDWhoGameOver).clientId}");

        // ���ӿ����� �÷��̾ ���� Ÿ���̾����� Ÿ�� �ʱ�ȭ, �ٽ� �˻� ����. 
        if (targetClientID == e.clientIDWhoGameOver)
        {
            Debug.Log("Target ���ӿ���! ���ο� Ÿ���� �˻��մϴ�");
            target = null;
            SetState(aiPatrolState);
        }
    }

    private void GameManager_OnGameStateChanged(object sender, EventArgs e)
    {
        if (GameManager.Instance.IsGamePlaying())
        {
            // ���� ī��Ʈ�ٿ��� ��! ���� ����! ������ �����մϴ�!
            SetState(aiPatrolState);
        }
    }

    private void Update()
    {
        //Debug.Log($"IsServer:{IsServer}, IsOwnedByServer:{IsOwnedByServer}");
        if (!IsServer) return;

        if (gameState == PlayerGameState.GameOver)
        {
            return;
        }
        currentState?.Update();
    }

    public override void OnDestroy()
    {
        GameManager.Instance.OnGameStateChanged -= GameManager_OnGameStateChanged;
        GameManager.Instance.OnPlayerGameOver -= GameManager_OnPlayerGameOver;
    }

    /// <summary>
    /// AI�� Ŭ���̾�Ʈ ID�� �Ҵ�������մϴ�.
    /// </summary>
    /// <param name="AIClientId"></param>
    public void InitializeAIPlayerOnServer(ulong AIClientId, Vector3 spawnPos)
    {
        gameState = PlayerGameState.Playing;

        //Debug.Log($"WizardRukeAIServer Player{AIClientId} (class : {this.characterClass.ToString()}) InitializeAIPlayerOnServer");

        if (GameAssetsManager.Instance == null)
        {
            Debug.Log($"{nameof(InitializeAIPlayerOnServer)}, GameAssets�� ã�� ���߽��ϴ�.");
            return;
        }

        PlayerInGameData playerInGameData = GameMultiplayer.Instance.GetPlayerDataFromClientId(AIClientId);
        SetCharacterData(playerInGameData);

        // NavMesh�� ����ϱ� �빮�� �ӵ� ������ ���� ì���ݴϴ�
        wizardRukeAIMovementServer.SetMoveSpeed(playerInGameData.moveSpeed);

        // ���� ��ġ �ʱ�ȭ   
        transform.position = spawnPos;// spawnPointsController.GetSpawnPoint(GameMultiplayer.Instance.GetPlayerDataIndexFromClientId(AIClientId));

        // HP �ʱ�ȭ
        // ���� HP ���� �� UI�� �ݿ�
        wizardRukeAIHPManagerServer.InitPlayerHP(this);

        // �÷��̾� �г���UI & ���͸��� �ʱ�ȭ
        wizardRukeAIClient.InitializeAIClientRPC(playerInGameData.playerName.ToString());

        // �÷��̾ ������ ��ų ��� ����
        wizardRukeAISpellManagerServer.InitAIPlayerSpellInfoArrayOnServer(this.skills);
    }

    public void MoveTowardsTarget()
    {
        if (!target) return;

        wizardRukeAIMovementServer.MoveToTarget(target.transform);

        // �̵� �ִϸ��̼� ����
        playerAnimator.UpdatePlayerMoveAnimationOnServer(PlayerMoveAnimState.Walking);
    }

    public void AttackTarget()
    {
        if (Time.time > lastAttackTime + attackCooldown)
        {
            //Debug.Log("Attacking the target!");
            wizardRukeAIBattleSystemServer.Attack();
            lastAttackTime = Time.time;
        }
    }

    public void SetState(AIState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState.Enter();
    }

    public void DetectAndSetTarget()
    {
        // ���� ��Ʈ�� ��� ����. �ִϸ��̼ǵ� Walk �ִϸ��̼�. �ڵ����� ����. 
        wizardRukeAIMovementServer.Patrol();

        // ��ó ���� �˻�
        Collider[] colliders = Physics.OverlapSphere(transform.position, maxDistanceDetect);
        List<PlayerServer> players = new List<PlayerServer>();
        List<WizardRukeAIServer> aiPlayers = new List<WizardRukeAIServer>();

        foreach (var collider in colliders)
        {
            if (collider.CompareTag("Player"))
            {
                if (collider.TryGetComponent<PlayerServer>(out PlayerServer player))
                {
                    players.Add(player);
                }
            }
            // �ڽ��� ������ AI�� �˻�
            else if (collider.gameObject != gameObject && collider.CompareTag("AI"))
            {
                if (collider.TryGetComponent<WizardRukeAIServer>(out WizardRukeAIServer aiPlayer))
                {
                    aiPlayers.Add(aiPlayer);
                }
            }
        }
        
        // HP�� ���� ���� �÷��̾ Ÿ������ ����
        if (players.Count > 0)
        {
            List<PlayerServer> sortedPlayers = players.OrderByDescending(player => player.GetPlayerHP()).ToList();
            target = sortedPlayers[0].gameObject;
        }
        if ( aiPlayers.Count > 0 )
        {
            List<WizardRukeAIServer> sortedAIPlayer = aiPlayers.OrderByDescending(aiPlayer => aiPlayer.GetPlayerHP()).ToList();
            target = sortedAIPlayer[0].gameObject;
        }

        // �˻��� Ÿ���� ���� ���. 
        if (players.Count == 0 && aiPlayers.Count == 0)
        {
            target = null;
        }
    }

    public sbyte GetPlayerHP()
    {
        return wizardRukeAIHPManagerServer.GetHP();
    }

    // ��ũ�� Ȱ��. ��ų ��ȭ VFX ����
    [ServerRpc(RequireOwnership = false)]
    public void StartApplyScrollVFXServerRPC()
    {
        GameObject vfxHeal = Instantiate(GameAssetsManager.Instance.gameAssets.vfx_SpellUpgrade, transform);
        vfxHeal.GetComponent<NetworkObject>().Spawn();
        vfxHeal.transform.SetParent(transform);
        //vfxHeal.transform.localPosition = new Vector3(0f, 0.1f, 0f);
    }

    public ICharacter GetCharacterData()
    {
        return this;
    }

    public void SetCharacterData(PlayerInGameData characterData)
    {
        this.AIClientId = characterData.clientId;
        this.characterClass = characterData.characterClass;
        this.hp = characterData.hp;
        this.maxHp = characterData.maxHp;
        this.moveSpeed = characterData.moveSpeed;
    }

    public void GameOver(ulong clientWhoAttacked)
    {
        if (gameState != PlayerGameState.Playing) return;
        gameState = PlayerGameState.GameOver;
        
        // ���� ���߱�
        wizardRukeAIMovementServer.StopMove();
        // �����浹 ����
        rb.isKinematic = true;
        _collider.enabled = false;
        // �÷��̾� �̸� & HP UI off
        wizardRukeAIClient.OffPlayerUIClientRPC();
        // ���������� �ʵ��� Tag ����
        tag = "GameOver";
        Debug.Log($"AI Player{AIClientId} is GameOver, new tag : {tag}");

        // ���� ���
        CalcScore(clientWhoAttacked);
        // GameOver �ִϸ��̼� ����
        playerAnimator.UpdatePlayerMoveAnimationOnServer(PlayerMoveAnimState.GameOver);
        // ���ӿ��� �÷��̾� ����� ������ ���.
        GameManager.Instance.UpdatePlayerGameOverOnServer(AIClientId, clientWhoAttacked);

        // ��ũ�� ������ ���
        DropItem();
    }

    private void CalcScore(ulong clientWhoAttacked)
    {
        // ������ ���ӿ��� ���� ���, ���� �� ��� �÷��̾�鿡�� ������ �ݴϴ�. 
        if (clientWhoAttacked == OwnerClientId)
        {
            foreach (PlayerInGameData playerInGameData in GameMultiplayer.Instance.GetPlayerDataNetworkList())
            {
                GameMultiplayer.Instance.AddPlayerScore(playerInGameData.clientId, DEFAULT_SCORE);
            }
        }
        // �Ϲ����� ��� ��� �÷��̾� 300���ھ� ȹ��
        else
        {
            GameMultiplayer.Instance.AddPlayerScore(clientWhoAttacked, DEFAULT_SCORE);
        }
    }

    private void DropItem()
    {
        // ���ʷ���Ʈ ������ ����
        Vector3 newItemHPPotionPos = new Vector3(transform.position.x + 0.5f, transform.position.y, transform.position.z);
        GameObject hpPotionObject = Instantiate(GameAssetsManager.Instance.GetItemHPPotionObject(), newItemHPPotionPos, transform.rotation, GameManager.Instance.transform);

        if (!hpPotionObject) return;

        if (hpPotionObject.TryGetComponent<NetworkObject>(out NetworkObject hpPotionObjectNetworkObject))
        {
            hpPotionObjectNetworkObject.Spawn();
            if (GameManager.Instance)
            {
                hpPotionObject.transform.parent = GameManager.Instance.transform;
                hpPotionObject.transform.position = newItemHPPotionPos;
            }
        }

        // ���ʷ���Ʈ ������ ��ũ��
        Vector3 newItemScrollPos = new Vector3(transform.position.x - 0.5f, transform.position.y, transform.position.z);
        GameObject scrollObject = Instantiate(GameAssetsManager.Instance.GetItemScrollObject(), newItemScrollPos, transform.rotation, GameManager.Instance.transform);

        if (!scrollObject) return;

        if (scrollObject.TryGetComponent<NetworkObject>(out NetworkObject scrollObjectNetworkObject))
        {
            scrollObjectNetworkObject.Spawn();
            if (GameManager.Instance)
            {
                scrollObject.transform.parent = GameManager.Instance.transform;
                scrollObject.transform.position = newItemScrollPos;
            }
        }
    }
}

