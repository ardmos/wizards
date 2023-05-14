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
        // �ó׸ӽ� ī�޶� ��������� ���� // ī�޶�� ���� �ʿ�. �Ʒ� �ڵ� �����Ұ� ������ ���� Ȯ�� ���
        // �÷��̾ ������ ���, �� �÷��̾�� ī�޶� �ϳ��� ����ٴ� �ʿ䰡 ����. 
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

        // ������� �ٶ� ��
        //Rotate(moveDir);

        // ���콺 Ŀ�� ���� �ٶ� ��
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
            //Debug.Log("mouseRay �浹! �浹 ��ġ: " + hit.point);
        }
        else
        {
            Vector3 pos = mouseRay.GetPoint(maxDistance);
            Debug.Log("Ŀ���� �� ������ ������ϴ�  mouseRay.GetPoint : " + pos);
            mouseDir = pos - transform.position;
        }

        // ���� ��ǥ(y) ����. ���� ĳ���� �㸮 ����(0f)��. ��� ĳ���� Ű�� 1f�� ����. ���� 0.5f��ŭ ��������. 
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
