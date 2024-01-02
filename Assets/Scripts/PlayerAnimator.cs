using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    private const string IS_WALKING = "IsWalking";
    private const string IS_ATTACK1 = "IsAttack1";

    [SerializeField] private Player player;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        //Debug.Log($"is walking? {player.IsWalking()}");
        animator.SetBool(IS_WALKING, player.IsWalking());
        // 애니메이션 변경이 필요합니다. 단계 구분. 1.캐스팅 2.발사
        //animator.SetBool(IS_ATTACK1, player.IsAttack1());
    }


}
