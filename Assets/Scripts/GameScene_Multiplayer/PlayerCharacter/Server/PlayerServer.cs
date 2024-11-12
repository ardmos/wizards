using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 서버 측 플레이어 관리 클래스입니다.
/// </summary>
public class PlayerServer : NetworkBehaviour
{
    #region Constants
    private const int DEFAULT_SCORE = 300;
    private const float ITEM_OFFSET_X = 0.5f;
    #endregion

    #region Components
    [SerializeField] private PlayerClient playerClient;
    [SerializeField] private PlayerHPManagerServer playerHPManagerServer;
    [SerializeField] private PlayerAnimator playerAnimator;
    [SerializeField] private SpellManagerServer spellManagerServer;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Collider mCollider;
    #endregion

    #region Initialization
    /// <summary>
    /// 서버측 플레이어 초기화 메서드입니다.
    /// </summary>
    /// <param name="character">캐릭터 인터페이스</param>
    public void InitializePlayerOnServer(ICharacter character)
    {
        if (GameAssetsManager.Instance == null)
        {
            Debug.LogError($"{nameof(InitializePlayerOnServer)}, GameAssets를 찾지 못했습니다.");
            return;
        }

        // HP 초기화
        playerHPManagerServer.InitPlayerHP(character);
        // 플레이어가 보유한 스킬 목록 저장
        spellManagerServer.InitPlayerSpellInfoArrayOnServer(character.spells);
    }
    #endregion

    #region Visual Effects
    /// <summary>
    /// 마법 스크롤 사용시 호출되는 스킬 강화 VFX를 실행합니다.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void ActivateScrollUseVFXServerRPC()
    {
        GameObject vfxPrefab = GameAssetsManager.Instance.gameAssets.vfx_SpellUpgrade;
        if (vfxPrefab == null) return;

        GameObject vfxHeal = Instantiate(vfxPrefab, transform);
        vfxHeal.GetComponent<NetworkObject>().Spawn();
        vfxHeal.transform.SetParent(transform);
    }
    #endregion

    #region Player Data
    /// <summary>
    /// 플레이어의 현재 HP를 반환합니다.
    /// </summary>
    public sbyte GetPlayerHP()
    {
        if (playerHPManagerServer == null) return 0;

        return playerHPManagerServer.GetHP();
    }
    #endregion

    #region Game Over Handling
    /// <summary>
    /// 게임오버 처리를 수행합니다.
    /// </summary>
    /// <param name="attackerClientId">공격한 클라이언트의 ID</param>
    public void GameOver(ulong attackerClientId)
    {
        if (!ValidateGameOverConditions()) return;

        DisablePhysicsAndCollisions();
        tag = Tags.GameOver;

        // 현 플레이어를 처치한 플레이어들에게 점수를 부여
        GiveScoreToAttackerWhenGameOver(attackerClientId);

        // 현 플레이어 게임오버 애니메이션 실행
        playerAnimator.UpdatePlayerAnimationOnServer(PlayerMoveAnimState.GameOver);

        // 현 플레이어 조작 불가 처리 및 게임오버 팝업 띄우기.
        playerClient.SetPlayerGameOverClientRPC();

        // 현 플레이어의 게임오버 사실을 서버에 기록.
        MultiplayerGameManager.Instance.UpdatePlayerGameOverOnServer(OwnerClientId, attackerClientId);
        
        // 아이템 드랍 로직 실행
        GenerateAndDropItemWhenGameOver();
    }

    /// <summary>
    /// 현 플레이어를 처치한 플레이어들에게 점수를 부여하는 메서드입니다.
    /// </summary>
    /// <param name="attackerClientId"></param>
    private void GiveScoreToAttackerWhenGameOver(ulong attackerClientId)
    {
        // 환경 요소등으로 인해 게임오버 당한 경우, 게임 내 모든 플레이어들에게 점수를 줍니다. 
        if (attackerClientId == OwnerClientId)
        {
            foreach (PlayerInGameData playerInGameData in CurrentPlayerDataManager.Instance.GetCurrentPlayers())
            {
                CurrentPlayerDataManager.Instance.AddPlayerScore(playerInGameData.clientId, DEFAULT_SCORE);
            }
        }
        // 일반적인 경우 상대 플레이어 300스코어 획득
        else
        {
            CurrentPlayerDataManager.Instance.AddPlayerScore(attackerClientId, DEFAULT_SCORE);
        }
    }

    /// <summary>
    /// 물리 및 충돌 비활성화 로직을 담은 메서드입니다.
    /// </summary>
    private void DisablePhysicsAndCollisions()
    {
        rb.isKinematic = true;
        mCollider.enabled = false;
    }
    #endregion

    #region Generate Drop Item Handling
    /// <summary>
    /// 게임오버시 아이템을 드랍해주는 메서드입니다.
    /// HP포션과 Scroll아이템을 드랍합니다. 
    /// </summary>
    private void GenerateAndDropItemWhenGameOver()
    {
        DropItem(GameAssetsManager.Instance.GetItemHPPotionObject(), ITEM_OFFSET_X);
        DropItem(GameAssetsManager.Instance.GetItemScrollObject(), -ITEM_OFFSET_X);
    }

    /// <summary>
    /// 매개변수로 전달받은 아이템 프리팹을 현 캐릭터의 드랍아이템으로써 게임 세상에 소환해주는 메서드 입니다.
    /// </summary>
    /// <param name="itemPrefab">생성할 아이템의 프리팹</param>
    /// <param name="offsetX">아이템 스폰 위치 보정값</param>
    private void DropItem(GameObject itemPrefab, float offsetX)
    {
        Vector3 newItemPos = new Vector3(transform.position.x + offsetX, transform.position.y, transform.position.z);
        GameObject itemObject = Instantiate(itemPrefab, newItemPos, transform.rotation, MultiplayerGameManager.Instance.transform);

        if (!itemObject)
        {
            Debug.LogWarning($"Failed to instantiate item: {itemPrefab.name}");
            return;
        }

        SpawnNetworkObject(itemObject, newItemPos);
    }

    /// <summary>
    /// 드랍 아이템을 네트워크 오브젝트로써 스폰시키는 메서드입니다.
    /// </summary>
    /// <param name="obj">스폰할 네트워크 오브젝트</param>
    /// <param name="position">아이템 스폰 위치</param>
    private void SpawnNetworkObject(GameObject obj, Vector3 position)
    {
        if (obj.TryGetComponent<NetworkObject>(out NetworkObject networkObject))
        {
            networkObject.Spawn();
            if (MultiplayerGameManager.Instance)
            {
                obj.transform.parent = MultiplayerGameManager.Instance.transform;
                obj.transform.position = position;
            }
        }
        else
        {
            Debug.LogWarning($"NetworkObject component not found on {obj.name}");
        }
    }
    #endregion

    #region ValiateConditions
    /// <summary>
    /// GameOver 메서드를 수행하기 전에 필요 조건들을 검증하는 메서드 입니다.
    /// </summary>
    /// <returns>검증 결과를 bool 형태로 반환합니다</returns>
    private bool ValidateGameOverConditions()
    {
        if (ServerNetworkConnectionManager.Instance == null) return false;
        PlayerInGameData playerData = CurrentPlayerDataManager.Instance.GetPlayerDataByClientId(OwnerClientId);
        if (playerData.playerGameState != PlayerGameState.Playing) return false;
        if (playerAnimator == null) return false;
        if (playerClient == null) return false;
        if (rb == null || mCollider == null) return false;

        return true;
    }
    #endregion
}
