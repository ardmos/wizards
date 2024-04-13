using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;


// Player Ŭ���� ���� �и��ؾ���. 
// PlayerClient�� PlayerServer�� �ڵ� �з���. ���� �з��ϰ� �������̽� ������� �ϱ�. Knight����.
// 1. WIzard, Knight�� �˸°� abstract �޼��� ���� �����ϱ�
// 2. GetScore, GetSpellController �� �ʿ伺 Ȯ�� �� ��ġor�����ϱ�
// 3. 

/// <summary>
/// 1. �ΰ��ӿ��� ���Ǵ� �÷��̾� ��ũ��Ʈ�Դϴ�.
/// </summary>
public class Player : NetworkBehaviour
{
/*    public static Player Instance { get; private set; }
*/
/*    public event EventHandler OnPlayerGameOver;
    public event EventHandler OnPlayerWin;*/


    // PlayerData������ ���� ����ϴ°����� �ڵ� ���� �ؾ���. HP���� ������ ���� ���� ������ ����.
    /*
        [SerializeField] protected GameInput gameInput;
        [SerializeField] protected GameObject virtualCameraObj;
        [SerializeField] protected DamageTextUIController damageTextUIController;
        [SerializeField] protected HPBarUIController hPBarUIController;
        [SerializeField] protected UserNameUIController userNameUIController;*/
/*    [SerializeField] protected SpellController spellController;
    [SerializeField] protected PlayerSpawnPointsController spawnPointsController;

    [SerializeField] protected sbyte hp;*/
    //[SerializeField] protected int score = 0;
/*
    protected GameAssets gameAssets;*/

/*    // �÷��̾ ������ ��� ��Ȳ. Ŭ���̾�Ʈ ���� ����. ������ ��������� ����ȭ �����ش�.
    [SerializeField] private Dictionary<ItemName, ushort> playerItemDictionaryOnClient;*/

/*    private void Update()
    {
        if (!IsOwner) return;

        GetComponent<PlayerMovementClient>().HandleMovementServerAuth();
    }*/

/*    /// <summary>
    /// ������ InitializePlayer
    /// 1. ������ġ �ʱ�ȭ
    /// 2. HP �ʱ�ȭ & ��ε�ĳ����
    /// 3. Ư�� �÷��̾ ������ ��ų ��� ���� & �ش��÷��̾�� ����
    /// </summary>
    /// <param name="ownedSpellList"></param>
    public void InitializePlayerOnServer(SpellName[] ownedSpellList, ulong requestedInitializeClientId)
    {
        Debug.Log($"OwnerClientId{OwnerClientId} Player InitializePlayerOnServer");

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
    }*/

/*    [ClientRpc]
    private void InitializePlayerClientRPC(SpellName[] ownedSpellNameList)
    {
        Debug.Log($"OwnerClientId {OwnerClientId} Player InitializePlayerClientRPC");

        // ī�޶� ��ġ �ʱ�ȭ. �����ڸ� ����ٴϵ��� �� 
        virtualCameraObj.SetActive(IsOwner);
        // �ڲ� isKinematic�� ������ �߰��� �ڵ�. Rigidbody network���� ��� �Ѵ� �� ����.
        GetComponent<Rigidbody>().isKinematic = false;
        // �÷��̾� �г��� ����
        PlayerInGameData playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(OwnerClientId);
        userNameUIController.Setup(playerData.playerName.ToString(), IsOwner);
        //Debug.Log($"player Name :{playerData.playerName.ToString()}");

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
        gameInput.OnDefenceStarted += GameInput_OnDefenceStarted;
        gameInput.OnDefenceEnded += GameInput_OnDefenceEnded;
    }*/

   /* private void GameInput_OnDefenceEnded(object sender, EventArgs e)
    {
        throw new NotImplementedException();
    }

    private void GameInput_OnDefenceStarted(object sender, EventArgs e)
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
    }*/

/*    [ClientRpc]
    public void SetHPClientRPC(sbyte hp, sbyte maxHP)
    {
        hPBarUIController.SetHP(hp, maxHP);
    }

    [ClientRpc]
    public void ShowDamagePopupClientRPC(byte damageAmount)
    {
        damageTextUIController.CreateTextObject(damageAmount);
    }*/

/*    /// <summary>
    /// ���� ������ ���۽�Ű�� �޼ҵ�.
    /// 1. �÷��̾� ���� �Ұ�
    /// 2. ���ӿ��� �˾� ����
    /// 3. �˾� BGM ���
    /// </summary>
    [ClientRpc]
    public void SetPlayerGameOverClientRPC()
    {
        //Debug.Log($"I'm player {OwnerClientId}");
        // �� ���ӿ��� ĳ������ �����ڰ� �ƴϸ� ����. RPC�� ������ üũ �� �� ����� ��. 
        if(!IsOwner) return;

        OnPlayerGameOver.Invoke(this, EventArgs.Empty);
        // Popup �����ֱ�
        GameSceneUIManager.Instance.popupGameOverUIController.Show();
        // BGM ���
        SoundManager.Instance.PlayLosePopupSound();
    }

    [ClientRpc]
    public void SetPlayerGameWinClientRPC()
    {
        // �� �¸� ĳ������ �����ڰ� �ƴϸ� ����.
        if (!IsOwner) return;

        OnPlayerWin.Invoke(this, EventArgs.Empty);
        // Popup �����ֱ�
        GameSceneUIManager.Instance.popupWinUIController.Show();
        // BGM ���
        SoundManager.Instance.PlayWinPopupSound();
    }*/

/*    #region Public �÷��̾� ���� Ȯ��
    public int GetScore()
    {
        return score;
    }
    #endregion
*/

    #region ��ų ��ũ�� ȹ��� ���۵�
/*    [ClientRpc]
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
        GameSceneUIManager.Instance.itemAcquireUIController.ShowItemAcquireUI();
    }*/
/*
    /// <summary>
    /// �������� �������� ��ũ�� ȿ�� ����� PopupSelectScrollEffectUIController�� ����.
    /// </summary>
    /// <param name="scrollNames"></param>
    [ClientRpc]
    public void SetScrollEffectsToPopupUIClientRPC(ItemName[] scrollNames)
    {
        if (!IsOwner) return;   
        GameSceneUIManager.Instance.popupSelectScrollEffectUIController.InitPopup(scrollNames);
    }*/
    #endregion

/*    public void SetPlayerItemsDictionaryOnClient(ItemName[] itemNameArray, ushort[] itemCountArray)
    {
        Dictionary<ItemName, ushort> playerItemDictionary = Enumerable.Range(0, itemNameArray.Length).ToDictionary(i => itemNameArray[i], i => itemCountArray[i]);
        Debug.Log($"SetPlayerItemsDictionaryOnClient. player{OwnerClientId}'s playerItemDictionary.Count: {playerItemDictionary.Count} ");
        foreach (var item in playerItemDictionary)
        {
            Debug.Log($"{item.Key}, {item.Value}");
        }

        playerItemDictionaryOnClient = playerItemDictionary;
    }*/
}
