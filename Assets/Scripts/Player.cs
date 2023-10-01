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
            case 10: m_Body = gameAssets.m_Body_10; break;
            case 12: m_Body = gameAssets.m_Body_12; break;
            case 13: m_Body = gameAssets.m_Body_13; break;
            case 14: m_Body = gameAssets.m_Body_14; break;
            case 15: m_Body = gameAssets.m_Body_15; break;
            case 16: m_Body = gameAssets.m_Body_16; break;
            case 17: m_Body = gameAssets.m_Body_17; break;
            case 18: m_Body = gameAssets.m_Body_18; break;
            case 19: m_Body = gameAssets.m_Body_19; break;
            case 20: m_Body = gameAssets.m_Body_20; break;

            default:
                Debug.LogError("Body Armor Equipment Error. body:"+body);
                break;
        }

        switch (hat)
        {
            case 1: m_Hat = gameAssets.m_Hat_1; break;  
            case 2: m_Hat = gameAssets.m_Hat_2; break;
            case 3: m_Hat = gameAssets.m_Hat_3; break;
            case 4: m_Hat = gameAssets.m_Hat_4; break;
            case 5: m_Hat = gameAssets.m_Hat_5; break;
            case 6: m_Hat = gameAssets.m_Hat_6; break;
            case 7: m_Hat = gameAssets.m_Hat_7; break;
            case 8: m_Hat = gameAssets.m_Hat_8; break;
            case 9: m_Hat = gameAssets.m_Hat_9; break;
            case 10: m_Hat = gameAssets.m_Hat_10; break;
            case 11: m_Hat = gameAssets.m_Hat_11; break;
            case 12: m_Hat = gameAssets.m_Hat_12; break;
            case 13: m_Hat = gameAssets.m_Hat_13; break;
            case 14: m_Hat = gameAssets.m_Hat_14; break;

            default:
                Debug.LogError("Hat Equipment Error. hat:" + hat);
                break;
        }

        switch (backPack)
        {
            case 1: m_BackPack = gameAssets.m_BackPack_1; break;
            case 2: m_BackPack = gameAssets.m_BackPack_2; break;
            case 3: m_BackPack = gameAssets.m_BackPack_3; break;

            default:
                Debug.LogError("BackPack Equipment Error. backPack:" + backPack);
                break;
        }

        switch (wand)
        {
            case 1: m_Wand = gameAssets.m_Wand_1; break;
            case 2: m_Wand = gameAssets.m_Wand_2; break;
            case 3: m_Wand = gameAssets.m_Wand_3; break;
            case 4: m_Wand = gameAssets.m_Wand_4; break;
            case 5: m_Wand = gameAssets.m_Wand_5; break;
            case 6: m_Wand = gameAssets.m_Wand_6; break;
            case 7: m_Wand = gameAssets.m_Wand_7; break;
            
            default:
                Debug.LogError("Wand Equipment Error. wand:" + wand);
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
    public void EquipArmor_2()
    {
        body = 2;
        UpdateEquipments();
    }
    public void EquipArmor_3()
    {
        body = 3;
        UpdateEquipments();
    }
    public void EquipArmor_4()
    {
        body = 4;
        UpdateEquipments();
    }
    public void EquipArmor_5()
    {
        body = 5;
        UpdateEquipments();
    }
    public void EquipArmor_6()
    {
        body = 6;
        UpdateEquipments();
    }
    public void EquipArmor_7()
    {
        body = 7;
        UpdateEquipments();
    }
    public void EquipArmor_8()
    {
        body = 8;
        UpdateEquipments();
    }
    public void EquipArmor_9()
    {
        body = 9;
        UpdateEquipments();
    }
    public void EquipArmor_10()
    {
        body = 10;
        UpdateEquipments();
    }
    public void EquipArmor_11()
    {
        body = 11;
        UpdateEquipments();
    }
    public void EquipArmor_12()
    {
        body = 12;
        UpdateEquipments();
    }
    public void EquipArmor_13()
    {
        body = 13;
        UpdateEquipments();
    }
    public void EquipArmor_14()
    {
        body = 14;
        UpdateEquipments();
    }
    public void EquipArmor_15()
    {
        body = 15;
        UpdateEquipments();
    }
    public void EquipArmor_16()
    {
        body = 16;
        UpdateEquipments();
    }
    public void EquipArmor_17()
    {
        body = 17;
        UpdateEquipments();
    }
    public void EquipArmor_18()
    {
        body = 18;
        UpdateEquipments();
    }
    public void EquipArmor_19()
    {
        body = 19;
        UpdateEquipments();
    }
    public void EquipArmor_20()
    {
        body = 20;
        UpdateEquipments();
    }
    public void EquipHat_1()
    {
        hat = 1;
        UpdateEquipments();
    }
    public void EquipHat_2()
    {
        hat = 2;
        UpdateEquipments();
    }
    public void EquipHat_3()
    {
        hat = 3;
        UpdateEquipments();
    }
    public void EquipHat_4()
    {
        hat = 4;
        UpdateEquipments();
    }
    public void EquipHat_5()
    {
        hat = 5;
        UpdateEquipments();
    }
    public void EquipHat_6()
    {
        hat = 6;
        UpdateEquipments();
    }
    public void EquipHat_7()
    {
        hat = 7;
        UpdateEquipments();
    }
    public void EquipHat_8()
    {
        hat = 8;
        UpdateEquipments();
    }
    public void EquipHat_9()
    {
        hat = 9;
        UpdateEquipments();
    }
    public void EquipHat_10()
    {
        hat = 10;
        UpdateEquipments();
    }
    public void EquipHat_11()
    {
        hat = 11;
        UpdateEquipments();
    }
    public void EquipHat_12()
    {
        hat = 12;
        UpdateEquipments();
    }
    public void EquipHat_13()
    {
        hat = 13;
        UpdateEquipments();
    }
    public void EquipHat_14()
    {
        hat = 14;
        UpdateEquipments();
    }
    public void EquipBackPack_1()
    {
        backPack = 1;
        UpdateEquipments();
    }
    public void EquipBackPack_2()
    {
        backPack = 2;
        UpdateEquipments();
    }
    public void EquipBackPack_3()
    {
        backPack = 3;
        UpdateEquipments();
    }
    public void EquipWand_1()
    {
        wand = 1;
        UpdateEquipments();
    }
    public void EquipWand_2()
    {
        wand = 2;
        UpdateEquipments();
    }
    public void EquipWand_3()
    {
        wand = 3;
        UpdateEquipments();
    }
    public void EquipWand_4()
    {
        wand = 4;
        UpdateEquipments();
    }
    public void EquipWand_5()
    {
        wand = 5;
        UpdateEquipments();
    }
    public void EquipWand_6()
    {
        wand = 6;
        UpdateEquipments();
    }
    public void EquipWand_7()
    {
        wand = 7;
        UpdateEquipments();
    }


    /// <summary>
    /// 아이템 구매 메서드
    /// </summary>
    public void BoughtItem(Item.ItemType itemType)
    {
       

    }
}
