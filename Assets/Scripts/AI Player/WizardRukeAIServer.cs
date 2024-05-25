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
    public PlayerServer target;
    public PlayerAnimator playerAnimator;
    public Rigidbody rb;

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
    public float maxDistanceDetect = 8;

    [Header("���� ����")]
    public float attackRange = 2f;
    public float attackCooldown = 1f;
    [SerializeField]private float lastAttackTime;

    [SerializeField] private AIState currentState;
    [SerializeField] private NavMeshAgent agent;

    [Header("Wizard Ruke AI�� ������Ʈ��")]
    public WizardRukeAISpellManagerServer wizardRukeAISpellManagerServer;
    public WizardRukeAIClient wizardRukeAIClient;
    public WizardRukeAIHPManagerServer wizardRukeAIHPManagerServer;

    public override void OnNetworkSpawn()
    {
        //Debug.Log($"Awake IsServer:{IsServer}");
        if (!IsServer) return;
        //Debug.Log($"Awake this is Server");
        //ulong dummyAIClientId = 3;
        //InitializePlayerOnServer(dummyAIClientId);
        
        SetState(new IdleState(this));
    }

    private void Update()
    {
        //Debug.Log($"IsServer:{IsServer}, IsOwnedByServer:{IsOwnedByServer}");
        if (!IsServer) return;

        if (gameState == PlayerGameState.GameOver)
        {
            playerAnimator.UpdatePlayerMoveAnimationOnServer(PlayerMoveAnimState.GameOver);
            return;
        }
        currentState?.Update();
    }

    /// <summary>
    /// AI�� Ŭ���̾�Ʈ ID�� �Ҵ�������մϴ�.
    /// </summary>
    /// <param name="AIClientId"></param>
    public void InitializeAIPlayerOnServer(ulong AIClientId, Vector3 spawnPos)
    {
        gameState = PlayerGameState.Playing;

        Debug.Log($"WizardRukeAIServer Player{AIClientId} (class : {this.characterClass.ToString()}) InitializeAIPlayerOnServer");

        if (GameAssetsManager.Instance == null)
        {
            Debug.Log($"{nameof(InitializeAIPlayerOnServer)}, GameAssets�� ã�� ���߽��ϴ�.");
            return;
        }

        PlayerInGameData playerInGameData = GameMultiplayer.Instance.GetPlayerDataFromClientId(AIClientId);
        SetCharacterData(playerInGameData);

        // ���� ��ġ �ʱ�ȭ   
        transform.position = spawnPos;// spawnPointsController.GetSpawnPoint(GameMultiplayer.Instance.GetPlayerDataIndexFromClientId(AIClientId));

        // HP �ʱ�ȭ
        // ���� HP ���� �� UI�� �ݿ�
        wizardRukeAIHPManagerServer.InitPlayerHP(this);

        // �÷��̾� �г���UI & ���͸��� �ʱ�ȭ
        wizardRukeAIClient.InitializeAIClientRPC(playerInGameData.playerName.ToString());

        // �÷��̾ ������ ��ų ��� ����
        wizardRukeAISpellManagerServer.InitPlayerSpellInfoArrayOnServer(this.skills);

        // �÷��̾� Layer ����
        switch (AIClientId)
        {
            case 0:
                gameObject.layer = LayerMask.NameToLayer("AI Player0");
                break;
            case 1:
                gameObject.layer = LayerMask.NameToLayer("AI Player1");
                break;
            case 2:
                gameObject.layer = LayerMask.NameToLayer("AI Player2");
                break;
            case 3:
                gameObject.layer = LayerMask.NameToLayer("AI Player3");
                break;
            default:
                break;
        }
    }

    // �׽�Ʈ �� Navmesh�� �����Ұ�. 
    public void MoveTowardsTarget()
    {
        //Vector3 direction = (target.transform.position - transform.position).normalized;
        //transform.position += direction * moveSpeed * Time.deltaTime;

        //agent.isStopped = false;
        agent.SetDestination(target.transform.position);

        //Debug.Log($"������Ʈ�� �����ִ°�? : {agent.isStopped}");
        // �̵� �ִϸ��̼� ����
        playerAnimator.UpdatePlayerMoveAnimationOnServer(PlayerMoveAnimState.Walking);
    }

    public void AttackTarget()
    {
        if (Time.time > lastAttackTime + attackCooldown)
        {
            //Debug.Log("Attacking the target!");
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
        // Idle �ִϸ��̼� ����
        playerAnimator.UpdatePlayerMoveAnimationOnServer(PlayerMoveAnimState.Idle);

        Collider[] colliders = Physics.OverlapSphere(transform.position, maxDistanceDetect);
        List<PlayerServer> players = new List<PlayerServer>();

        foreach (var collider in colliders)
        {
            if (collider.CompareTag("Player"))
            {
                collider.TryGetComponent<PlayerServer>(out PlayerServer player);
                if (player != null)
                {
                    players.Add(player);
                }
            }
        }
        
        // HP�� ���� ���� �÷��̾ Ÿ������ ����
        if (players.Count > 0)
        {
            List<PlayerServer> sortedPlayers = players.OrderByDescending(player => player.GetPlayerHP()).ToList();
            target = sortedPlayers[0];
        }
    }

    // �ǰ�ó�� 
    public void PlayerGotHitOnServer(sbyte damage, ulong clientWhoAttacked)
    {
        // �ǰ� ó�� �Ѱ�.
        wizardRukeAIHPManagerServer.TakingDamage(damage, clientWhoAttacked);
        // �� Client UI ������Ʈ ���� Damage Text Popup
        wizardRukeAIClient.ShowDamageTextPopupClientRPC(damage);
        // ���� �÷��̾� ī�޶� ����ũ. AI��� ����ũ ��ų �ʿ� �����ϴ�.
        NetworkClient networkClient = NetworkManager.ConnectedClients[clientWhoAttacked];
        if(networkClient.PlayerObject.TryGetComponent<PlayerClient>(out PlayerClient playerClient))
        {
            playerClient.ActivateHitCameraShakeClientRPC();
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
}

