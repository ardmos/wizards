using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WizardRukeAIMovementServer : MonoBehaviour
{
    [SerializeField] private NavMeshAgent agent;

    [Header("������ ���� ������")]
    [SerializeField] private PlayerSpawnPointsController playerSpawnPointsController;
    [SerializeField] private int randomIndex;
    [SerializeField] private Transform patrolDestination;
    [SerializeField] private bool isChasing;
    [SerializeField] private Transform target; // ��� ������Ʈ
    [SerializeField] private float minDesiredDistance = 4f; // �����ϰ� ���� �ּ� �Ÿ�
    [SerializeField] private float maxDesiredDistance = 7f; // �����ϰ� ���� �ִ� �Ÿ�

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

                // NavMesh ���� ��ȿ�� ��ġ���� Ȯ��. desiredPosition�� ������ ���� ����� NavMesh ����Ʈ�� ��ȯ���ݴϴ�.
                NavMeshHit hit;
                if (NavMesh.SamplePosition(desiredPosition, out hit, 1.0f, NavMesh.AllAreas))
                {
                    agent.SetDestination(hit.position);
                }             
            }
        }
    }
}
