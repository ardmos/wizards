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

    [Header("캐릭터용 UI 컨트롤러들")]
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

    // 플레이어가 보유한 장비 현황. 클라이언트 저장 버전. 서버측 저장버전과 동기화 시켜준다.
    [SerializeField] private Dictionary<ItemName, ushort> playerItemDictionaryOnClient;

    [ClientRpc]
    public virtual void InitializePlayerClientRPC(SkillName[] skills)
    {
        Debug.Log($"OwnerClientId {OwnerClientId} Player InitializePlayerClientRPC");

        // 카메라 위치 초기화. 소유자만 따라다니도록 함 
        VirtualCamera?.gameObject.SetActive(IsOwner);
        // 자꾸 isKinematic이 켜져서 추가한 코드. Rigidbody network에서 계속 켜는 것 같다.
        mRigidbody.isKinematic = false;
        // 플레이어 닉네임 설정
        PlayerInGameData playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(OwnerClientId);
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

        // 보유 skill 정보를 GamePad UI에 반영          
        Debug.Log($"PlayerClient.InitializePlayerClientRPC() skills.Length : {skills.Length}");
        GameSceneUIManager.Instance.gamePadUIController.UpdateSpellUI(skills);

        // Input Action 이벤트 구독
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


    // 적중 카메라 쉐이크 효과 실행
    [ClientRpc]
    public void ActivateHitCameraShakeClientRPC()
    {
        if (!IsOwner) return;
        cameraShake.ShakeCamera(3f, 0.2f);
    }

    // 캐릭터 프로즌 이펙트 실행
    [ClientRpc]
    public void ActivateFrozenEffectClientRPC()
    {
        currentMaterial = frozenMaterial;
        foreach (Renderer renderer in ownRenderers)
        {
            renderer.material = frozenMaterial;
        }
    }
    // 캐릭터 프로즌 이펙트 종료
    [ClientRpc]
    public void DeactivateFrozenEffectClientRPC()
    {
        currentMaterial = originalMaterial;
        foreach (Renderer renderer in ownRenderers)
        {
            renderer.material = originalMaterial;
        }
    }

    // 피격 캐릭터 반짝 이펙트 실행
    [ClientRpc]
    public void ActivateHitByAttackEffectClientRPC()
    {
        foreach(Renderer renderer in ownRenderers)
        {
            renderer.material = highlightMaterial;
        }
        
        StartCoroutine(ResetFlashEffect());
    }
    // 피격 캐릭터 반짝 효과를 일정 시간 후에 비활성화하는 코루틴 메서드
    private IEnumerator ResetFlashEffect()
    {
        yield return new WaitForSeconds(0.1f);

        foreach (Renderer renderer in ownRenderers)
        {
            renderer.material = currentMaterial;
        }
    }

    // 피격 사운드 실행

    // 피격 카메라 쉐이크 효과 실행
    [ClientRpc]
    public void ActivateHitByAttackCameraShakeClientRPC()
    {
        if (!IsOwner) return;
        cameraShake.ShakeCamera(5f, 0.2f);
    }

    // 피격 카메라 테두리 효과 실행
    [ClientRpc]
    public void ActivateHitByAttackCameraEffectClientRPC()
    {
        // 피격당한 플레이어에만 실행될 효과
        if (!IsOwner) return;

        FindObjectOfType<Volume>().profile.TryGet(out Vignette vignette);
        vignette.intensity.value = 0.25f;
        StartCoroutine(ResetCameraEffect(vignette));
    }
    // 피격 테두리 효과를 일정 시간 후에 비활성화하는 코루틴 메서드
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
    /// 게임 오버시 동작시키는 메소드.
    /// 1. 플레이어 조작 불가
    /// 2. 게임오버 팝업 띄우기
    /// 3. 팝업 BGM 재생
    /// </summary>
    [ClientRpc]
    public void SetPlayerGameOverClientRPC()
    {
        //Debug.Log($"I'm player {OwnerClientId}");
        // 이 게임오버 캐릭터의 소유자가 아니면 리턴. RPC라 소유자 체크 한 번 해줘야 함. 
        if (!IsOwner) return;

        // GameInput에서 조작 불가하도록 설정
        OnPlayerGameOver.Invoke(this, EventArgs.Empty);
        // Popup 보여주기
        GameSceneUIManager.Instance.popupGameOverUIController.Show();

        // 게임 결과 저장. 게임 오버! ( 기기에 저장중입니다. )
        SaveGameOverResult();

        // BGM 재생
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
        // 이 승리 캐릭터의 소유자가 아니면 리턴.
        if (!IsOwner) return;

        //OnPlayerWin.Invoke(this, EventArgs.Empty);
        // Popup 보여주기
        GameSceneUIManager.Instance.popupWinUIController.Show(OwnerClientId);

        // 게임 결과 저장. 승리! (기기에 저장중입니다)
        SaveWinResult();

        // BGM 재생
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
    /// 스크롤 획득시 호출되는 메서드. 
    /// 1. 우측 UI 업데이트
    /// 2. 중앙 팝업 UI 노출
    /// </summary>
    /// <param name="scrollSpellSlotArray"></param>
    [ClientRpc]
    public void AddScrollClientRPC(int scrollCount)
    {
        if (!IsOwner) return;

        // 1. 우측 UI 업데이트
        playerSpellScrollQueueManagerClient.UpdatePlayerScrollSpellSlotUI(scrollCount);

        // SFX 재생
        SoundManager.Instance.PlayItemSFXServerRPC(ItemName.ScrollPickup, transform.position);

        // 2. 중앙 팝업 UI 노출
        GameSceneUIManager.Instance.itemAcquireUIController.ShowScrollAcquiredUI();
    }

    [ClientRpc]
    public void UpdateScrollClientRPC(int scrollCount)
    {
        if (!IsOwner) return;

        // 1. 우측 UI 업데이트
        playerSpellScrollQueueManagerClient.UpdatePlayerScrollSpellSlotUI(scrollCount);

        // SFX 재생
        SoundManager.Instance.PlayItemSFXServerRPC(ItemName.ScrollUse, transform.position);

        // VFX 재생
        playerServer.StartApplyScrollVFXServerRPC();
    }

    /// <summary>
    /// 서버에서 제공해준 스크롤 효과 목록을 PopupSelectScrollEffectUIController에 적용.
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
