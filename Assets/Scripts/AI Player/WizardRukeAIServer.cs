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
    public ulong AIClientId;

    [Header("Wizard Ruke의 클래스 정보들")]
    public SpellManagerServerWizard spellManagerServerWizard;
    public Character characterClass { get; set; } = Character.Wizard;
    public sbyte hp { get; set; } = 5;
    public sbyte maxHp { get; set; } = 5;
    public float moveSpeed { get; set; } = 4f;
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

    public WizardRukeAIClient wizardRukeAIClient;
    public PlayerHPManagerServer playerHPManager;
    public SkillSpellManagerServer skillSpellManagerServer;
    public Rigidbody rb;

    void Awake()
    {
        gameState = PlayerGameState.Playing;
        SetState(new IdleState(this));
    }

    private void Update()
    {
        if (gameState == PlayerGameState.GameOver)
        {
            playerAnimator.UpdatePlayerMoveAnimationOnServer(PlayerMoveAnimState.GameOver);
            return;
        }
        currentState?.Update();
    }

    /// <summary>
    /// AI의 클라이언트 ID를 할당해줘야합니다.
    /// </summary>
    /// <param name="AIClientId"></param>
    public void InitializePlayerOnServer(ulong AIClientId)
    {
        Debug.Log($"WizardRukeAIServer Player{AIClientId} (class : {this.characterClass.ToString()}) InitializePlayerOnServer");

        PlayerSpawnPointsController spawnPointsController = FindObjectOfType<PlayerSpawnPointsController>();

        if (GameAssetsManager.Instance == null)
        {
            Debug.Log($"{nameof(InitializePlayerOnServer)}, GameAssets를 찾지 못했습니다.");
            return;
        }

        if (spawnPointsController == null)
        {
            Debug.LogError($"{nameof(InitializePlayerOnServer)}, 스폰위치를 특정하지 못했습니다.");
            return;
        }

        // 스폰 위치 초기화   
        transform.position = spawnPointsController.GetSpawnPoint(GameMultiplayer.Instance.GetPlayerDataIndexFromClientId(AIClientId));

        // HP 초기화
        // 현재 HP 저장 및 설정
        playerHPManager.InitPlayerHP(this);

        // 플레이어가 보유한 스킬 목록 저장
        skillSpellManagerServer.InitPlayerSpellInfoArrayOnServer(this.skills);

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
        agent.SetDestination(transform.position);
        // 이동 애니메이션 실행
        playerAnimator.UpdatePlayerMoveAnimationOnServer(PlayerMoveAnimState.Walking);
    }

    public void AttackTarget()
    {
        if (Time.time > lastAttackTime + attackCooldown)
        {
            Debug.Log("Attacking the target!");
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
        // Idle 애니메이션 실행
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
        
        // HP가 가장 낮은 플레이어를 타겟으로 설정
        if (players.Count > 0)
        {
            List<PlayerServer> sortedPlayers = players.OrderByDescending(player => player.GetPlayerHP()).ToList();
            target = sortedPlayers[0];
        }
    }

    // 피격처리 
    public void PlayerGotHitOnServer(sbyte damage, ulong clientWhoAttacked)
    {
        // 피격 처리 총괄.
        playerHPManager.TakingDamage(damage, clientWhoAttacked);
        // 각 Client UI 업데이트 지시 Damage Text Popup
        wizardRukeAIClient.ShowDamageTextPopupClientRPC(damage);
        // 맞춘 플레이어 카메라 쉐이크. AI라면 쉐이크 시킬 필요 없습니다.
        NetworkClient networkClient = NetworkManager.ConnectedClients[clientWhoAttacked];
        if(networkClient.PlayerObject.TryGetComponent<PlayerClient>(out PlayerClient playerClient))
        {
            playerClient.ActivateHitCameraShakeClientRPC();
        }     
    }

    public sbyte GetPlayerHP()
    {
        return playerHPManager.GetHP();
    }

    public ICharacter GetCharacterData()
    {
        throw new NotImplementedException();
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
}

