using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class ChickenAIMovementServer : NetworkBehaviour
{
    public NavMeshAgent agent;
    public float wanderRadius = 3f;
    public float minWanderTime = 3f;
    public float maxWanderTime = 10f;

    private Vector3 startPosition;
    private float timer;

    void Start()
    {
        startPosition = transform.position;
        SetNewDestination();
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            SetNewDestination();
        }
    }

    void SetNewDestination()
    {
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection += startPosition;

        NavMeshHit hit;
        NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, NavMesh.AllAreas);

        agent.SetDestination(hit.position);

        timer = Random.Range(minWanderTime, maxWanderTime);
    }

    public void StopMove()
    {
        agent.isStopped = true;
    }

    public void ReduceMoveSpeed(float value)
    {
        agent.speed -= value;
    }

    public void AddMoveSpeed(float value)
    {
        agent.speed += value;
    }
}
