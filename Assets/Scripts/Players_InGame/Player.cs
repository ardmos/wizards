using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
/// <summary>
/// 1. 공통적인 플레이어 정보 관리
/// </summary>
public class Player : NetworkBehaviour, IStoreCustomer
{
    public static Player LocalInstance { get; private set; }


    [SerializeField] protected GameInput gameInput;
    [SerializeField] protected GameObject virtualCameraObj;
    [SerializeField] protected SpellController spellController;
    [SerializeField] protected HPBarUI hPBar;
    [SerializeField] protected Mesh m_Body;
    [SerializeField] protected Mesh m_Hat;
    [SerializeField] protected Mesh m_BackPack;
    [SerializeField] protected Mesh m_Wand;
    [SerializeField] protected float moveSpeed;
    [SerializeField] protected sbyte hp;
    [SerializeField] protected int score = 0;
    [SerializeField] protected List<Vector3> spawnPositionList;    

    protected GameAssets gameAssets;

    //protected bool isWalking;
    protected bool isBtnAttack1Clicked;
    protected bool isBtnAttack2Clicked;
    protected bool isBtnAttack3Clicked;
    protected int body;
    protected int hat;
    protected int backPack;
    protected int wand;

    // 플레이어 조작 제어를 위한 변수
    [SerializeField] private bool isPlayerGameOver;   

    /// <summary>
    /// 서버측 InitializePlayer
    /// 1. 스폰위치 초기화
    /// 2. HP 초기화 & 브로드캐스팅
    /// 3. 특정 플레이어가 보유한 스킬 목록 저장 & 해당플레이어에게 공유
    /// </summary>
    /// <param name="ownedSpellList"></param>
    public void InitializePlayerOnServer(SpellName[] ownedSpellList, ulong requestedInitializeClientId)
    {
        gameAssets = GameAssets.instantiate;

        Debug.Log($"InitializePlayerOnServer. spawnPositionList.Count: {spawnPositionList.Count}, requestedInitializeClientId: {requestedInitializeClientId}, GameMultiplayer.Instance.GetPlayerDataIndexFromClientId: {GameMultiplayer.Instance.GetPlayerDataIndexFromClientId(requestedInitializeClientId)}");

        // 스폰 위치 초기화
        //transform.position = spawnPositionList[GameMultiplayer.Instance.GetPlayerDataIndexFromClientId(OwnerClientId)];
        transform.position = spawnPositionList[0];

        // HP 초기화 & 브로드캐스팅
        PlayerHPManager.Instance.SetPlayerHPOnServer(hp, requestedInitializeClientId);

        // 특정 플레이어가 보유한 스킬 목록 저장


        // Spawn된 클라이언트측 InitializePlayer 시작 & 보유 스킬 리스트 공유
        NetworkClient networkClient = NetworkManager.ConnectedClients[requestedInitializeClientId];
        networkClient.PlayerObject.GetComponent<Player>().InitializePlayerClientRPC(ownedSpellList);
    }

    [ClientRpc]
    private void InitializePlayerClientRPC(SpellName[] ownedSpellNameList)
    {
        // 카메라 위치 초기화. 소유자만 따라다니도록 함 
        virtualCameraObj.SetActive(IsOwner);
        // 자꾸 isKinematic이 켜져서 추가한 코드. Rigidbody network에서 계속 켜는 것 같다.
        GetComponent<Rigidbody>().isKinematic = false;

        if (!IsOwner) return;
        LocalInstance = this;

        Debug.Log($"InitializePlayerClientRPC. Player{OwnerClientId}! IsClient?{IsClient}, IsOwner?{IsOwner}, LocalInstance:{LocalInstance}");


        // 테스트용
        //GameManager.Instance.UpdatePlayerGameOver();

        // 보유 스킬 로드          
        Debug.Log($"ownedSpellNameList.Length : {ownedSpellNameList.Length}");
        spellController.SetCurrentSpellOnClient(ownedSpellNameList);
        
        // Input Action 이벤트 구독
        gameInput.OnAttack1Started += GameInput_OnAttack1Started;
        gameInput.OnAttack2Started += GameInput_OnAttack2Started;
        gameInput.OnAttack3Started += GameInput_OnAttack3Started;
        gameInput.OnAttack1Ended += GameInput_OnAttack1Ended;
        gameInput.OnAttack2Ended += GameInput_OnAttack2Ended;
        gameInput.OnAttack3Ended += GameInput_OnAttack3Ended;
    }

    private void GameInput_OnAttack1Started(object sender, EventArgs e)
    {
        if (isPlayerGameOver) return;
        isBtnAttack1Clicked = true;
        spellController.StartCastingSpellOnClient(0);
    }

    private void GameInput_OnAttack2Started(object sender, EventArgs e)
    {
        if (isPlayerGameOver) return;
        isBtnAttack2Clicked = true;
        spellController.StartCastingSpellOnClient(1);
    }

    private void GameInput_OnAttack3Started(object sender, EventArgs e)
    {
        if (isPlayerGameOver) return;
        isBtnAttack3Clicked = true;
        spellController.StartCastingSpellOnClient(2);
    }

    private void GameInput_OnAttack1Ended(object sender, EventArgs e)
    {
        if (isPlayerGameOver) return;
        if (!isBtnAttack1Clicked) return;
        isBtnAttack1Clicked = false;
        spellController.ShootCurrentCastingSpellOnClient(0);
    }

