using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
/// <summary>
/// 1. �ΰ��ӿ��� ���Ǵ� �÷��̾� ��ũ��Ʈ�Դϴ�.
/// </summary>
public class Player : NetworkBehaviour
{
    public static Player LocalInstance { get; private set; }

    // PlayerData������ ���� ����ϴ°����� �ڵ� ���� �ؾ���. HP���� ������ ���� ���� ������ ����.

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

    // �÷��̾� ���� ��� ���� ����
    [SerializeField] private bool isPlayerGameOver;

    // �÷��̾ ������ ��� ��Ȳ. Ŭ���̾�Ʈ ���� ����. ������ ��������� ����ȭ �����ش�.
    [SerializeField] private Dictionary<ItemName, ushort> playerItemDictionaryOnClient;

    /// <summary>
    /// ������ InitializePlayer
    /// 1. ������ġ �ʱ�ȭ
    /// 2. HP �ʱ�ȭ & ��ε�ĳ����
    /// 3. Ư�� �÷��̾ ������ ��ų ��� ���� & �ش��÷��̾�� ����
    /// </summary>
    /// <param name="ownedSpellList"></param>
    public void InitializePlayerOnServer(SpellName[] ownedSpellList, ulong requestedInitializeClientId)
    {
        gameAssets = GameAssets.instantiate;
        spawnPointsController = FindObjectOfType<PlayerSpawnPointsController>();

        if (gameAssets == null)
        {
            Debug.Log($"{nameof(InitializePlayerOnServer)}, GameAssets�� ã�� ���߽��ϴ�.");
            return;
        }

        if (spawnPointsController == null)
        {
            Debug.LogError($"{nameof(InitializePlayerOnServer)}, ������ġ�� Ư������ ���߽��ϴ�.");
            return;
        }

        // ���� ��ġ �ʱ�ȭ   
        transform.position = spawnPointsController.GetSpawnPoint(GameMultiplayer.Instance.GetPlayerDataIndexFromClientId(OwnerClientId));

        // HP �ʱ�ȭ
        PlayerInGameData playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(requestedInitializeClientId);  
        playerData.playerHP = hp;
        playerData.playerMaxHP = hp;    
        GameMultiplayer.Instance.SetPlayerDataFromClientId(requestedInitializeClientId, playerData);

        // ���� HP ���� �� ����
        PlayerHPManager.Instance.UpdatePlayerHP(requestedInitializeClientId, playerData.playerHP, playerData.playerMaxHP);

        // Ư�� �÷��̾ ������ ��ų ��� ����
        SpellManager.Instance.InitPlayerSpellInfoArrayOnServer(requestedInitializeClientId, ownedSpellList);

        // Spawn�� Ŭ���̾�Ʈ�� InitializePlayer ����
        InitializePlayerClientRPC(ownedSpellList);
    }

