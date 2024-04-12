using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SocialPlatforms.Impl;

public abstract class PlayerClient : NetworkBehaviour
{
    public static PlayerClient Instance {  get; private set; }
    public event EventHandler OnPlayerGameOver;
    //public event EventHandler OnPlayerWin;

    private GameInput gameInput;

    // �÷��̾ ������ ��� ��Ȳ. Ŭ���̾�Ʈ ���� ����. ������ ��������� ����ȭ �����ش�.
    [SerializeField] private Dictionary<ItemName, ushort> playerItemDictionaryOnClient;

    private void Awake()
    {
        gameInput = GetComponent<GameInput>();
    }

    [ClientRpc]
    public void InitializePlayerClientRPC(ICharacter character)
    {
        Debug.Log($"OwnerClientId {OwnerClientId} Player InitializePlayerClientRPC");

        // ī�޶� ��ġ �ʱ�ȭ. �����ڸ� ����ٴϵ��� �� 
        GetComponentInChildren<CinemachineVirtualCamera>()?.gameObject.SetActive(IsOwner);
        // �ڲ� isKinematic�� ������ �߰��� �ڵ�. Rigidbody network���� ��� �Ѵ� �� ����.
        GetComponent<Rigidbody>().isKinematic = false;
        // �÷��̾� �г��� ����
        PlayerInGameData playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(OwnerClientId);
        GetComponentInChildren<UserNameUIController>()?.Setup(playerData.characterData.playerName.ToString(), IsOwner);
        //Debug.Log($"player Name :{playerData.playerName.ToString()}");

        if (!IsOwner) return;

        Instance = this;

        // ���� ������ GamePad UI�� �ݿ�          
        //Debug.Log($"ownedSpellNameList.Length : {ownedSpellNameList.Length}");
        FindObjectOfType<GamePadUIController>().UpdateSpellUI(character.skills);

        // Input Action �̺�Ʈ ����
        gameInput.OnAttack1Started += GameInput_OnAttack1Started;
        gameInput.OnAttack2Started += GameInput_OnAttack2Started;
        gameInput.OnAttack3Started += GameInput_OnAttack3Started;
        gameInput.OnAttack1Ended += GameInput_OnAttack1Ended;
        gameInput.OnAttack2Ended += GameInput_OnAttack2Ended;
        gameInput.OnAttack3Ended += GameInput_OnAttack3Ended;
        gameInput.OnDefenceStarted += GameInput_OnDefenceStarted;
        gameInput.OnDefenceEnded += GameInput_OnDefenceEnded;
    }

    private void OnDisable()
    {
        // ���� ����
        gameInput.OnAttack1Started -= GameInput_OnAttack1Started;
        gameInput.OnAttack2Started -= GameInput_OnAttack2Started;
        gameInput.OnAttack3Started -= GameInput_OnAttack3Started;
        gameInput.OnAttack1Ended -= GameInput_OnAttack1Ended;
        gameInput.OnAttack2Ended -= GameInput_OnAttack2Ended;
        gameInput.OnAttack3Ended -= GameInput_OnAttack3Ended;
        gameInput.OnDefenceStarted -= GameInput_OnDefenceStarted;
        gameInput.OnDefenceEnded -= GameInput_OnDefenceEnded;
    }

    protected abstract void GameInput_OnAttack1Started(object sender, EventArgs e);
    protected abstract void GameInput_OnAttack2Started(object sender, EventArgs e);
    protected abstract void GameInput_OnAttack3Started(object sender, EventArgs e);
    protected abstract void GameInput_OnDefenceStarted(object sender, EventArgs e);
    protected abstract void GameInput_OnAttack1Ended(object sender, EventArgs e);
    protected abstract void GameInput_OnAttack2Ended(object sender, EventArgs e);
    protected abstract void GameInput_OnAttack3Ended(object sender, EventArgs e);
    protected abstract void GameInput_OnDefenceEnded(object sender, EventArgs e);

    [ClientRpc]
    public void SetHPClientRPC(sbyte hp, sbyte maxHP)
    {
        GetComponentInChildren<HPBarUIController>()?.SetHP(hp, maxHP);
    }

    [ClientRpc]
    public void ShowDamagePopupClientRPC(byte damageAmount)
    {
        GetComponentInChildren<DamageTextUIController>()?.CreateTextObject(damageAmount);
    }

    /// <summary>
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
        if (!IsOwner) return;

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

        //OnPlayerWin.Invoke(this, EventArgs.Empty);
        // Popup �����ֱ�
        GameSceneUIManager.Instance.popupWinUIController.Show();
        // BGM ���
        SoundManager.Instance.PlayWinPopupSound();
    }

    public int GetPlayerScore()
    {
        return GameMultiplayer.Instance.GetPlayerDataFromClientId(OwnerClientId).characterData.score;
    }

    [ClientRpc]
    public void UpdateScrollQueueClientRPC(byte[] scrollSpellSlotArray)
    {
        if (!IsOwner) return;

        Queue<byte> scrollSpellSlotQueue = new Queue<byte>(scrollSpellSlotArray);

        // Queue ������Ʈ
        GetComponent<PlayerSpellScrollQueueControllerClient>().UpdatePlayerScrollSpellSlotQueueOnClient(scrollSpellSlotQueue);

        // SFX ����
        SoundManager.Instance.PlayOpenScrollSound();
    }

    [ClientRpc]
    public void ShowItemAcquiredUIClientRPC()
    {
        if (!IsOwner) return;
        // �˸� UI ����
        GameSceneUIManager.Instance.itemAcquireUIController.ShowItemAcquireUI();
    }

    /// <summary>
    /// �������� �������� ��ũ�� ȿ�� ����� PopupSelectScrollEffectUIController�� ����.
    /// </summary>
    /// <param name="scrollNames"></param>
    [ClientRpc]
    public void SetScrollEffectsToPopupUIClientRPC(ItemName[] scrollNames)
    {
        if (!IsOwner) return;
        GameSceneUIManager.Instance.popupSelectScrollEffectUIController.InitPopup(scrollNames);
    }

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
