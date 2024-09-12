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

    #region Fields
    public PlayerClient playerClient;
    public PlayerHPManagerServer playerHPManager;
    public PlayerAnimator playerAnimator;
    public SpellManagerServer spellManagerServer;

    [Header("물리 관련")]
    public Rigidbody rb;
    public Collider _collider;
    #endregion

    #region Network Lifecycle
    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        ICharacter character = (ICharacter)playerClient;
        InitializePlayerOnServer(character);
    }
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

        PlayerSpawnPointsController spawnPointsController = FindObjectOfType<PlayerSpawnPointsController>();
        if (spawnPointsController == null)
        {
            Debug.LogError($"{nameof(InitializePlayerOnServer)}, 스폰위치를 특정하지 못했습니다.");
            return;
        }

        // 스폰 위치 초기화       
        transform.position = spawnPointsController.GetSpawnPoint();
        // HP 초기화
        playerHPManager.InitPlayerHP(character);
        // 플레이어가 보유한 스킬 목록 저장
        spellManagerServer.InitPlayerSpellInfoArrayOnServer(character.skills);
        // 클라이언트측 플레이어도 초기화 실행
        playerClient.InitializePlayerClientRPC();
    }
    #endregion

    #region Visual Effects
    /// <summary>
    /// 마법 스크롤 사용시 호출되는 스킬 강화 VFX를 실행합니다.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void ActivateScrollUseVFXServerRPC()
    {
        GameObject vfxHeal = Instantiate(GameAssetsManager.Instance.gameAssets.vfx_SpellUpgrade, transform);
        vfxHeal.GetComponent<NetworkObject>().Spawn();
        vfxHeal.transform.SetParent(transform);
    }
    #endregion

    #region Game State Management
    /// <summary>
    /// 게임오버 처리를 수행합니다.
    /// </summary>
    /// <param name="attackerClientId">공격한 클라이언트의 ID</param>
    public void GameOver(ulong attackerClientId)
    {
        PlayerInGameData playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(OwnerClientId);
        if (playerData.playerGameState != PlayerGameState.Playing) return;

        rb.isKinematic = true;
        _collider.enabled = false;
        tag = "GameOver";

        // 현 플레이어를 처치한 플레이어들에게 점수를 부여
        GiveScoreToAttacker(attackerClientId);
        // 현 플레이어 게임오버 애니메이션 실행
        playerAnimator.UpdatePlayerMoveAnimationOnServer(PlayerMoveAnimState.GameOver);
        // 현 플레이어 조작 불가 처리 및 게임오버 팝업 띄우기.
        playerClient.SetPlayerGameOverClientRPC();
        // 현 플레이어의 '이름 & HP' UI off
        playerClient.OffPlayerUIClientRPC();
        // 현 플레이어의 게임오버 사실을 서버에 기록.
        MultiplayerGameManager.Instance.UpdatePlayerGameOverOnServer(OwnerClientId, attackerClientId);
        // 아이템 드랍 로직 실행
        GenerateAndDropItemWhenGameOver();
    }
    #endregion

    #region Player Data
    /// <summary>
    /// 플레이어의 현재 HP를 반환합니다.
    /// </summary>
    public sbyte GetPlayerHP()
    {
        return playerHPManager.GetHP();
    }
    #endregion

    #region Game Over Handling
    /// <summary>
    /// 현 플레이어를 처치한 플레이어들에게 점수를 부여하는 메서드입니다.
    /// </summary>
    /// <param name="attackerClientId"></param>
    private void GiveScoreToAttacker(ulong attackerClientId)
    {
        // 환경 요소등으로 인해 게임오버 당한 경우, 게임 내 모든 플레이어들에게 점수를 줍니다. 
        if (attackerClientId == OwnerClientId)
        {
            foreach (PlayerInGameData playerInGameData in GameMultiplayer.Instance.GetPlayerDataNetworkList())
            {
                GameMultiplayer.Instance.AddPlayerScore(playerInGameData.clientId, DEFAULT_SCORE);
            }
        }
        // 일반적인 경우 상대 플레이어 300스코어 획득
        else
        {
            GameMultiplayer.Instance.AddPlayerScore(attackerClientId, DEFAULT_SCORE);
        }
    }

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
    /// 매개변수로 전달받은 아이템 드랍을 실행하는 메서드 입니다.
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
}
