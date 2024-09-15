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
    [SerializeField] private float minDesiredDistance = 3f; // �����ϰ� ���� �ּ� �Ÿ�
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
                playerAnimator.UpdatePlayerAnimationOnServer(PlayerMoveAnimState.Walking);
            }
        }
    }

    // ����κ��� ���� �ݰ��� ���ο� �������� ���� �ʴ� ����� �߰��� ����
    private void KeepDistance()
    {
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
            {
                Vector3 newDestination = transform.position; // �⺻ ��ġ�� ���� ��ġ
                float distanceToNewDestination;
                int attempts = 0;
                float minMoveDistance = 2f; // �ּ� �̵� �Ÿ� ����. �ʿ信 ���� �����ϼ���.

                do
                {
                    // ���� ����
                    // ��ǥ�� �������� ���ֻ��� ������ ������ ����
                    float randomAngle = Random.Range(0f, 360f);
                    float randomDistance = Random.Range(minDesiredDistance, maxDesiredDistance);

                    Vector3 direction = new Vector3(Mathf.Cos(randomAngle), 0, Mathf.Sin(randomAngle));
                    Vector3 desiredPosition = target.position + direction * randomDistance;

                    // NavMesh ���� ��ȿ�� ��ġ���� Ȯ��
                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(desiredPosition, out hit, 1.0f, NavMesh.AllAreas))
                    {
                        newDestination = hit.position;
                        distanceToNewDestination = Vector3.Distance(transform.position, newDestination);
                    }
                    else
                    {
                        distanceToNewDestination = 0f; // ��ȿ���� ���� ��ġ�� ��� �ٽ� �õ�
                    }

                    attempts++;
                } while (distanceToNewDestination < minMoveDistance && attempts < 10); // �ִ� 10�� �õ�

                if (distanceToNewDestination >= minMoveDistance)
                {
                    agent.SetDestination(newDestination);
                }
                // 10�� �õ��ص� ������ ��ġ�� ã�� ������ ����� ó��
                // ��: ���� ��ġ�� �ӹ����ų�, �ٸ� �ൿ�� ���ϰ� �� �� �ֽ��ϴ�.
            }
        }
    }

    /*    private void KeepDistance()
        {
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                {
                    // ���� ����
                    // ��ǥ�� �������� ���ֻ��� ������ ������ ����
                    float randomAngle = Random.Range(0f, 360f);
                    float randomDistance = Random.Range(minDesiredDistance, maxDesiredDistance);

                    Vector3 direction = new Vector3(Mathf.Cos(randomAngle), 0, Mathf.Sin(randomAngle));
                    Vector3 desiredPosition = target.position + direction * randomDistance;

                    // NavMesh ���� ��ȿ�� ��ġ���� Ȯ��. desiredPosition�� ������ ���� ����� NavMesh ����Ʈ�� ��ȯ���ݴϴ�.
                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(desiredPosition, out hit, 1.0f, NavMesh.AllAreas))
                    {
                        agent.SetDestination(hit.position);
                    }             
                }
            }
        }*/
}
