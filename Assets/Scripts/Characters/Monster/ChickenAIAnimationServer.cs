using UnityEngine;
using UnityEngine.AI;

public class ChickenAIAnimationServer : MonoBehaviour
{
    public NavMeshAgent agent;
    public Animator animator;

    public float moveThreshold = 0.1f;

    void Update()
    {
        // NavMeshAgent의 속도를 체크하여 이동 중인지 확인
        bool isMoving = agent.velocity.magnitude > moveThreshold;

        // Animator의 IsWalking 파라미터 업데이트
        animator.SetBool("IsWalking", isMoving);
    }
}
