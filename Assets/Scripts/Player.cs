using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Cinemachine;

public class Player : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private GameInput gameInput;
    private bool isWalking;
    private bool isAttack1;

    private void Start()
    {
        // 시네머신 카메라가 따라오도록 변경 
        CinemachineVirtualCamera cinemachineVirtualCamera = GameObject.Find("Virtual Camera (1)").GetComponent<CinemachineVirtualCamera>();
        cinemachineVirtualCamera.Follow = transform;
        cinemachineVirtualCamera.LookAt = transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;

        Attack1();
        Move();     
    }

    private void Move()
    {
        Vector2 inputVector = gameInput.GetMovementVectorNormalized();

        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);
        transform.position += moveDir * moveSpeed * Time.deltaTime;
        isWalking = moveDir != Vector3.zero;

        // 진행방향 바라볼 때
        //Rotate(moveDir);
        // 마우스 커서 방향 바라볼 때
        /*Vector3 mouseWorldCoordinates = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 mouseDir = mouseWorldCoordinates - transform.position;
        Rotate(mouseDir);*/

        RaycastHit hit;
        //float maxDistance = 100f;
        Vector3 mousePos = Input.mousePosition;
        Ray mouseRay = Camera.main.ScreenPointToRay(mousePos);
        Vector3 mouseDir = Vector3.zero;
        if (Physics.Raycast(mouseRay.origin, mouseRay.direction, out hit)) {
            mouseDir = hit.point - transform.position;
            Debug.Log("mouseRay 충돌! 충돌 위치: " + hit.point);
        }
        else
        {
            Vector3 pos = mouseRay.GetPoint(100);
            Debug.Log("mouseRay.GetPoint : " + pos);
            mouseDir = pos - transform.position;
        }
        Rotate(mouseDir);
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
        //Debug.Log("isAttack1 = " + isAttack1);
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
