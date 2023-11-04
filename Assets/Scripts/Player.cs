using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Cinemachine;

public class Player : NetworkBehaviour, IStoreCustomer
{
    public static Player LocalInstance { get; private set; }

    [SerializeField] private GameInput gameInput;
    [SerializeField] private GameObject virtualCameraObj;
    [SerializeField] private SpellController spellController;
    [SerializeField] private HPBarUI hPBar;
    [SerializeField] private Mesh m_Body;
    [SerializeField] private Mesh m_Hat;
    [SerializeField] private Mesh m_BackPack;
    [SerializeField] private Mesh m_Wand;
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private int hp = 5;
    [SerializeField] private int score = 0;
    [SerializeField] private List<Vector3> spawnPositionList;

    private GameAssets gameAssets;

    private bool isWalking;
    private bool isAttack1;
    private bool isAttack2;
    private bool isAttack3;
    private int body;
    private int hat;
    private int backPack;
    private int wand;

    // Update is called once per frame
    private void Update()
    {
        if (!IsOwner) return;

        UpdateAttackInput();
        // Server Auth ����� �̵� ó��
        HandleMovementServerAuth();
        // Client Auth ����� �̵� ó��
        //HandleMovement(); 
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner) LocalInstance = this;

        // HP �ʱ�ȭ
        hPBar.SetHP(hp);

        // ī�޶� ��ġ �ʱ�ȭ. �����ڸ� ����ٴϵ��� �� 
        virtualCameraObj.SetActive(IsOwner);
        gameAssets = GameAssets.instantiate;

