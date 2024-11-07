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
/// 클라이언트 측 플레이어 관리 클래스입니다.
/// 플레이어의 직업에 상관없이 클라이언트라면 기본적으로 갖는 동작들을 관리하는 추상 클래스입니다.
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

        // 카메라 위치 초기화. 소유자만 따라다니도록 함 
        VirtualCamera?.gameObject.SetActive(IsOwner);
        // 자꾸 isKinematic이 켜져서 추가한 코드. Rigidbody network에서 계속 켜는 것 같다.
        mRigidbody.isKinematic = false;
        // 플레이어 닉네임 표시
        PlayerInGameData playerData = CurrentPlayerDataManager.Instance.GetPlayerDataByClientId(OwnerClientId);
        userNameUIController?.SetName(playerData.playerName.ToString());

        // 오디오리스너 초기화
        audioListener.enabled = IsOwner;

        // 기본 메터리얼 초기화
        currentMaterial = originalMaterial;

        // 플레이어 UI 컬러 설정
        teamColorController?.Setup(IsOwner);

        if (!IsOwner) {
            // enemy 플레이어 테두리 컬러 설정
            foreach (Renderer renderer in ownRenderers)
            {
                renderer.material = enemyMaterial;
            }        
            // 추후 동맹시스템 추가시 여기서 동맹컬러 설정 분기 나눠줘야합니다. 
            return;
        }
        

        Instance = this;

        // Input Action 이벤트 구독
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
        // 구독 해제
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
    /// 캐릭터 프로즌 이펙트를 실행합니다.
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
    /// 캐릭터 프로즌 이펙트를 종료합니다.
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
    /// 캐릭터의 피격 이펙트를 실행합니다.
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
    /// 피격 캐릭터 반짝 효과를 일정 시간 후에 비활성화하는 코루틴 메서드입니다.
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
    /// 다른 캐릭터를 적중시켰을 때 발동되는 카메라 쉐이크 효과를 실행합니다.
    /// </summary>
    [ClientRpc]
    public void ActivateHitCameraShakeClientRPC()
    {
        if (!IsOwner) return;
        cameraShake.ShakeCamera(3f, 0.2f);
    }

    /// <summary>
    /// 피격 카메라 쉐이크 효과를 실행합니다.
    /// </summary>
    public void ActivateHitByAttackCameraShake()
    {
        if (!IsOwner) return;
        cameraShake.ShakeCamera(5f, 0.2f);
    }

    /// <summary>
    /// 피격 카메라 테두리 효과를 실행합니다.
    /// </summary>
    public void ActivateHitByAttackCameraEffect()
    {
        if (!IsOwner) return;

        FindObjectOfType<Volume>().profile.TryGet(out Vignette vignette);
        vignette.intensity.value = 0.25f;
        StartCoroutine(ResetCameraEffect(vignette));
    }
    /// <summary>
    /// 피격 테두리 효과를 일정 시간 후에 비활성화하는 코루틴 메서드입니다.
    /// </summary>
    private IEnumerator ResetCameraEffect(Vignette vignette)
    {
        yield return new WaitForSeconds(0.1f);
        vignette.intensity.value = 0f;
    }
    #endregion

    #region UI Management
    /// <summary>
    /// 데미지 텍스트 팝업을 표시합니다.
    /// </summary>
    public void ShowDamageTextPopup(sbyte damageAmount)
    {
        if(damageTextUIController == null) return;

        damageTextUIController.CreateTextObject(damageAmount);
    }

    /// <summary>
    /// 플레이어 UI를 비활성화합니다.
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
    /// 게임 오버 시 동작하는 메서드입니다.
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
    /// 게임 오버 결과를 저장합니다.
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
    /// 게임 승리 시 동작하는 메서드입니다.
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
    /// 플레이어의 전적에 승리 결과를 추가합니다.
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
    /// 스크롤 획득 시 호출되는 메서드입니다.
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
    /// 스크롤 사용 시 호출되는 메서드입니다.
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
    /// 스크롤 효과 선택 팝업 UI를 초기화합니다.
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
