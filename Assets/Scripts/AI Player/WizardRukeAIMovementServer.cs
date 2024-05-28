using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WizardRukeAIMovementServer : MonoBehaviour
{
    [SerializeField] private NavMeshAgent agent;

    [Header("순찰을 위한 변수들")]
    [SerializeField] private PlayerSpawnPointsController playerSpawnPointsController;
    [SerializeField] private int randomIndex;
    [SerializeField] private Transform patrolDestination;
    [SerializeField] private bool isChasing;
    [SerializeField] private Transform target; // 상대 오브젝트
    [SerializeField] private float minDesiredDistance = 4f; // 유지하고 싶은 최소 거리
    [SerializeField] private float maxDesiredDistance = 7f; // 유지하고 싶은 최대 거리

    public PlayerAnimator playerAnimator;

    private void Awake()
    {
        isChasing = false;
        playerSpawnPointsController = FindObjectOfType<PlayerSpawnPointsController>();
    }

    private void Update()
    {
        if (isChasing && target)
        {
            KeepDistance();
        }
    }

    public void StopMove()
    {
        agent.isStopped = true;
        isChasing= false;
    }

    public void ReduceMoveSpeed(float value)
    {
        agent.speed -= value;
    }

    public void AddMoveSpeed(float value)
    {
        agent.speed += value;
    }

    public void SetMoveSpeed(float value)
    {
        agent.speed = value;
    }

    public void MoveToTarget(Transform target)
    {
        isChasing = true;
        this.target = target;
    }

    public void Patrol()
    {
        isChasing = false;
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

    private void KeepDistance()
    {
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
            {
                Vector3 direction = (transform.position - target.position).normalized;
                Vector3 desiredPosition = target.position + direction * Random.Range(minDesiredDistance, maxDesiredDistance);

                // NavMesh 상의 유효한 위치인지 확인. desiredPosition과 근접한 가장 가까운 NavMesh 포인트를 반환해줍니다.
                NavMeshHit hit;
                if (NavMesh.SamplePosition(desiredPosition, out hit, 1.0f, NavMesh.AllAreas))
                {
                    agent.SetDestination(hit.position);
                }             
            }
        }
    }
}
