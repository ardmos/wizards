using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary> 
/// 1. Wizard ���� ����
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
        // Server Auth ����� �̵� ó��
        HandleMovementServerAuth();
    }

    private void InitStat()
    {
        hp = 5;
        moveSpeed = 7f;

    }
}
