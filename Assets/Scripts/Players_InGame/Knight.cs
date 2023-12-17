using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 1. Knight 스탯 관리
/// </summary>
public class Knight : Player
{
    // Start is called before the first frame update
    void Start()
    {
        InitStat();
        GetComponent<Rigidbody>().isKinematic = false;
    }

    // Update is called once per frame
    void Update()
    {
        //if (!IsOwner) return;

        UpdateAttackInput();
        // Server Auth 방식의 이동 처리
        //HandleMovementServerAuth();
        //테스트용 Client Auth 방식 이동 처리
        HandleMovement(); 
    }

    private void InitStat()
    {
        hp = 7;
        moveSpeed = 7f;
    }
}
