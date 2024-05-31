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
    public GameObject target;
    public PlayerAnimator playerAnimator;

    [Header("물리 관련")]
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
    public float maxDistanceDetect;

    [Header("공격 관련")]
    public float attackRange;
    public float attackCooldown;
    [SerializeField]private float lastAttackTime;

    [SerializeField] private AIState currentState;

    [Header("Wizard Ruke AI용 컴포넌트들")]
    public WizardRukeAISpellManagerServer wizardRukeAISpellManagerServer;
    public WizardRukeAIClient wizardRukeAIClient;
    public WizardRukeAIHPManagerServer wizardRukeAIHPManagerServer;
    public WizardRukeAIMovementServer wizardRukeAIMovementServer;
    public WizardRukeAIBattleSystemServer wizardRukeAIBattleSystemServer;

    // GC 작업의 최소화를 위한 캐싱
    private IdleState aiIdleState;
    private PatrolState aiPatrolState;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        aiIdleState = new IdleState(this);
        aiPatrolState = new PatrolState(this);

        // 게임 시작 전 대기!
        SetState(aiIdleState);

        GameManager.Instance.OnGameStateChanged += GameManager_OnGameStateChanged;
        GameManager.Instance.OnPlayerGameOver += GameManager_OnPlayerGameOver;
    }

    private void GameManager_OnPlayerGameOver(object sender, PlayerGameOverEventArgs e)
    {
        // 게임오버된 플레이어가 현재 target라면 target을 초기화하고 재검색합니다.

        // 현재 target이 설정되어있지 않은 상태라면 작업을 해줄 필요 없습니다.
        if(!target) return;

        ulong targetClientID = 0;
        // AI인 경우
        if (target.TryGetComponent<WizardRukeAIServer>(out WizardRukeAIServer wizardRukeAIServer)) {
            targetClientID = wizardRukeAIServer.AIClientId;
        }
        // Player인 경우
        else if(target.TryGetComponent<PlayerServer>(out PlayerServer playerServer))
        {
            targetClientID = playerServer.OwnerClientId;
        }

        // 게임오버된 플레이어가 현재 타겟이었으면 타겟 초기화, 다시 검색 시작. 
        if(targetClientID == GameMultiplayer.Instance.GetPlayerDataFromClientId(e.clientIDWhoGameOver).clientId)
        {
            Debug.Log("Target 게임오버! 새로운 타겟을 검색합니다");
            target = null;
            SetState(aiPatrolState);
        }
    }

    private void GameManager_OnGameStateChanged(object sender, EventArgs e)
    {
        if (GameManager.Instance.IsGamePlaying())
        {
            // 이제 카운트다운은 끝! 게임 시작! 순찰을 시작합니다!
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
    /// AI의 클라이언트 ID를 할당해줘야합니다.
    /// </summary>
    /// <param name="AIClientId"></param>
    public void InitializeAIPlayerOnServer(ulong AIClientId, Vector3 spawnPos)
    {
        gameState = PlayerGameState.Playing;

        //Debug.Log($"WizardRukeAIServer Player{AIClientId} (class : {this.characterClass.ToString()}) InitializeAIPlayerOnServer");

        if (GameAssetsManager.Instance == null)
        {
            Debug.Log($"{nameof(InitializeAIPlayerOnServer)}, GameAssets를 찾지 못했습니다.");
            return;
        }

        PlayerInGameData playerInGameData = GameMultiplayer.Instance.GetPlayerDataFromClientId(AIClientId);
        SetCharacterData(playerInGameData);

        // NavMesh를 사용하기 대문에 속도 설정을 따로 챙겨줍니다
        wizardRukeAIMovementServer.SetMoveSpeed(playerInGameData.moveSpeed);

        // 스폰 위치 초기화   
        transform.position = spawnPos;// spawnPointsController.GetSpawnPoint(GameMultiplayer.Instance.GetPlayerDataIndexFromClientId(AIClientId));

        // HP 초기화
        // 현재 HP 설정 및 UI에 반영
        wizardRukeAIHPManagerServer.InitPlayerHP(this);

        // 플레이어 닉네임UI & 메터리얼 초기화
        wizardRukeAIClient.InitializeAIClientRPC(playerInGameData.playerName.ToString());

        // 플레이어가 보유한 스킬 목록 저장
        wizardRukeAISpellManagerServer.InitAIPlayerSpellInfoArrayOnServer(this.skills);

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

    public void MoveTowardsTarget()
    {
        if (!target) return;

        wizardRukeAIMovementServer.MoveToTarget(target.transform);

        // 이동 애니메이션 실행
        playerAnimator.UpdatePlayerMoveAnimationOnServer(PlayerMoveAnimState.Walking);
    }

    public void AttackTarget()
    {
        if (Time.time > lastAttackTime + attackCooldown)
        {
            //Debug.Log("Attacking the target!");
            //wizardRukeAIBattleSystemServer.Attack();
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
        wizardRukeAIMovementServer.Patrol();

        // 근처 범위 검색
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
            // 자신을 제외한 AI를 검색
            else if (collider.gameObject != gameObject && collider.CompareTag("AI"))
            {
                if (collider.TryGetComponent<WizardRukeAIServer>(out WizardRukeAIServer aiPlayer))
                {
                    aiPlayers.Add(aiPlayer);
                }
            }
        }
        
        // HP가 가장 낮은 플레이어를 타겟으로 설정
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
    }

    // 피격처리.
    public void PlayerGotHitOnServer(sbyte damage, ulong clientIDWhoAttacked, GameObject clientObjectWhoAttacked)
    {
        // 피격 처리 총괄.
        wizardRukeAIHPManagerServer.TakingDamage(damage, clientIDWhoAttacked);
        
        // 공격자가 인식범위 안에 있으면 타겟으로 설정. 
        if(Vector3.Distance(transform.position, target.transform.position) <= maxDistanceDetect)
            target = clientObjectWhoAttacked;

        // 방어스킬 발동
        wizardRukeAIBattleSystemServer.Defence();

        // 공격자가 Player인가? AI인가? 
        if (GameMultiplayer.Instance.GetPlayerDataFromClientId(clientIDWhoAttacked).isAI)
        {
            // AI라면 카메라 쉐이크는 하지 않는다.
            return;
        }

        // 공격자가 Player라면 카메라 쉐이크 
        NetworkClient networkClient = NetworkManager.ConnectedClients[clientIDWhoAttacked];
        if (networkClient.PlayerObject.TryGetComponent<PlayerClient>(out PlayerClient playerClient))
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

    public void GameOver()
    {
        if (gameState != PlayerGameState.Playing) return;
        Debug.Log($"AI Player{AIClientId} is GameOver");
        gameState = PlayerGameState.GameOver;
        //playerAnimator.UpdatePlayerMoveAnimationOnServer(PlayerMoveAnimState.GameOver);
        // 추적 멈추기
        wizardRukeAIMovementServer.StopMove();
        // 물리충돌 해제
        rb.isKinematic = true;
        _collider.enabled = false;
        // 플레이어 이름 & HP UI off
        wizardRukeAIClient.OffPlayerUIClientRPC();
    }
}