    [ClientRpc]
    private void InitializePlayerClientRPC(SpellName[] ownedSpellNameList)
    {
        // ī�޶� ��ġ �ʱ�ȭ. �����ڸ� ����ٴϵ��� �� 
        virtualCameraObj.SetActive(IsOwner);
        // �ڲ� isKinematic�� ������ �߰��� �ڵ�. Rigidbody network���� ��� �Ѵ� �� ����.
        GetComponent<Rigidbody>().isKinematic = false;
        // �÷��̾� �г��� ����
        PlayerInGameData playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(OwnerClientId);
        userNameUIController.Setup(playerData.playerName.ToString(), IsOwner);
        Debug.Log($"player Name :{playerData.playerName.ToString()}");

        if (!IsOwner) return;
        LocalInstance = this;

        // ���� ������ GamePad UI�� �ݿ�          
        //Debug.Log($"ownedSpellNameList.Length : {ownedSpellNameList.Length}");
        FindObjectOfType<GamePadUIController>().UpdateSpellUI(ownedSpellNameList);
        
        // Input Action �̺�Ʈ ����
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
    /// ���� ������ ���۽�Ű�� �޼ҵ�.
    /// 1. �÷��̾� ���� �Ұ�
    /// 2. ���ӿ��� �˾� ����
    /// </summary>
    [ClientRpc]
    public void SetPlayerGameOverClientRPC()
    {
        //Debug.Log($"I'm player {OwnerClientId}");
        // �� ���ӿ��� ĳ������ �����ڰ� �ƴϸ� ����. RPC�� ������ üũ �� �� ����� ��. 
        if(!IsOwner) return;

        // �÷��̾ ĳ���� ���� �Ұ�
        isPlayerGameOver = true;
        // �̵��ӵ� 0
        HandleMovementServerRPC(Vector2.zero, isBtnAttack1Clicked, isBtnAttack2Clicked, isBtnAttack3Clicked);
        // ���ӿ��� �˾� ����ֱ�
        GameUIController.instance.popupGameOverUIController.Show();
    }

    [ClientRpc]
    public void SetPlayerGameWinClientRPC()
    {
        // �� �¸� ĳ������ �����ڰ� �ƴϸ� ����.
        if (!IsOwner) return;

        // �¸� �˾� ����ֱ�
        GameUIController.instance.popupWinUIController.Show();
    }

    #region Public �÷��̾� ���� Ȯ��
    public int GetScore()
    {
        return score;
    }
    #endregion

    public SpellController GetSpellController()
    {
        return spellController;
    }

    #region ��ų ��ũ�� ȹ��� ���۵�
    [ClientRpc]
    public void UpdateScrollQueueClientRPC(byte[] scrollSpellSlotArray)
    {
        if (!IsOwner) return;

        Queue<byte> scrollSpellSlotQueue = new Queue<byte>(scrollSpellSlotArray);

        // Queue ������Ʈ
        SpellManager.Instance.UpdatePlayerScrollSpellSlotQueueOnClient(scrollSpellSlotQueue);

        // SFX ����
        SoundManager.instance.PlayOpenScrollSound();
    }

    [ClientRpc]
    public void ShowItemAcquiredUIClientRPC()
    {
        if (!IsOwner) return;
        // �˸� UI ����
        GameUIController.instance.itemAcquireUIController.ShowItemAcquireUI();
    }

    public void RequestUniqueRandomScrollsToServer()
    {
        SpellManager.Instance.GetUniqueRandomScrollsServerRPC();
    }

    /// <summary>
    /// �������� �������� ��ũ�� ȿ�� ����� PopupSelectScrollEffectUIController�� ����.
    /// </summary>
    /// <param name="scrollNames"></param>
    [ClientRpc]
    public void SetScrollEffectsToPopupUIClientRPC(ItemName[] scrollNames)
    {
        if (!IsOwner) return;   
        GameUIController.instance.popupSelectScrollEffectUIController.InitPopup(scrollNames);
    }

    // ���� ���ý� ����. Ŭ���̾�Ʈ���� ���ư��� �޼ҵ� �Դϴ�.
    public void RequestApplyScrollEffectToServer(ItemName scrollName, byte spellIndex)
    {
        //Debug.Log($"RequestApplyScrollEffectToServer. scrollNames:{scrollName}, spellIndexToApply:{spellIndex}");

        // ���޹��� ��ũ�� �̸��� �����ε����� ����ؼ� ȿ�� ������ �����Ѵ�.
        SpellManager.Instance.UpdateScrollEffectServerRPC(scrollName, spellIndex);

        // SFX ���
        SoundManager.instance.PlayItemSFX(scrollName);
        // VFX ���
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
        // ��ũ�� Ȱ��. ��ų ��ȭ VFX ����
        GameObject vfxHeal = Instantiate(GameAssets.instantiate.vfxSpellUpgrade, transform);
        vfxHeal.GetComponent<NetworkObject>().Spawn();
        vfxHeal.transform.SetParent(transform);
        vfxHeal.transform.localPosition = new Vector3(0f, 0.1f, 0f);
    }
    #endregion

    #region Private&Protect ĳ���� ����
    // Server Auth ����� �̵� ó�� (�� ������Ʈ�� Network Transform�� �ʿ�)
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

        // �������ѹ���� ��Ʈ��ũ���� �̵�ó�� �� �� ������ DeltaTime�� Ŭ���̾�Ʈ�� ��ŸŸ�Ӱ��� �ٸ� ��찡 ����. ���� �Ʒ�ó�� �����ؾ���
        //float moveDistance = moveSpeed * Time.deltaTime;
        float moveDistance = moveSpeed * NetworkManager.Singleton.ServerTime.FixedDeltaTime;
        transform.position += moveDir * moveDistance;

        // ����(GameMultiplayer)�� ���ο� Player Anim State ����. (GameOver���°� �ƴ� ������!)
        if (GameMultiplayer.Instance.GetPlayerDataFromClientId(serverRpcParams.Receive.SenderClientId).playerMoveAnimState == PlayerMoveAnimState.GameOver)
            return;

        if (moveDir != Vector3.zero) {
            GameMultiplayer.Instance.UpdatePlayerMoveAnimStateOnServer(serverRpcParams.Receive.SenderClientId, PlayerMoveAnimState.Walking);
        }
        else
        {
            GameMultiplayer.Instance.UpdatePlayerMoveAnimStateOnServer(serverRpcParams.Receive.SenderClientId, PlayerMoveAnimState.Idle);
        }

        // �������� �ƴ� ������ ����������� ĳ���� ȸ��
        if (!isBtnAttack1Clicked && !isBtnAttack2Clicked && !isBtnAttack3Clicked)
        {
            Rotate(moveDir);
        }
    }

    /// <summary>
    /// GamePad UI ��ų��ư �巡�׿��� ȣ���Ͽ� �÷��̾ ȸ����Ű�� �޼ҵ�.
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
