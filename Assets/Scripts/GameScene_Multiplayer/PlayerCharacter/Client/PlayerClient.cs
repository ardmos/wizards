using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Ŭ���̾�Ʈ �� �÷��̾� ���� Ŭ�����Դϴ�.
/// �÷��̾��� ������ ������� Ŭ���̾�Ʈ��� �⺻������ ���� ���۵��� �����ϴ� �߻� Ŭ�����Դϴ�.
/// </summary>
public abstract class PlayerClient : NetworkBehaviour
{
    #region Singleton
    public static PlayerClient Instance { get; private set; }
    #endregion

    #region Components
    public PlayerServer playerServer;
    public TeamColorController teamColorController;
    public UserNameUIController userNameUIController;
    public HPBarUIController hPBarUIController;
    public DamageTextUIController damageTextUIController;
    public CinemachineVirtualCamera VirtualCamera;
    public Rigidbody mRigidbody;
    public AudioListener audioListener;
    public PlayerScrollCounter playerScrollCounter;
    public GameInput gameInput;
    public CameraShake cameraShake;
    #endregion

    #region Events
    public event EventHandler OnPlayerGameOver;
    #endregion

    #region Materials
    private Material currentMaterial;
    public Material originalMaterial;
    public Material highlightMaterial;
    public Material frozenMaterial;
    public Material enemyMaterial;
    public Renderer[] ownRenderers;
    #endregion

    #region Player Data
    [SerializeField] private Dictionary<ItemName, ushort> playerItemDictionaryOnClient;
    #endregion

    #region Initialization
    [ClientRpc]
    public virtual void InitializePlayerClientRPC()
    {
        Debug.Log($"OwnerClientId {OwnerClientId} Player InitializePlayerClientRPC");

        // ī�޶� ��ġ �ʱ�ȭ. �����ڸ� ����ٴϵ��� �� 
        VirtualCamera?.gameObject.SetActive(IsOwner);
        // �ڲ� isKinematic�� ������ �߰��� �ڵ�. Rigidbody network���� ��� �Ѵ� �� ����.
        mRigidbody.isKinematic = false;
        // �÷��̾� �г��� ǥ��
        PlayerInGameData playerData = CurrentPlayerDataManager.Instance.GetPlayerDataByClientId(OwnerClientId);
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

        // Input Action �̺�Ʈ ����
        SubscribeToInputEvents();
    }
    #endregion

    #region Event Subscription
    private void SubscribeToInputEvents()
    {
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
        UnsubscribeFromInputEvents();
    }

    private void UnsubscribeFromInputEvents()
    {
        gameInput.OnAttack1Started -= GameInput_OnAttack1Started;
        gameInput.OnAttack2Started -= GameInput_OnAttack2Started;
        gameInput.OnAttack3Started -= GameInput_OnAttack3Started;
        gameInput.OnAttack1Ended -= GameInput_OnAttack1Ended;
        gameInput.OnAttack2Ended -= GameInput_OnAttack2Ended;
        gameInput.OnAttack3Ended -= GameInput_OnAttack3Ended;
        gameInput.OnDefenceStarted -= GameInput_OnDefenceStarted;
        gameInput.OnDefenceEnded -= GameInput_OnDefenceEnded;
    }
    #endregion

    #region Input Handlers
    protected abstract void GameInput_OnAttack1Started(object sender, EventArgs e);
    protected abstract void GameInput_OnAttack2Started(object sender, EventArgs e);
    protected abstract void GameInput_OnAttack3Started(object sender, EventArgs e);
    protected abstract void GameInput_OnDefenceStarted(object sender, EventArgs e);
    protected abstract void GameInput_OnAttack1Ended(object sender, EventArgs e);
    protected abstract void GameInput_OnAttack2Ended(object sender, EventArgs e);
    protected abstract void GameInput_OnAttack3Ended(object sender, EventArgs e);
    protected abstract void GameInput_OnDefenceEnded(object sender, EventArgs e);
    #endregion

