using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Cinemachine;

public class Player : NetworkBehaviour, IStoreCustomer
{

    #region Private
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private GameInput gameInput;
    [SerializeField] private GameObject virtualCameraObj;
    [SerializeField] private Mesh m_Body;
    [SerializeField] private Mesh m_Hat;
    [SerializeField] private Mesh m_BackPack;
    [SerializeField] private Mesh m_Wand;

    private GameAssets gameAssets;

    private bool isWalking;
    private bool isAttack1;
    private int body;
    private int hat;
    private int backPack;
    private int wand;

    private void UpdateEquipments()
    {
        UpdateEquipments(body, hat, backPack, wand);
    }

    private void UpdateEquipments(int body, int hat, int backPack, int wand) {

        switch (body)
        {
            case 1: m_Body = gameAssets.m_Body_1; break;
            case 2: m_Body = gameAssets.m_Body_2; break;
            case 3: m_Body = gameAssets.m_Body_3; break;
            case 4: m_Body = gameAssets.m_Body_4; break;
            case 5: m_Body = gameAssets.m_Body_5; break;
            case 6: m_Body = gameAssets.m_Body_6; break;
            case 7: m_Body = gameAssets.m_Body_7; break;
            case 8: m_Body = gameAssets.m_Body_8; break;
            case 9: m_Body = gameAssets.m_Body_9; break;
           
            default:
                Debug.LogError("Body Armor Equipment Error. body:"+body);
                break;
        }
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
    #endregion Private


    public override void OnNetworkSpawn()
    {
        // 카메라가 소유자만 따라다니도록 함 
        virtualCameraObj.SetActive(IsOwner);
    }

    void Start()
    {
        gameAssets = GameAssets.instantiate;
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;

        Attack1();
        Move();     
    }

    public bool IsWalking()
    {
        return isWalking;
    }
    public bool IsAttack1()
    {
        return isAttack1;
    }

    public void EquipArmor_1()
    {
        body = 1;
        UpdateEquipments();
    }
    public void EquipHat_1()
    {
        hat = 1;
        UpdateEquipments();
    }
    public void EquipBackPack_1()
    {
        backPack = 1;
        UpdateEquipments();
    }
    public void EquipWand_1()
    {
        wand = 1;
        UpdateEquipments();
    }


    /// <summary>
    /// 아이템 구매 메서드
    /// </summary>
    public void BoughtItem(Item.ItemType itemType)
    {
        switch (itemType)
        {
            case Item.ItemType.Armor_1:

                break;
            case Item.ItemType.Hat_1:
                break;
            case Item.ItemType.BackPack_1:
                break;
            case Item.ItemType.Wand_1:
                break;
            case Item.ItemType.Scroll_1:
                break;
            default:
                break;
        }

    }
}
