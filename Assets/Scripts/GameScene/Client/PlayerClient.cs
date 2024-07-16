using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SocialPlatforms.Impl;

public abstract class PlayerClient : NetworkBehaviour
{
    public static PlayerClient Instance { get; private set; }
    public PlayerServer playerServer;
    public event EventHandler OnPlayerGameOver;
    //public event EventHandler OnPlayerWin;

    [Header("ĳ���Ϳ� UI ��Ʈ�ѷ���")]
    public TeamColorController teamColorController;
    public UserNameUIController userNameUIController;
    public HPBarUIController hPBarUIController;
    public DamageTextUIController damageTextUIController;

    public CinemachineVirtualCamera VirtualCamera;
    public Rigidbody mRigidbody;
    public AudioListener audioListener;
    public PlayerSpellScrollQueueManagerClient playerSpellScrollQueueManagerClient;
    public KnightBuzzMeshTrail meshTrail;

    public GameInput gameInput;

    private Material currentMaterial;
    public Material originalMaterial;
    public Material highlightMaterial;
    public Material frozenMaterial;
    public Material enemyMaterial;
    public Renderer[] ownRenderers;

    public CameraShake cameraShake;

    // �÷��̾ ������ ��� ��Ȳ. Ŭ���̾�Ʈ ���� ����. ������ ��������� ����ȭ �����ش�.
    [SerializeField] private Dictionary<ItemName, ushort> playerItemDictionaryOnClient;

