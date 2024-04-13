using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;


// Player 클래스 마저 분리해야함. 
// PlayerClient와 PlayerServer로 코드 분류중. 마저 분류하고 인터페이스 적용까지 하기. Knight에도.
// 1. WIzard, Knight에 알맞게 abstract 메서드 내용 구현하기
// 2. GetScore, GetSpellController 등 필요성 확인 후 위치or삭제하기
// 3. 

/// <summary>
/// 1. 인게임에서 사용되는 플레이어 스크립트입니다.
/// </summary>
public class Player : NetworkBehaviour
{
/*    public static Player Instance { get; private set; }
*/
/*    public event EventHandler OnPlayerGameOver;
    public event EventHandler OnPlayerWin;*/


    // PlayerData변수를 만들어서 사용하는것으로 코드 정리 해야함. HP같은 변수들 따로 있을 이유가 없음.
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

/*    // 플레이어가 보유한 장비 현황. 클라이언트 저장 버전. 서버측 저장버전과 동기화 시켜준다.
    [SerializeField] private Dictionary<ItemName, ushort> playerItemDictionaryOnClient;*/

/*    private void Update()
    {
        if (!IsOwner) return;

        GetComponent<PlayerMovementClient>().HandleMovementServerAuth();
    }*/

/*    /// <summary>
    /// 서버측 InitializePlayer
    /// 1. 스폰위치 초기화
    /// 2. HP 초기화 & 브로드캐스팅
    /// 3. 특정 플레이어가 보유한 스킬 목록 저장 & 해당플레이어에게 공유
    /// </summary>
    /// <param name="ownedSpellList"></param>
    public void InitializePlayerOnServer(SpellName[] ownedSpellList, ulong requestedInitializeClientId)
    {
        Debug.Log($"OwnerClientId{OwnerClientId} Player InitializePlayerOnServer");

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
    }*/

/*    [ClientRpc]
    private void InitializePlayerClientRPC(SpellName[] ownedSpellNameList)
    {
        Debug.Log($"OwnerClientId {OwnerClientId} Player InitializePlayerClientRPC");

        // 카메라 위치 초기화. 소유자만 따라다니도록 함 
        virtualCameraObj.SetActive(IsOwner);
        // 자꾸 isKinematic이 켜져서 추가한 코드. Rigidbody network에서 계속 켜는 것 같다.
        GetComponent<Rigidbody>().isKinematic = false;
        // 플레이어 닉네임 설정
        PlayerInGameData playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(OwnerClientId);
        userNameUIController.Setup(playerData.playerName.ToString(), IsOwner);
        //Debug.Log($"player Name :{playerData.playerName.ToString()}");

        if (!IsOwner) return;

        Instance = this;

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
        if(!IsOwner) return;

        OnPlayerGameOver.Invoke(this, EventArgs.Empty);
        // Popup 보여주기
        GameSceneUIManager.Instance.popupGameOverUIController.Show();
        // BGM 재생
        SoundManager.Instance.PlayLosePopupSound();
    }

    [ClientRpc]
    public void SetPlayerGameWinClientRPC()
    {
        // 이 승리 캐릭터의 소유자가 아니면 리턴.
        if (!IsOwner) return;

        OnPlayerWin.Invoke(this, EventArgs.Empty);
        // Popup 보여주기
        GameSceneUIManager.Instance.popupWinUIController.Show();
        // BGM 재생
        SoundManager.Instance.PlayWinPopupSound();
    }*/

/*    #region Public 플레이어 정보 확인
    public int GetScore()
    {
        return score;
    }
    #endregion
*/

    #region 스킬 스크롤 획득시 동작들
/*    [ClientRpc]
    public void UpdateScrollQueueClientRPC(byte[] scrollSpellSlotArray)
    {
        if (!IsOwner) return;

        Queue<byte> scrollSpellSlotQueue = new Queue<byte>(scrollSpellSlotArray);

        // Queue 업데이트
        SpellManager.Instance.UpdatePlayerScrollSpellSlotQueueOnClient(scrollSpellSlotQueue);

        // SFX 실행
        SoundManager.Instance.PlayOpenScrollSound();
    }

    [ClientRpc]
    public void ShowItemAcquiredUIClientRPC()
    {
        if (!IsOwner) return;
        // 알림 UI 실행
        GameSceneUIManager.Instance.itemAcquireUIController.ShowItemAcquireUI();
    }*/
/*
    /// <summary>
    /// 서버에서 제공해준 스크롤 효과 목록을 PopupSelectScrollEffectUIController에 적용.
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
