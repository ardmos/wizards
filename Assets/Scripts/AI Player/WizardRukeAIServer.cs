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
    public Collider _collider;

   // PlayerInGameData하고 ICharacter하고, 플레이어 정보를 초기화할 때 혼선이 있다. 이걸 하나로 통일할 필요가 있음. 확인하기.
    // InitializeAIPlayerOnServer 에서 전부 할당해줍니다.
    public ulong AIClientId;
    public Character characterClass { get; set; }
    public sbyte hp { get; set; }
    public sbyte maxHp { get; set; }
    public float moveSpeed { get; set; }
    // 이건 여기서 직접 해줍니다. 이것도 일관성이 없다.  수정 필요
    public SkillName[] skills { get; set; } = new SkillName[]{
                SkillName.FireBallLv1,
                SkillName.WaterBallLv1,
                SkillName.BlizzardLv1,
                SkillName.MagicShieldLv1
                };

    [Header("상태 관련")]
    public PlayerGameState gameState;

    [Header("색적 관련")]
    public float maxDistanceDetect = 8;

    [Header("공격 관련")]
    public float attackRange = 2f;
    public float attackCooldown = 1f;
    [SerializeField]private float lastAttackTime;

    [SerializeField] private AIState currentState;
    [SerializeField] private NavMeshAgent agent;

    [Header("Wizard Ruke AI용 컴포넌트들")]
    public WizardRukeAISpellManagerServer wizardRukeAISpellManagerServer;
    public WizardRukeAIClient wizardRukeAIClient;
    public WizardRukeAIHPManagerServer wizardRukeAIHPManagerServer;

    [Header("순찰을 위한 변수들")]
    [SerializeField] private PlayerSpawnPointsController playerSpawnPointsController;
    [SerializeField] private int randomIndex;
    [SerializeField] private Transform patrolDestination;

    public override void OnNetworkSpawn()
    {
        //Debug.Log($"Awake IsServer:{IsServer}");
        if (!IsServer) return;
        //Debug.Log($"Awake this is Server");
        //ulong dummyAIClientId = 3;
        //InitializePlayerOnServer(dummyAIClientId);

        playerSpawnPointsController = FindObjectOfType<PlayerSpawnPointsController>();

        SetState(new IdleState(this));

        GameManager.Instance.OnGameStateChanged += GameManager_OnGameStateChanged;


    }

    private void GameManager_OnGameStateChanged(object sender, EventArgs e)
    {
        if (GameManager.Instance.IsGamePlaying())
        {
            // 이제 카운트다운은 끝! 게임 시작!
            SetState(new PatrolState(this));
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
    }

    /// <summary>
    /// AI의 클라이언트 ID를 할당해줘야합니다.
    /// </summary>
    /// <param name="AIClientId"></param>
    public void InitializeAIPlayerOnServer(ulong AIClientId, Vector3 spawnPos)
    {
        gameState = PlayerGameState.Playing;

        Debug.Log($"WizardRukeAIServer Player{AIClientId} (class : {this.characterClass.ToString()}) InitializeAIPlayerOnServer");

        if (GameAssetsManager.Instance == null)
        {
            Debug.Log($"{nameof(InitializeAIPlayerOnServer)}, GameAssets를 찾지 못했습니다.");
            return;
        }

        PlayerInGameData playerInGameData = GameMultiplayer.Instance.GetPlayerDataFromClientId(AIClientId);
        SetCharacterData(playerInGameData);

        // NavMesh를 사용하기 대문에 속도 설정을 따로 챙겨줍니다
        agent.speed = playerInGameData.moveSpeed;

        // 스폰 위치 초기화   
        transform.position = spawnPos;// spawnPointsController.GetSpawnPoint(GameMultiplayer.Instance.GetPlayerDataIndexFromClientId(AIClientId));

        // HP 초기화
        // 현재 HP 설정 및 UI에 반영
        wizardRukeAIHPManagerServer.InitPlayerHP(this);

        // 플레이어 닉네임UI & 메터리얼 초기화
        wizardRukeAIClient.InitializeAIClientRPC(playerInGameData.playerName.ToString());

        // 플레이어가 보유한 스킬 목록 저장
        wizardRukeAISpellManagerServer.InitPlayerSpellInfoArrayOnServer(this.skills);

        // 플레이어 Layer 설정
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

    // 테스트 후 Navmesh로 변경할것. 
    public void MoveTowardsTarget()
    {
        //Vector3 direction = (target.transform.position - transform.position).normalized;
        //transform.position += direction * moveSpeed * Time.deltaTime;

        //agent.isStopped = false;
        agent.SetDestination(target.transform.position);

        //Debug.Log($"에이전트는 멈춰있는가? : {agent.isStopped}");
        // 이동 애니메이션 실행
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
        // 이제 패트롤 모드 실행. 애니메이션도 Walk 애니메이션. 자동으로 실행. 
        Patrol();

        // 근처 범위 검색
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
        
        // HP가 가장 낮은 플레이어를 타겟으로 설정
        if (players.Count > 0)
        {
            List<PlayerServer> sortedPlayers = players.OrderByDescending(player => player.GetPlayerHP()).ToList();
            target = sortedPlayers[0];
        }
    }

    /// <summary>
    /// Spawn 포인트들을 랜덤으로 돌아보며 순찰합니다.
    /// </summary>
    private void Patrol()
    {
        if (playerSpawnPointsController == null) return;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
            {
                randomIndex = UnityEngine.Random.Range(0, playerSpawnPointsController.spawnPoints.Length);
                patrolDestination = playerSpawnPointsController.spawnPoints[randomIndex];

                agent.SetDestination(patrolDestination.position);
                playerAnimator.UpdatePlayerMoveAnimationOnServer(PlayerMoveAnimState.Walking);
            }
        }
    }

    // 피격처리 
    public void PlayerGotHitOnServer(sbyte damage, ulong clientWhoAttacked)
    {
        // 피격 처리 총괄.
        wizardRukeAIHPManagerServer.TakingDamage(damage, clientWhoAttacked);
        // 각 Client UI 업데이트 지시 Damage Text Popup.  이젠 HPManager.TakingDamage에서 해줍니다.
        //wizardRukeAIClient.ShowDamageTextPopupClientRPC(damage);
        // 맞춘 플레이어 카메라 쉐이크. AI라면 쉐이크 시킬 필요 없습니다.
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

    // 스크롤 활용. 스킬 강화 VFX 실행
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

    public void ReduceMoveSpeed(float value)
    {
        agent.speed -= value;
    }

    public void AddMoveSpeed(float value)
    {
        agent.speed += value;
    }

    public void GameOver()
    {
        if (gameState != PlayerGameState.Playing) return;
        Debug.Log($"AI Player{AIClientId} is GameOver");
        gameState = PlayerGameState.GameOver;
        playerAnimator.UpdatePlayerMoveAnimationOnServer(PlayerMoveAnimState.GameOver);
        agent.isStopped = true; // 추적 멈추기
        // 물리충돌 해제
        rb.isKinematic = true;
        _collider.enabled = false;
        // 플레이어 이름 & HP UI off
        wizardRukeAIClient.OffPlayerUIClientRPC();
    }
}