    [ClientRpc]
    public virtual void InitializePlayerClientRPC(SkillName[] skills)
    {
        Debug.Log($"OwnerClientId {OwnerClientId} Player InitializePlayerClientRPC");

        // ī�޶� ��ġ �ʱ�ȭ. �����ڸ� ����ٴϵ��� �� 
        VirtualCamera?.gameObject.SetActive(IsOwner);
        // �ڲ� isKinematic�� ������ �߰��� �ڵ�. Rigidbody network���� ��� �Ѵ� �� ����.
        mRigidbody.isKinematic = false;
        // �÷��̾� �г��� ����
        PlayerInGameData playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(OwnerClientId);
        userNameUIController?.SetName(playerData.playerName.ToString());

        // ����������� �ʱ�ȭ
        audioListener.enabled = IsOwner;

        // �⺻ ���͸��� �ʱ�ȭ
        currentMaterial = originalMaterial;

        // �÷��̾� UI �÷� ����
        teamColorController?.Setup(IsOwner);

        if (!IsOwner) {
            // enemy �÷��̾� �׵θ� �÷� ����
            foreach (Renderer renderer in ownRenderers)
            {
                renderer.material = enemyMaterial;
            }        
            // ���� ���ͽý��� �߰��� ���⼭ �����÷� ���� �б� ��������մϴ�. 
            return;
        }
        

        Instance = this;

        // ���� skill ������ GamePad UI�� �ݿ�          
        Debug.Log($"PlayerClient.InitializePlayerClientRPC() skills.Length : {skills.Length}");
        GameSceneUIManager.Instance.gamePadUIController.UpdateSpellUI(skills);

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


    // ���� ī�޶� ����ũ ȿ�� ����
    [ClientRpc]
    public void ActivateHitCameraShakeClientRPC()
    {
        if (!IsOwner) return;
        cameraShake.ShakeCamera(3f, 0.2f);
    }

    // ĳ���� ������ ����Ʈ ����
    [ClientRpc]
    public void ActivateFrozenEffectClientRPC()
    {
        currentMaterial = frozenMaterial;
        foreach (Renderer renderer in ownRenderers)
        {
            renderer.material = frozenMaterial;
        }
    }
    // ĳ���� ������ ����Ʈ ����
    [ClientRpc]
    public void DeactivateFrozenEffectClientRPC()
    {
        currentMaterial = originalMaterial;
        foreach (Renderer renderer in ownRenderers)
        {
            renderer.material = originalMaterial;
        }
    }

    // �ǰ� ĳ���� ��¦ ����Ʈ ����
    [ClientRpc]
    public void ActivateHitByAttackEffectClientRPC()
    {
        foreach(Renderer renderer in ownRenderers)
        {
            renderer.material = highlightMaterial;
        }
        
        StartCoroutine(ResetFlashEffect());
    }
    // �ǰ� ĳ���� ��¦ ȿ���� ���� �ð� �Ŀ� ��Ȱ��ȭ�ϴ� �ڷ�ƾ �޼���
    private IEnumerator ResetFlashEffect()
    {
        yield return new WaitForSeconds(0.1f);

        foreach (Renderer renderer in ownRenderers)
        {
            renderer.material = currentMaterial;
        }
    }

    // �ǰ� ���� ����

    // �ǰ� ī�޶� ����ũ ȿ�� ����
    [ClientRpc]
    public void ActivateHitByAttackCameraShakeClientRPC()
    {
        if (!IsOwner) return;
        cameraShake.ShakeCamera(5f, 0.2f);
    }

    // �ǰ� ī�޶� �׵θ� ȿ�� ����
    [ClientRpc]
    public void ActivateHitByAttackCameraEffectClientRPC()
    {
        // �ǰݴ��� �÷��̾�� ����� ȿ��
        if (!IsOwner) return;

        FindObjectOfType<Volume>().profile.TryGet(out Vignette vignette);
        vignette.intensity.value = 0.25f;
        StartCoroutine(ResetCameraEffect(vignette));
    }
    // �ǰ� �׵θ� ȿ���� ���� �ð� �Ŀ� ��Ȱ��ȭ�ϴ� �ڷ�ƾ �޼���
    private IEnumerator ResetCameraEffect(Vignette vignette)
    {
        yield return new WaitForSeconds(0.1f);
        vignette.intensity.value = 0f;
    }

    [ClientRpc]
    public void ActivateDashTrailEffectClientRPC()
    {
        meshTrail.ActivateTrail();
    }

    [ClientRpc]
    public void SetHPClientRPC(sbyte hp, sbyte maxHP)
    {
        hPBarUIController?.SetHP(hp, maxHP);
    }

    [ClientRpc]
    public void ShowDamageTextPopupClientRPC(sbyte damageAmount)
    {
        damageTextUIController?.CreateTextObject(damageAmount);
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

        // GameInput���� ���� �Ұ��ϵ��� ����
        OnPlayerGameOver.Invoke(this, EventArgs.Empty);
        // Popup �����ֱ�
        GameSceneUIManager.Instance.popupGameOverUIController.Show();

        // ���� ��� ����. ���� ����! ( ��⿡ �������Դϴ�. )
        SaveGameOverResult();

        // BGM ���
        SoundManager.Instance.PlayLosePopupSound();
    }

    private void SaveGameOverResult()
    {
        PlayerOutGameData playerOutGameData = PlayerDataManager.Instance.GetPlayerOutGameData();

        if (playerOutGameData == null) return;

        float score = GameMultiplayer.Instance.GetPlayerScore(OwnerClientId);

        if (playerOutGameData.hightestKOinOneMatch < score)
        {
            playerOutGameData.hightestKOinOneMatch = (byte)score;
        }
        playerOutGameData.knockOuts += (uint)score;
        playerOutGameData.totalScore = playerOutGameData.knockOuts * 2;

        PlayerDataManager.Instance.UpdatePlayerData(playerOutGameData);
    }

    [ClientRpc]
    public void SetPlayerGameWinClientRPC()
    {
        // �� �¸� ĳ������ �����ڰ� �ƴϸ� ����.
        if (!IsOwner) return;

        //OnPlayerWin.Invoke(this, EventArgs.Empty);
        // Popup �����ֱ�
        GameSceneUIManager.Instance.popupWinUIController.Show(OwnerClientId);

        // ���� ��� ����. �¸�! (��⿡ �������Դϴ�)
        SaveWinResult();

        // BGM ���
        SoundManager.Instance.PlayWinPopupSound();
    }

    private void SaveWinResult()
    {
        PlayerOutGameData playerOutGameData = PlayerDataManager.Instance.GetPlayerOutGameData();

        if (playerOutGameData == null) return;

        float score = GameMultiplayer.Instance.GetPlayerScore(OwnerClientId);

        if (playerOutGameData.hightestKOinOneMatch < score)
        {
            playerOutGameData.hightestKOinOneMatch = (byte)score;
        }
        playerOutGameData.mostWins++;
        playerOutGameData.soloVictories++;
        playerOutGameData.knockOuts += (uint)score;
        playerOutGameData.totalScore = playerOutGameData.knockOuts * 2;

        //Debug.Log($"player{OwnerClientId} score : {GameMultiplayer.Instance.GetPlayerScore(OwnerClientId)}, (uint) : {(uint)GameMultiplayer.Instance.GetPlayerScore(OwnerClientId)}");

        PlayerDataManager.Instance.UpdatePlayerData(playerOutGameData);
    }

    public int GetPlayerScore()
    {
        return GameMultiplayer.Instance.GetPlayerDataFromClientId(OwnerClientId).score;
    }

    /// <summary>
    /// ��ũ�� ȹ��� ȣ��Ǵ� �޼���. 
    /// 1. ���� UI ������Ʈ
    /// 2. �߾� �˾� UI ����
    /// </summary>
    /// <param name="scrollSpellSlotArray"></param>
    [ClientRpc]
    public void AddScrollClientRPC(int scrollCount)
    {
        if (!IsOwner) return;

        // 1. ���� UI ������Ʈ
        playerSpellScrollQueueManagerClient.UpdatePlayerScrollSpellSlotUI(scrollCount);

        // SFX ���
        SoundManager.Instance.PlayItemSFXServerRPC(ItemName.ScrollPickup, transform.position);

        // 2. �߾� �˾� UI ����
        GameSceneUIManager.Instance.itemAcquireUIController.ShowScrollAcquiredUI();
    }

    [ClientRpc]
    public void UpdateScrollClientRPC(int scrollCount)
    {
        if (!IsOwner) return;

        // 1. ���� UI ������Ʈ
        playerSpellScrollQueueManagerClient.UpdatePlayerScrollSpellSlotUI(scrollCount);

        // SFX ���
        SoundManager.Instance.PlayItemSFXServerRPC(ItemName.ScrollUse, transform.position);

        // VFX ���
        playerServer.StartApplyScrollVFXServerRPC();
    }

    /// <summary>
    /// �������� �������� ��ũ�� ȿ�� ����� PopupSelectScrollEffectUIController�� ����.
    /// </summary>
    [ClientRpc]
    public void InitSelectScrollEffectsPopupUIClientRPC(SkillUpgradeOptionDTO[] skillUpgradeOptionsDTO)
    {
        if (!IsOwner) return;
        List<ISkillUpgradeOption> skillUpgradeOptions = SkillUpgradeFactory.FromDTOList(skillUpgradeOptionsDTO.ToList());
        GameSceneUIManager.Instance.popupSelectScrollEffectUIController.InitPopup(skillUpgradeOptions);
    }
    /*    [ClientRpc]
        public void InitSelectScrollEffectsPopupUIClientRPC(ItemName[] scrollNames, byte spellIndexToApplyEffect)
        {
            if (!IsOwner) return;
            GameSceneUIManager.Instance.popupSelectScrollEffectUIController.InitPopup(scrollNames, spellIndexToApplyEffect);
        }*/

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

    [ClientRpc]
    public void OffPlayerUIClientRPC()
    {
        hPBarUIController.gameObject.SetActive(false);
        userNameUIController.gameObject?.SetActive(false);
    }
}
