using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary> 
/// 1. Wizard 스탯 관리
/// </summary>
public class Wizard : Player
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
        if (!IsOwner) return;

        UpdateAttackInput();
        // Server Auth 방식의 이동 처리
        HandleMovementServerAuth();
    }

    private void InitStat()
    {
        hp = 5;
        moveSpeed = 7f;

    }
}