    #region Visual Effects
    /// <summary>
    /// ĳ���� ������ ����Ʈ�� �����մϴ�.
    /// </summary>
    [ClientRpc]
    public void ActivateFrozenEffectClientRPC()
    {
        currentMaterial = frozenMaterial;
        foreach (Renderer renderer in ownRenderers)
        {
            renderer.material = frozenMaterial;
        }
    }

    /// <summary>
    /// ĳ���� ������ ����Ʈ�� �����մϴ�.
    /// </summary>
    [ClientRpc]
    public void DeactivateFrozenEffectClientRPC()
    {
        currentMaterial = originalMaterial;
        foreach (Renderer renderer in ownRenderers)
        {
            renderer.material = originalMaterial;
        }
    }

    /// <summary>
    /// ĳ������ �ǰ� ����Ʈ�� �����մϴ�.
    /// </summary>
    public void ActivateHitByAttackEffect()
    {
        foreach(Renderer renderer in ownRenderers)
        {
            renderer.material = highlightMaterial;
        }
        
        StartCoroutine(ResetFlashEffect());
    }

    /// <summary>
    /// �ǰ� ĳ���� ��¦ ȿ���� ���� �ð� �Ŀ� ��Ȱ��ȭ�ϴ� �ڷ�ƾ �޼����Դϴ�.
    /// </summary>
    private IEnumerator ResetFlashEffect()
    {
        yield return new WaitForSeconds(0.1f);

        foreach (Renderer renderer in ownRenderers)
        {
            renderer.material = currentMaterial;
        }
    }
    #endregion

    #region Camera Effects
    /// <summary>
    /// �ٸ� ĳ���͸� ���߽����� �� �ߵ��Ǵ� ī�޶� ����ũ ȿ���� �����մϴ�.
    /// </summary>
    [ClientRpc]
    public void ActivateHitCameraShakeClientRPC()
    {
        if (!IsOwner) return;
        cameraShake.ShakeCamera(3f, 0.2f);
    }

    /// <summary>
    /// �ǰ� ī�޶� ����ũ ȿ���� �����մϴ�.
    /// </summary>
    public void ActivateHitByAttackCameraShake()
    {
        if (!IsOwner) return;
        cameraShake.ShakeCamera(5f, 0.2f);
    }

    /// <summary>
    /// �ǰ� ī�޶� �׵θ� ȿ���� �����մϴ�.
    /// </summary>
    public void ActivateHitByAttackCameraEffect()
    {
        if (!IsOwner) return;

        FindObjectOfType<Volume>().profile.TryGet(out Vignette vignette);
        vignette.intensity.value = 0.25f;
        StartCoroutine(ResetCameraEffect(vignette));
    }
    /// <summary>
    /// �ǰ� �׵θ� ȿ���� ���� �ð� �Ŀ� ��Ȱ��ȭ�ϴ� �ڷ�ƾ �޼����Դϴ�.
    /// </summary>
    private IEnumerator ResetCameraEffect(Vignette vignette)
    {
        yield return new WaitForSeconds(0.1f);
        vignette.intensity.value = 0f;
    }
    #endregion

    #region UI Management
    /// <summary>
    /// ������ �ؽ�Ʈ �˾��� ǥ���մϴ�.
    /// </summary>
    public void ShowDamageTextPopup(sbyte damageAmount)
    {
        if(damageTextUIController == null) return;

        damageTextUIController.CreateTextObject(damageAmount);
    }

    /// <summary>
    /// �÷��̾� UI�� ��Ȱ��ȭ�մϴ�.
    /// </summary>
    [ClientRpc]
    public void OffPlayerUIClientRPC()
    {
        if (hPBarUIController == null || userNameUIController == null) return;

        hPBarUIController.gameObject.SetActive(false);
        userNameUIController.gameObject.SetActive(false);
    }
    #endregion

    #region Game Over Handling
    /// <summary>
    /// ���� ���� �� �����ϴ� �޼����Դϴ�.
    /// </summary>
    [ClientRpc]
    public void SetPlayerGameOverClientRPC()
    {
        if (!IsOwner) return;

        OnPlayerGameOver.Invoke(this, EventArgs.Empty);
        GameSceneUIManager.Instance.popupGameOverUIController.Show();
        SoundManager.Instance.PlayLosePopupSound();
        SaveGameOverResult();
    }

