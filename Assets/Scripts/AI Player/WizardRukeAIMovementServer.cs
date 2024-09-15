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
    [SerializeField] private float minDesiredDistance = 3f; // 유지하고 싶은 최소 거리
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
                playerAnimator.UpdatePlayerAnimationOnServer(PlayerMoveAnimState.Walking);
            }
        }
    }

    // 현재로부터 일정 반경은 새로운 목적지로 하지 않는 기능을 추가한 버전
    private void KeepDistance()
    {
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
            {
                Vector3 newDestination = transform.position; // 기본 위치는 현재 위치
                float distanceToNewDestination;
                int attempts = 0;
                float minMoveDistance = 2f; // 최소 이동 거리 설정. 필요에 따라 조정하세요.

                do
                {
                    // 범위 무빙
                    // 목표를 기준으로 원주상의 임의의 지점을 선택
                    float randomAngle = Random.Range(0f, 360f);
                    float randomDistance = Random.Range(minDesiredDistance, maxDesiredDistance);

                    Vector3 direction = new Vector3(Mathf.Cos(randomAngle), 0, Mathf.Sin(randomAngle));
                    Vector3 desiredPosition = target.position + direction * randomDistance;

                    // NavMesh 상의 유효한 위치인지 확인
                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(desiredPosition, out hit, 1.0f, NavMesh.AllAreas))
                    {
                        newDestination = hit.position;
                        distanceToNewDestination = Vector3.Distance(transform.position, newDestination);
                    }
                    else
                    {
                        distanceToNewDestination = 0f; // 유효하지 않은 위치일 경우 다시 시도
                    }

                    attempts++;
                } while (distanceToNewDestination < minMoveDistance && attempts < 10); // 최대 10번 시도

                if (distanceToNewDestination >= minMoveDistance)
                {
                    agent.SetDestination(newDestination);
                }
                // 10번 시도해도 적절한 위치를 찾지 못했을 경우의 처리
                // 예: 현재 위치에 머무르거나, 다른 행동을 취하게 할 수 있습니다.
            }
        }
    }

    /*    private void KeepDistance()
        {
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                {
                    // 범위 무빙
                    // 목표를 기준으로 원주상의 임의의 지점을 선택
                    float randomAngle = Random.Range(0f, 360f);
                    float randomDistance = Random.Range(minDesiredDistance, maxDesiredDistance);

                    Vector3 direction = new Vector3(Mathf.Cos(randomAngle), 0, Mathf.Sin(randomAngle));
                    Vector3 desiredPosition = target.position + direction * randomDistance;

                    // NavMesh 상의 유효한 위치인지 확인. desiredPosition과 근접한 가장 가까운 NavMesh 포인트를 반환해줍니다.
                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(desiredPosition, out hit, 1.0f, NavMesh.AllAreas))
                    {
                        agent.SetDestination(hit.position);
                    }             
                }
            }
        }*/
}
