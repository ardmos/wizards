using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
/// <summary>
/// 1. 인게임에서 사용되는 플레이어 스크립트입니다.
/// </summary>
public class Player : NetworkBehaviour
{
    public static Player LocalInstance { get; private set; }

    // PlayerData변수를 만들어서 사용하는것으로 코드 정리 해야함. HP같은 변수들 따루 있을 이유가 없음.

    [SerializeField] protected GameInput gameInput;
    [SerializeField] protected GameObject virtualCameraObj;
    [SerializeField] protected DamageTextUIController damageTextUIController;
    [SerializeField] protected HPBarUIController hPBarUIController;
    [SerializeField] protected UserNameUIController userNameUIController;
    [SerializeField] protected SpellController spellController;
    [SerializeField] protected PlayerSpawnPointsController spawnPointsController;
    [SerializeField] protected float moveSpeed;
    [SerializeField] protected sbyte hp;
    [SerializeField] protected int score = 0;

    protected GameAssets gameAssets;

    //protected bool isWalking;
    protected bool isBtnAttack1Clicked;
    protected bool isBtnAttack2Clicked;
    protected bool isBtnAttack3Clicked;

    // 플레이어 조작 제어를 위한 변수
    [SerializeField] private bool isPlayerGameOver;

    // 플레이어가 보유한 장비 현황. 클라이언트 저장 버전. 서버측 저장버전과 동기화 시켜준다.
    [SerializeField] private Dictionary<ItemName, ushort> playerItemDictionaryOnClient;

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
        spawnPointsController = FindObjectOfType<PlayerSpawnPointsController>();

        if (gameAssets == null)
        {
            Debug.Log($"{nameof(InitializePlayerOnServer)}, GameAssets를 찾지 못했습니다.");
            return;
        }

        if (spawnPointsController == null)
        {
            Debug.LogError($"{nameof(InitializePlayerOnServer)}, 스폰위치를 특정하지 못했습니다.");
            return;
        }

        // 스폰 위치 초기화   
        transform.position = spawnPointsController.GetSpawnPoint(GameMultiplayer.Instance.GetPlayerDataIndexFromClientId(OwnerClientId));

        // HP 초기화
        PlayerInGameData playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(requestedInitializeClientId);  
        playerData.playerHP = hp;
        playerData.playerMaxHP = hp;    
        GameMultiplayer.Instance.SetPlayerDataFromClientId(requestedInitializeClientId, playerData);

        // 현재 HP 저장 및 설정
        PlayerHPManager.Instance.UpdatePlayerHP(requestedInitializeClientId, playerData.playerHP, playerData.playerMaxHP);

        // 특정 플레이어가 보유한 스킬 목록 저장
        SpellManager.Instance.InitPlayerSpellInfoArrayOnServer(requestedInitializeClientId, ownedSpellList);