    /// <summary>
    /// ���� ���� ����� �����մϴ�.
    /// </summary>
    private void SaveGameOverResult()
    {
        PlayerOutGameData playerOutGameData = PlayerDataManager.Instance.GetPlayerOutGameData();

        if (playerOutGameData == null) return;

        float score = GetPlayerScore();

        if (playerOutGameData.hightestKOinOneMatch < score)
        {
            playerOutGameData.hightestKOinOneMatch = (byte)score;
        }
        playerOutGameData.knockOuts += (uint)score;
        playerOutGameData.totalScore = playerOutGameData.knockOuts * 2;

        PlayerDataManager.Instance.UpdatePlayerData(playerOutGameData);
    }
    #endregion

    #region Game Win Handling
    /// <summary>
    /// ���� �¸� �� �����ϴ� �޼����Դϴ�.
    /// </summary>
    [ClientRpc]
    public void SetPlayerGameWinClientRPC()
    {
        if (!IsOwner) return;

        GameSceneUIManager.Instance.popupWinUIController.Show();
        SoundManager.Instance.PlayWinPopupSound();
        SaveWinResult();
    }

    /// <summary>
    /// �÷��̾��� ������ �¸� ����� �߰��մϴ�.
    /// </summary>
    private void SaveWinResult()
    {
        PlayerOutGameData playerOutGameData = PlayerDataManager.Instance.GetPlayerOutGameData();

        if (playerOutGameData == null) return;

        float score = GetPlayerScore();

        if (playerOutGameData.hightestKOinOneMatch < score)
        {
            playerOutGameData.hightestKOinOneMatch = (byte)score;
        }
        playerOutGameData.mostWins++;
        playerOutGameData.soloVictories++;
        playerOutGameData.knockOuts += (uint)score;
        playerOutGameData.totalScore = playerOutGameData.knockOuts * 2;

        PlayerDataManager.Instance.UpdatePlayerData(playerOutGameData);
    }
    #endregion

    #region Scroll Management
    /// <summary>
    /// ��ũ�� ȹ�� �� ȣ��Ǵ� �޼����Դϴ�.
    /// </summary>
    [ClientRpc]
    public void PickUpScrollClientRPC(int scrollCount)
    {
        if (!IsOwner) return;

        playerScrollCounter.UpdateScrollCount(scrollCount);
        SoundManager.Instance.PlayItemSFXServerRPC(ItemName.ScrollPickup, transform.position);
        GameSceneUIManager.Instance.itemAcquireUIController.ShowScrollAcquiredUI();
    }

    /// <summary>
    /// ��ũ�� ��� �� ȣ��Ǵ� �޼����Դϴ�.
    /// </summary>
    [ClientRpc]
    public void UseScrollClientRPC(int scrollCount)
    {
        if (!IsOwner) return;

        playerScrollCounter.UpdateScrollCount(scrollCount);
        SoundManager.Instance.PlayItemSFXServerRPC(ItemName.ScrollUse, transform.position);
        playerServer.ActivateScrollUseVFXServerRPC();
    }

    /// <summary>
    /// ��ũ�� ȿ�� ���� �˾� UI�� �ʱ�ȭ�մϴ�.
    /// </summary>
    [ClientRpc]
    public void InitSelectScrollEffectsPopupUIClientRPC(SkillUpgradeOptionDTO[] skillUpgradeOptionsDTO)
    {
        if (!IsOwner) return;
        List<ISkillUpgradeOption> skillUpgradeOptions = SkillUpgradeFactory.FromDTOList(skillUpgradeOptionsDTO.ToList());
        GameSceneUIManager.Instance.popupSelectScrollEffectUIController.InitPopup(skillUpgradeOptions);
    }
    #endregion

    #region Player Data
    public int GetPlayerScore()
    {
        if (CurrentPlayerDataManager.Instance == null) return 0;

        return CurrentPlayerDataManager.Instance.GetPlayerScore(OwnerClientId);
    } 
    #endregion
}