        // ���� ��ġ �ʱ�ȭ
        transform.position = spawnPositionList[GameMultiplayer.Instance.GetPlayerDataIndexFromClientId(OwnerClientId)];
    }

    public void GetHit(int damage)
    {
        // HP ����
        hp--;
        // HP �� ����
        hPBar.SetHP(hp);
    }

    public int GetScore()
    {
        return score;
    }

    public bool IsWalking()
    {
        return isWalking;
    }
    public bool IsAttack1()
    {
        return isAttack1;
    }
    public bool IsAttack2()
    {
        return isAttack2;
    }
    public bool IsAttack3()
    {
        return isAttack3;
    }

    #region Public ������ ����
    /// <summary>
    /// ������ ���� �޼���
    /// </summary>
    public void BoughtSpellScroll(Item.ItemType itemType, int slotNumber)
    {
        Debug.Log("Bought spell: " + itemType + ", slotNum : " + slotNumber);
        switch (itemType)
        {
            case Item.ItemType.FireBall_1:
                spellController.SetCurrentSpell(gameAssets.fireBall_1, slotNumber);
                break;
            case Item.ItemType.WaterBall_1:
                spellController.SetCurrentSpell(gameAssets.waterBall_1, slotNumber);
                break;
            case Item.ItemType.IceBall_1:
                spellController.SetCurrentSpell(gameAssets.iceBall_1, slotNumber);
                break;
            default:
                break;
        }

    }
    public void BoughtItem(Item.ItemType itemType)
    {
        Debug.Log("Bought item: " + itemType);
        switch (itemType)
        {
            case Item.ItemType.Armor_1: EquipArmor_1(); break;
            case Item.ItemType.Armor_2: EquipArmor_2(); break;
            case Item.ItemType.Armor_3: EquipArmor_3(); break;
            case Item.ItemType.Armor_4: EquipArmor_4(); break;
            case Item.ItemType.Armor_5: EquipArmor_5(); break;
            case Item.ItemType.Armor_6: EquipArmor_6(); break;
            case Item.ItemType.Armor_7: EquipArmor_7(); break;
            case Item.ItemType.Armor_8: EquipArmor_8(); break;
            case Item.ItemType.Armor_9: EquipArmor_9(); break;
            case Item.ItemType.Armor_10: EquipArmor_10(); break;
            case Item.ItemType.Armor_11: EquipArmor_11(); break;
            case Item.ItemType.Armor_12: EquipArmor_12(); break;
            case Item.ItemType.Armor_13: EquipArmor_13(); break;
            case Item.ItemType.Armor_14: EquipArmor_14(); break;
            case Item.ItemType.Armor_15: EquipArmor_15(); break;
            case Item.ItemType.Armor_16: EquipArmor_16(); break;
            case Item.ItemType.Armor_17: EquipArmor_17(); break;
            case Item.ItemType.Armor_18: EquipArmor_18(); break;
            case Item.ItemType.Armor_19: EquipArmor_19(); break;
            case Item.ItemType.Armor_20: EquipArmor_20(); break;
            case Item.ItemType.Hat_1: EquipHat_1(); break;
            case Item.ItemType.Hat_2: EquipHat_2(); break;
            case Item.ItemType.Hat_3: EquipHat_3(); break;
            case Item.ItemType.Hat_4: EquipHat_4(); break;
            case Item.ItemType.Hat_5: EquipHat_5(); break;
            case Item.ItemType.Hat_6: EquipHat_6(); break;
            case Item.ItemType.Hat_7: EquipHat_7(); break;
            case Item.ItemType.Hat_8: EquipHat_8(); break;
            case Item.ItemType.Hat_9: EquipHat_9(); break;
            case Item.ItemType.Hat_10: EquipHat_10(); break;
            case Item.ItemType.Hat_11: EquipHat_11(); break;
            case Item.ItemType.Hat_12: EquipHat_12(); break;
            case Item.ItemType.Hat_13: EquipHat_13(); break;
            case Item.ItemType.Hat_14: EquipHat_14(); break;
            case Item.ItemType.BackPack_1: EquipBackPack_1(); break;
            case Item.ItemType.BackPack_2: EquipBackPack_2(); break;
            case Item.ItemType.BackPack_3: EquipBackPack_3(); break;
            case Item.ItemType.Wand_1: EquipWand_1(); break;
            case Item.ItemType.Wand_2: EquipWand_2(); break;
            case Item.ItemType.Wand_3: EquipWand_3(); break;
            case Item.ItemType.Wand_4: EquipWand_4(); break;
            case Item.ItemType.Wand_5: EquipWand_5(); break;
            case Item.ItemType.Wand_6: EquipWand_6(); break;
            case Item.ItemType.Wand_7: EquipWand_7(); break;
            default:
                break;
        }

    }
    #endregion

    #region Private ������ ����
    private void UpdateEquipments()
    {
        UpdateEquipments(body, hat, backPack, wand);
    }

    private void UpdateEquipments(int body, int hat, int backPack, int wand)
    {

        switch (body)
        {
            case 0: break;
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
                Debug.LogError("Body Armor Equipment Error. body:" + body);
                break;
        }

        switch (hat)
        {
            case 0: break;
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
            case 0: break;
            case 1: m_BackPack = gameAssets.m_BackPack_1; break;
            case 2: m_BackPack = gameAssets.m_BackPack_2; break;
            case 3: m_BackPack = gameAssets.m_BackPack_3; break;

            default:
                Debug.LogError("BackPack Equipment Error. backPack:" + backPack);
                break;
        }

        switch (wand)
        {
            case 0: break;
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

    private void EquipArmor_1()
    {
        body = 1;
        UpdateEquipments();
    }
    private void EquipArmor_2()
    {
        body = 2;
        UpdateEquipments();
    }
    private void EquipArmor_3()
    {
        body = 3;
        UpdateEquipments();
    }
    private void EquipArmor_4()
    {
        body = 4;
        UpdateEquipments();
    }
    private void EquipArmor_5()
    {
        body = 5;
        UpdateEquipments();
    }
    private void EquipArmor_6()
    {
        body = 6;
        UpdateEquipments();
    }
    private void EquipArmor_7()
    {
        body = 7;
        UpdateEquipments();
    }
    private void EquipArmor_8()
    {
        body = 8;
        UpdateEquipments();
    }
    private void EquipArmor_9()
    {
        body = 9;
        UpdateEquipments();
    }
    private void EquipArmor_10()
    {
        body = 10;
        UpdateEquipments();
    }
    private void EquipArmor_11()
    {
        body = 11;
        UpdateEquipments();
    }
    private void EquipArmor_12()
    {
        body = 12;
        UpdateEquipments();
    }
    private void EquipArmor_13()
    {
        body = 13;
        UpdateEquipments();
    }
    private void EquipArmor_14()
    {
        body = 14;
        UpdateEquipments();
    }
    private void EquipArmor_15()
    {
        body = 15;
        UpdateEquipments();
    }
    private void EquipArmor_16()
    {
        body = 16;
        UpdateEquipments();
    }
    private void EquipArmor_17()
    {
        body = 17;
        UpdateEquipments();
    }
    private void EquipArmor_18()
    {
        body = 18;
        UpdateEquipments();
    }
    private void EquipArmor_19()
    {
        body = 19;
        UpdateEquipments();
    }
    private void EquipArmor_20()
    {
        body = 20;
        UpdateEquipments();
    }
    private void EquipHat_1()
    {
        hat = 1;
        UpdateEquipments();
    }
    private void EquipHat_2()
    {
        hat = 2;
        UpdateEquipments();
    }
    private void EquipHat_3()
    {
        hat = 3;
        UpdateEquipments();
    }
    private void EquipHat_4()
    {
        hat = 4;
        UpdateEquipments();
    }
    private void EquipHat_5()
    {
        hat = 5;
        UpdateEquipments();
    }
    private void EquipHat_6()
    {
        hat = 6;
        UpdateEquipments();
    }
    private void EquipHat_7()
    {
        hat = 7;
        UpdateEquipments();
    }
    private void EquipHat_8()
    {
        hat = 8;
        UpdateEquipments();
    }
    private void EquipHat_9()
    {
        hat = 9;
        UpdateEquipments();
    }
    private void EquipHat_10()
    {
        hat = 10;
        UpdateEquipments();
    }
    private void EquipHat_11()
    {
        hat = 11;
        UpdateEquipments();
    }
    private void EquipHat_12()
    {
        hat = 12;
        UpdateEquipments();
    }
    private void EquipHat_13()
    {
        hat = 13;
        UpdateEquipments();
    }
    private void EquipHat_14()
    {
        hat = 14;
        UpdateEquipments();
    }
    private void EquipBackPack_1()
    {
        backPack = 1;
        UpdateEquipments();
    }
    private void EquipBackPack_2()
    {
        backPack = 2;
        UpdateEquipments();
    }
    private void EquipBackPack_3()
    {
        backPack = 3;
        UpdateEquipments();
    }
    private void EquipWand_1()
    {
        wand = 1;
        UpdateEquipments();
    }
    private void EquipWand_2()
    {
        wand = 2;
        UpdateEquipments();
    }
    private void EquipWand_3()
    {
        wand = 3;
        UpdateEquipments();
    }
    private void EquipWand_4()
    {
        wand = 4;
        UpdateEquipments();
    }
    private void EquipWand_5()
    {
        wand = 5;
        UpdateEquipments();
    }
    private void EquipWand_6()
    {
        wand = 6;
        UpdateEquipments();
    }
    private void EquipWand_7()
    {
        wand = 7;
        UpdateEquipments();
    }
    #endregion

    #region Private ĳ���� ����
    // Server Auth ����� �̵� ó�� (�� ������Ʈ�� Network Transform�� �ʿ�)
    private void HandleMovementServerAuth()
    {
        Vector2 inputVector = gameInput.GetMovementVectorNormalized();
        HandleMovementServerRPC(inputVector);
    }
    [ServerRpc(RequireOwnership = false)]
    private void HandleMovementServerRPC(Vector2 inputVector)
    {
        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);

        float moveDistance = moveSpeed * Time.deltaTime;
        transform.position += moveDir * moveDistance;
        isWalking = moveDir != Vector3.zero;

        // ������� �ٶ� ��
        Rotate(moveDir);

        // ���콺 Ŀ�� ���� �ٶ� ��
        //Vector3 mouseDir = GetMouseDir();
        //Rotate(mouseDir);
    }

    // Client Auth ����� �̵� ó�� (�� ������Ʈ�� Client Network Transform�� �ʿ�)
    private void HandleMovement()
    {
        Vector2 inputVector = gameInput.GetMovementVectorNormalized();

        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);

        float moveDistance = moveSpeed * Time.deltaTime;
        transform.position += moveDir * moveDistance;
        isWalking = moveDir != Vector3.zero;

        // ������� �ٶ� ��
        Rotate(moveDir);

        // ���콺 Ŀ�� ���� �ٶ� ��
        //Vector3 mouseDir = GetMouseDir();
        //Rotate(mouseDir);
    }

    private void Rotate(Vector3 mMoveDir)
    {
        float rotateSpeed = 20f;
        Vector3 slerpResult = Vector3.Slerp(transform.forward, mMoveDir, Time.deltaTime * rotateSpeed);
        transform.forward = slerpResult;
    }

    // ���콺 Ŀ�� ���� �ٶ� �� ����ߴ� �޼ҵ�. ���� ���� ����
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
            //Debug.Log("Ŀ���� �� ������ ������ϴ�  mouseRay.GetPoint : " + pos);
            mouseDir = pos - transform.position;
        }

        // ���� ��ǥ(y) ����. ���� ĳ���� �㸮 ����(0f)��. ��� ĳ���� Ű�� 1f�� ����. ���� 0.5f��ŭ ��������. 
        mouseDir.y = 0f;
        return mouseDir;
    }

    private void UpdateAttackInput()
    {
        isAttack1 = gameInput.GetIsAttack1() == 1;
        isAttack2 = gameInput.GetIsAttack2() == 1;
        isAttack3 = gameInput.GetIsAttack3() == 1;
    }
    #endregion
}
