using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 1. Knight ���� ����
/// </summary>
public class Knight : Player
{
    // Start is called before the first frame update
    void Start()
    {
        InitStat();
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
        hp = 7;
        moveSpeed = 7f;
    }
}
