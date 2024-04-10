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
    public static Player Instance { get; private set; }

    public event EventHandler OnPlayerGameOver;
    public event EventHandler OnPlayerWin;


    // PlayerData������ ���� ����ϴ°����� �ڵ� ���� �ؾ���. HP���� ������ ���� ���� ������ ����.

    [SerializeField] protected GameInput gameInput;
    [SerializeField] protected GameObject virtualCameraObj;
    [SerializeField] protected DamageTextUIController damageTextUIController;
    [SerializeField] protected HPBarUIController hPBarUIController;
    [SerializeField] protected UserNameUIController userNameUIController;
    [SerializeField] protected SpellController spellController;
    [SerializeField] protected PlayerSpawnPointsController spawnPointsController;

    [SerializeField] protected sbyte hp;
    [SerializeField] protected int score = 0;

    protected GameAssets gameAssets;

    protected bool isAttackButtonClicked;

    // �÷��̾ ������ ��� ��Ȳ. Ŭ���̾�Ʈ ���� ����. ������ ��������� ����ȭ �����ش�.
    [SerializeField] private Dictionary<ItemName, ushort> playerItemDictionaryOnClient;

    private void Awake()
    {
        Debug.Log("Player Awake()");
    }

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

        Instance = this;

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
        spellController.ActivateDefenceSpellOnClient();
    }

    private void GameInput_OnAttack1Started(object sender, EventArgs e)
    {
        isAttackButtonClicked = true;
        spellController.StartCastingSpellOnClient(0);
    }

    private void GameInput_OnAttack2Started(object sender, EventArgs e)
    {
        isAttackButtonClicked = true;
        spellController.StartCastingSpellOnClient(1);
    }

    private void GameInput_OnAttack3Started(object sender, EventArgs e)
    {
        isAttackButtonClicked = true;
        spellController.StartCastingSpellOnClient(2);
    }

    private void GameInput_OnAttack1Ended(object sender, EventArgs e)
    {
        if (!isAttackButtonClicked) return;
        isAttackButtonClicked = false;
        spellController.ShootCurrentCastingSpellOnClient(0);
    }

    private void GameInput_OnAttack2Ended(object sender, EventArgs e)
    {
        if (!isAttackButtonClicked) return;
        isAttackButtonClicked = false;
        spellController.ShootCurrentCastingSpellOnClient(1);
    }

    private void GameInput_OnAttack3Ended(object sender, EventArgs e)
    {
        if (!isAttackButtonClicked) return;
        isAttackButtonClicked = false;
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

        OnPlayerGameOver.Invoke(this, EventArgs.Empty);
    }

    [ClientRpc]
    public void SetPlayerGameWinClientRPC()
    {
        // �� �¸� ĳ������ �����ڰ� �ƴϸ� ����.
        if (!IsOwner) return;

        OnPlayerWin.Invoke(this, EventArgs.Empty);
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
        SoundManager.Instance.PlayOpenScrollSound();
    }

    [ClientRpc]
    public void ShowItemAcquiredUIClientRPC()
    {
        if (!IsOwner) return;
        // �˸� UI ����
        GameSceneUIController.Instance.itemAcquireUIController.ShowItemAcquireUI();
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
        GameSceneUIController.Instance.popupSelectScrollEffectUIController.InitPopup(scrollNames);
    }

    // ���� ���ý� ����. Ŭ���̾�Ʈ���� ���ư��� �޼ҵ� �Դϴ�.
    public void RequestApplyScrollEffectToServer(ItemName scrollName, byte spellIndex)
    {
        //Debug.Log($"RequestApplyScrollEffectToServer. scrollNames:{scrollName}, spellIndexToApply:{spellIndex}");

        // ���޹��� ��ũ�� �̸��� �����ε����� ����ؼ� ȿ�� ������ �����Ѵ�.
        SpellManager.Instance.UpdateScrollEffectServerRPC(scrollName, spellIndex);

        // SFX ���
        SoundManager.Instance.PlayItemSFX(scrollName);
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
        GameObject vfxHeal = Instantiate(GameAssets.instantiate.vfx_SpellUpgrade, transform);
        vfxHeal.GetComponent<NetworkObject>().Spawn();
        vfxHeal.transform.SetParent(transform);
        vfxHeal.transform.localPosition = new Vector3(0f, 0.1f, 0f);
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

    public bool GetIsAttackButtonClicked()
    {
        return isAttackButtonClicked;
    }
}