        // Spawn된 클라이언트측 InitializePlayer 시작
        InitializePlayerClientRPC(ownedSpellList);
    }

    [ClientRpc]
    private void InitializePlayerClientRPC(SpellName[] ownedSpellNameList)
    {
        // 카메라 위치 초기화. 소유자만 따라다니도록 함 
        virtualCameraObj.SetActive(IsOwner);
        // 자꾸 isKinematic이 켜져서 추가한 코드. Rigidbody network에서 계속 켜는 것 같다.
        GetComponent<Rigidbody>().isKinematic = false;
        // 플레이어 닉네임 설정
        PlayerInGameData playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(OwnerClientId);
        userNameUIController.Setup(playerData.playerName.ToString(), IsOwner);
        Debug.Log($"player Name :{playerData.playerName.ToString()}");

        if (!IsOwner) return;
        LocalInstance = this;

        // 보유 스펠을 GamePad UI에 반영          
        //Debug.Log($"ownedSpellNameList.Length : {ownedSpellNameList.Length}");
        FindObjectOfType<GamePadUIController>().UpdateSpellUI(ownedSpellNameList);
        
        // Input Action 이벤트 구독
        gameInput.OnAttack1Started += GameInput_OnAttack1Started;
        gameInput.OnAttack2Started += GameInput_OnAttack2Started;
        gameInput.OnAttack3Started += GameInput_OnAttack3Started;
        gameInput.OnAttack1Ended += GameInput_OnAttack1Ended;
        gameInput.OnAttack2Ended += GameInput_OnAttack2Ended;
        gameInput.OnAttack3Ended += GameInput_OnAttack3Ended;
        gameInput.OnDefenceClicked += GameInput_OnDefenceClicked;
    }

    private void GameInput_OnDefenceClicked(object sender, EventArgs e)
    {
        if (isPlayerGameOver) return;
        spellController.ActivateDefenceSpellOnClient();
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
    public void SetHPClientRPC(sbyte hp, sbyte maxHP)
    {
        hPBarUIController.SetHP(hp, maxHP);
    }

    [ClientRpc]
    public void ShowDamagePopupClientRPC(byte damageAmount)
    {
        damageTextUIController.CreateTextObject(damageAmount);
    }

    /// <summary>
    /// 게임 오버시 동작시키는 메소드.
    /// 1. 플레이어 조작 불가
    /// 2. 게임오버 팝업 띄우기
    /// </summary>
    [ClientRpc]
    public void SetPlayerGameOverClientRPC()
    {
        //Debug.Log($"I'm player {OwnerClientId}");
        // 이 게임오버 캐릭터의 소유자가 아니면 리턴. RPC라 소유자 체크 한 번 해줘야 함. 
        if(!IsOwner) return;

        // 플레이어가 캐릭터 조작 불가
        isPlayerGameOver = true;
        // 이동속도 0
        HandleMovementServerRPC(Vector2.zero, isBtnAttack1Clicked, isBtnAttack2Clicked, isBtnAttack3Clicked);
        // 게임오버 팝업 띄워주기
        GameUIController.instance.popupGameOverUIController.Show();
    }

    [ClientRpc]
    public void SetPlayerGameWinClientRPC()
    {
        // 이 승리 캐릭터의 소유자가 아니면 리턴.
        if (!IsOwner) return;

        // 승리 팝업 띄워주기
        GameUIController.instance.popupWinUIController.Show();
    }

    #region Public 플레이어 정보 확인
    public int GetScore()
    {
        return score;
    }
    #endregion

    public SpellController GetSpellController()
    {
        return spellController;
    }

    #region 스킬 스크롤 획득시 동작들
    [ClientRpc]
    public void UpdateScrollQueueClientRPC(byte[] scrollSpellSlotArray)
    {
        if (!IsOwner) return;

        Queue<byte> scrollSpellSlotQueue = new Queue<byte>(scrollSpellSlotArray);

        // Queue 업데이트
        SpellManager.Instance.UpdatePlayerScrollSpellSlotQueueOnClient(scrollSpellSlotQueue);

        // SFX 실행
        SoundManager.instance.PlayOpenScrollSound();
    }

    [ClientRpc]
    public void ShowItemAcquiredUIClientRPC()
    {
        if (!IsOwner) return;
        // 알림 UI 실행
        GameUIController.instance.itemAcquireUIController.ShowItemAcquireUI();
    }

    public void RequestUniqueRandomScrollsToServer()
    {
        SpellManager.Instance.GetUniqueRandomScrollsServerRPC();
    }

    /// <summary>
    /// 서버에서 제공해준 스크롤 효과 목록을 PopupSelectScrollEffectUIController에 적용.
    /// </summary>
    /// <param name="scrollNames"></param>
    [ClientRpc]
    public void SetScrollEffectsToPopupUIClientRPC(ItemName[] scrollNames)
    {
        if (!IsOwner) return;   
        GameUIController.instance.popupSelectScrollEffectUIController.InitPopup(scrollNames);
    }

    // 슬롯 선택시 동작. 클라이언트에서 돌아가는 메소드 입니다.
    public void RequestApplyScrollEffectToServer(ItemName scrollName, byte spellIndex)
    {
        //Debug.Log($"RequestApplyScrollEffectToServer. scrollNames:{scrollName}, spellIndexToApply:{spellIndex}");

        // 전달받은 스크롤 이름과 스펠인덱스를 사용해서 효과 적용을 진행한다.
        SpellManager.Instance.UpdateScrollEffectServerRPC(scrollName, spellIndex);

        // SFX 재생
        SoundManager.instance.PlayItemSFX(scrollName);
        // VFX 재생
        ApplyScrollVFXServerRPC();
    }

    [ClientRpc]
    public void DequeuePlayerScrollSpellSlotQueueClientRPC()
    {
        if (!IsOwner) return;
        SpellManager.Instance.DequeuePlayerScrollSpellSlotQueueOnClient();
    }

    [ServerRpc]
    private void ApplyScrollVFXServerRPC()
    {
        // 스크롤 활용. 스킬 강화 VFX 실행
        GameObject vfxHeal = Instantiate(GameAssets.instantiate.vfxSpellUpgrade, transform);
        vfxHeal.GetComponent<NetworkObject>().Spawn();
        vfxHeal.transform.SetParent(transform);
        vfxHeal.transform.localPosition = new Vector3(0f, 0.1f, 0f);
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

        // 서버(GameMultiplayer)에 새로운 Player Anim State 저장. (GameOver상태가 아닐 때에만!)
        if (GameMultiplayer.Instance.GetPlayerDataFromClientId(serverRpcParams.Receive.SenderClientId).playerMoveAnimState == PlayerMoveAnimState.GameOver)
            return;

        if (moveDir != Vector3.zero) {
            GameMultiplayer.Instance.UpdatePlayerMoveAnimStateOnServer(serverRpcParams.Receive.SenderClientId, PlayerMoveAnimState.Walking);
        }
        else
        {
            GameMultiplayer.Instance.UpdatePlayerMoveAnimStateOnServer(serverRpcParams.Receive.SenderClientId, PlayerMoveAnimState.Idle);
        }

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


    public void SetPlayerItemsDictionaryOnClient(ItemName[] itemNameArray, ushort[] itemCountArray)
    {
        Dictionary<ItemName, ushort> playerItemDictionary = Enumerable.Range(0, itemNameArray.Length).ToDictionary(i => itemNameArray[i], i => itemCountArray[i]);
        Debug.Log($"SetPlayerItemsDictionaryOnClient. player{OwnerClientId}'s playerItemDictionary.Count: {playerItemDictionary.Count} ");
        foreach (var item in playerItemDictionary)
        {
            Debug.Log($"{item.Key}, {item.Value}");
        }

        playerItemDictionaryOnClient = playerItemDictionary;
    }
}
