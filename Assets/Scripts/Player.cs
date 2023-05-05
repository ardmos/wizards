using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private GameInput gameInput;
    private bool isWalking;
    private bool isAttack1;


    // Update is called once per frame
    void Update()
    {
        Attack1();
        Move();
    }

    private void Move()
    {
        Vector2 inputVector = gameInput.GetMovementVectorNormalized();

        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);
        transform.position += moveDir * moveSpeed * Time.deltaTime;
        isWalking = moveDir != Vector3.zero;

        Rotate(moveDir);
    }

    private void Rotate(Vector3 mMoveDir)
    {
        float rotateSpeed = 10f;
        Vector3 slerpResult = Vector3.Slerp(transform.forward, mMoveDir, Time.deltaTime * rotateSpeed);       
        transform.forward = slerpResult;
    }

    private void Attack1()
    {           
        isAttack1 = gameInput.GetIsAttack1() == 1;
        Debug.Log("isAttack1 = " + isAttack1);
    }

    public bool IsWalking()
    {
        return isWalking;
    }
    public bool IsAttack1()
    {
        return isAttack1;
    }
}
