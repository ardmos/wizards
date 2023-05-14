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
        // 시네머신 카메라가 따라오도록 변경 // 카메라명 수정 필요. 아래 코드 수정할건 없는지 추후 확인 요망
        // 플레이어가 여럿인 경우, 각 플레이어에게 카메라가 하나씩 따라다닐 필요가 있음. 
/*        CinemachineVirtualCamera cinemachineVirtualCamera = GameObject.Find("Virtual Camera (1)").GetComponent<CinemachineVirtualCamera>();
        cinemachineVirtualCamera.Follow = transform;
        cinemachineVirtualCamera.LookAt = transform;*/
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
        Vector3 mouseDir = GetMouseDir();
        Rotate(mouseDir);
    }

    private void Rotate(Vector3 mMoveDir)
    {
        float rotateSpeed = 10f;
        Vector3 slerpResult = Vector3.Slerp(transform.forward, mMoveDir, Time.deltaTime * rotateSpeed);       
        transform.forward = slerpResult;
    }

    private Vector3 GetMouseDir()
    {
        RaycastHit hit;
        float maxDistance = 100f;
        Vector3 mousePos = Input.mousePosition;
        Ray mouseRay = Camera.main.ScreenPointToRay(mousePos);
        Vector3 mouseDir = Vector3.zero;
        if (Physics.Raycast(mouseRay.origin, mouseRay.direction, out hit, maxDistance))
        {
            mouseDir = hit.point - transform.position;
            //Debug.Log("mouseRay 충돌! 충돌 위치: " + hit.point);
        }
        else
        {
            Vector3 pos = mouseRay.GetPoint(maxDistance);
            Debug.Log("커서가 맵 밖으로 벗어났습니다  mouseRay.GetPoint : " + pos);
            mouseDir = pos - transform.position;
        }

        // 높이 좌표(y) 고정. 현재 캐릭터 허리 높이(0f)로. 모든 캐릭터 키는 1f로 균일. 땅이 0.5f만큼 꺼져있음. 
        mouseDir.y = 0f;
        return mouseDir;
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
