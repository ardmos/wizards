using UnityEngine;
using UnityEngine.AI;

public class ChickenAIAnimationServer : MonoBehaviour
{
    public NavMeshAgent agent;
    public Animator animator;

    public float moveThreshold = 0.1f;

    void Update()
    {
        // NavMeshAgent�� �ӵ��� üũ�Ͽ� �̵� ������ Ȯ��
        bool isMoving = agent.velocity.magnitude > moveThreshold;

        // Animator�� IsWalking �Ķ���� ������Ʈ
        animator.SetBool("IsWalking", isMoving);
    }
}