    private void GameInput_OnAttack2Ended(object sender, EventArgs e)
    {
        if (isPlayerGameOver) return;
        if (!isBtnAttack2Clicked) return;
        isBtnAttack2Clicked = false;
        spellController.ShootCurrentCastingSpellOnClient(1);
    }

    private void GameInput_OnAttack3Ended(object sender, EventArgs e)
    {
        if (isPlayerGameOver) return;
        if (!isBtnAttack3Clicked) return;
        isBtnAttack3Clicked = false;
        spellController.ShootCurrentCastingSpellOnClient(2);
    }

    [ClientRpc]
    public void SetHPClientRPC(ulong clientId, sbyte hp)
    {
        Debug.Log($"Player{clientId}.SetHPClientPRC() HP Set! hp:{hp}");
        hPBar.SetHP(hp);
    }

    /// <summary>
    /// 게임오버 처리 메소드
    /// </summary>
    [ClientRpc]
    public void SetPlayerGameOverClientRPC()
    {
        isPlayerGameOver = true;
        // 이동속도 0
        HandleMovementServerRPC(Vector2.zero, isBtnAttack1Clicked, isBtnAttack2Clicked, isBtnAttack3Clicked);

        PopupGameOverUIOnClient();
    }
    /// <summary>
    /// 클라이언트에서 동작하는 메소드.
    /// </summary>
    public void PopupGameOverUIOnClient()
    {
        Debug.Log("Player PopupGameOverUIOnClient");
        //GameUI.Instance.popupGameOverUI.Show();
        // 일단은 팝업 없음. 테스트중. 
    }

    #region Public 플레이어 정보 확인
    public int GetScore()
    {
        return score;
    }
    #endregion

    #region Public 아이템 구매
    /// <summary>
    /// 아이템 구매 메서드
    /// </summary>
    public void BoughtSpellScroll(Item.ItemType itemType, int slotNumber)
    {
        Debug.Log("Bought spell: " + itemType + ", slotNum : " + slotNumber);
        switch (itemType)
        {
            case Item.ItemType.FireBall_1:
                // CurrentSpell 바꾸는건 현재 착용 장비에 따라 달라지고, 아이템 구매(획득)시에는 해당 스킬 레벨이 올라간다
                //spellController.SetCurrentSpell(gameAssets.fireBall_1, slotNumber);
                break;
            case Item.ItemType.WaterBall_1:
                //spellController.SetCurrentSpell(gameAssets.waterBall_1, slotNumber);
                break;
            case Item.ItemType.IceBall_1:
                //spellController.SetCurrentSpell(gameAssets.iceBall_1, slotNumber);
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

    #region Private 아이템 장착
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

    #region Private&Protect 캐릭터 조작
    // Server Auth 방식의 이동 처리 (현 오브젝트에 Network Transform이 필요)
    protected void HandleMovementServerAuth()
    {
        if (isPlayerGameOver) return;

        Vector2 inputVector = gameInput.GetMovementVectorNormalized();
        HandleMovementServerRPC(inputVector, isBtnAttack1Clicked, isBtnAttack2Clicked, isBtnAttack3Clicked);
    }
    [ServerRpc]
    private void HandleMovementServerRPC(Vector2 inputVector, bool isBtnAttack1Clicked, bool isBtnAttack2Clicked, bool isBtnAttack3Clicked, ServerRpcParams serverRpcParams = default)
    {
        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);

        // 서버권한방식의 네트워크에서 이동처리 할 때 서버의 DeltaTime이 클라이언트의 델타타임과는 다른 경우가 생김. 따라서 아래처럼 수정해야함
        //float moveDistance = moveSpeed * Time.deltaTime;
        float moveDistance = moveSpeed * NetworkManager.Singleton.ServerTime.FixedDeltaTime;
        transform.position += moveDir * moveDistance;

        // 서버(GameMultiplayer)에 새로운 Player Anim State 저장
        if (moveDir != Vector3.zero) {
            GameMultiplayer.Instance.UpdatePlayerAnimStateOnServer(serverRpcParams.Receive.SenderClientId, PlayerMoveAnimState.Walking);
        }
        else
        {
            GameMultiplayer.Instance.UpdatePlayerAnimStateOnServer(serverRpcParams.Receive.SenderClientId, PlayerMoveAnimState.Idle);
        }

        //Debug.Log($"isBtnAttack1Clicked:{isBtnAttack1Clicked}, isBtnAttack2Clicked:{isBtnAttack2Clicked}, isBtnAttack3Clicked:{isBtnAttack3Clicked}");
        // 공격중이 아닐 때에만 진행방향으로 캐릭터 회전
        if (!isBtnAttack1Clicked && !isBtnAttack2Clicked && !isBtnAttack3Clicked)
        {
            Rotate(moveDir);
        }
    }

    /// <summary>
    /// GamePad UI 스킬버튼 드래그에서 호출하여 플레이어를 회전시키는 메소드.
    /// </summary>
    public void RotateByDragSpellBtn(Vector3 dir)
    {
        //Debug.Log($"RotateByDragSpellBtn dir:{dir}");
        RotateByDragSpellBtnServerRPC(dir);
    }
    [ServerRpc (RequireOwnership = false)]
    private void RotateByDragSpellBtnServerRPC(Vector3 dir)
    {
        Rotate(dir);
    }

    private void Rotate(Vector3 moveDir)
    {
        if (moveDir == Vector3.zero) return;

        float rotateSpeed = 30f;
        Vector3 slerpResult = Vector3.Slerp(transform.forward, moveDir, Time.deltaTime * rotateSpeed);
        transform.forward = slerpResult;
    }
    #endregion
}
