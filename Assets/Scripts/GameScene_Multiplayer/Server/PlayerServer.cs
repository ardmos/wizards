using Unity.Netcode;
using UnityEngine;

/// <summary>
/// ���� �� �÷��̾� ���� Ŭ�����Դϴ�.
/// </summary>
public class PlayerServer : NetworkBehaviour
{
    #region Constants
    private const int DEFAULT_SCORE = 300;
    private const float ITEM_OFFSET_X = 0.5f;
    #endregion

    #region Fields & Components
    public PlayerClient playerClient;
    public PlayerHPManagerServer playerHPManager;
    public PlayerAnimator playerAnimator;
    public SpellManagerServer spellManagerServer;

    [Header("���� ����")]
    public Rigidbody rb;
    public Collider mCollider;
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
    /// ������ �÷��̾� �ʱ�ȭ �޼����Դϴ�.
    /// </summary>
    /// <param name="character">ĳ���� �������̽�</param>
    public void InitializePlayerOnServer(ICharacter character)
    {
        if (GameAssetsManager.Instance == null)
        {
            Debug.LogError($"{nameof(InitializePlayerOnServer)}, GameAssets�� ã�� ���߽��ϴ�.");
            return;
        }

        PlayerSpawnPointsController spawnPointsController = FindObjectOfType<PlayerSpawnPointsController>();
        if (spawnPointsController == null)
        {
            Debug.LogError($"{nameof(InitializePlayerOnServer)}, ������ġ�� Ư������ ���߽��ϴ�.");
            return;
        }

        // ���� ��ġ �ʱ�ȭ       
        transform.position = spawnPointsController.GetSpawnPoint();
        // HP �ʱ�ȭ
        playerHPManager.InitPlayerHP(character);
        // �÷��̾ ������ ��ų ��� ����
        spellManagerServer.InitPlayerSpellInfoArrayOnServer(character.skills);
        // Ŭ���̾�Ʈ�� �÷��̾ �ʱ�ȭ ����
        playerClient.InitializePlayerClientRPC();
    }
    #endregion

    #region Visual Effects
    /// <summary>
    /// ���� ��ũ�� ���� ȣ��Ǵ� ��ų ��ȭ VFX�� �����մϴ�.
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
    /// �÷��̾��� ���� HP�� ��ȯ�մϴ�.
    /// </summary>
    public sbyte GetPlayerHP()
    {
        if (playerHPManager == null) return 0;

        return playerHPManager.GetHP();
    }
    #endregion

    #region Game Over Handling
    /// <summary>
    /// ���ӿ��� ó���� �����մϴ�.
    /// </summary>
    /// <param name="attackerClientId">������ Ŭ���̾�Ʈ�� ID</param>
    public void GameOver(ulong attackerClientId)
    {
        if (!ValidateGameOverConditions()) return;

        DisablePhysicsAndCollisions();
        tag = "GameOver";

        // �� �÷��̾ óġ�� �÷��̾�鿡�� ������ �ο�
        GiveScoreToAttackerWhenGameOver(attackerClientId);

        // �� �÷��̾� ���ӿ��� �ִϸ��̼� ����
        playerAnimator.UpdatePlayerAnimationOnServer(PlayerMoveAnimState.GameOver);

        // �� �÷��̾� ���� �Ұ� ó�� �� ���ӿ��� �˾� ����.
        playerClient.SetPlayerGameOverClientRPC();
        // �� �÷��̾��� '�̸� & HP' UI off
        playerClient.OffPlayerUIClientRPC();

        // �� �÷��̾��� ���ӿ��� ����� ������ ���.
        MultiplayerGameManager.Instance.UpdatePlayerGameOverOnServer(OwnerClientId, attackerClientId);
        
        // ������ ��� ���� ����
        GenerateAndDropItemWhenGameOver();
    }

    /// <summary>
    /// �� �÷��̾ óġ�� �÷��̾�鿡�� ������ �ο��ϴ� �޼����Դϴ�.
    /// </summary>
    /// <param name="attackerClientId"></param>
    private void GiveScoreToAttackerWhenGameOver(ulong attackerClientId)
    {
        // ȯ�� ��ҵ����� ���� ���ӿ��� ���� ���, ���� �� ��� �÷��̾�鿡�� ������ �ݴϴ�. 
        if (attackerClientId == OwnerClientId)
        {
            foreach (PlayerInGameData playerInGameData in ServerNetworkManager.Instance.GetPlayerDataNetworkList())
            {
                ServerNetworkManager.Instance.AddPlayerScore(playerInGameData.clientId, DEFAULT_SCORE);
            }
        }
        // �Ϲ����� ��� ��� �÷��̾� 300���ھ� ȹ��
        else
        {
            ServerNetworkManager.Instance.AddPlayerScore(attackerClientId, DEFAULT_SCORE);
        }
    }

    /// <summary>
    /// ���� �� �浹 ��Ȱ��ȭ ������ ���� �޼����Դϴ�.
    /// </summary>
    private void DisablePhysicsAndCollisions()
    {
        rb.isKinematic = true;
        mCollider.enabled = false;
    }
    #endregion

    #region Generate Drop Item Handling
    /// <summary>
    /// ���ӿ����� �������� ������ִ� �޼����Դϴ�.
    /// HP���ǰ� Scroll�������� ����մϴ�. 
    /// </summary>
    private void GenerateAndDropItemWhenGameOver()
    {
        DropItem(GameAssetsManager.Instance.GetItemHPPotionObject(), ITEM_OFFSET_X);
        DropItem(GameAssetsManager.Instance.GetItemScrollObject(), -ITEM_OFFSET_X);
    }

    /// <summary>
    /// �Ű������� ���޹��� ������ �������� �� ĳ������ ������������ν� ���� ���� ��ȯ���ִ� �޼��� �Դϴ�.
    /// </summary>
    /// <param name="itemPrefab">������ �������� ������</param>
    /// <param name="offsetX">������ ���� ��ġ ������</param>
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
    /// ��� �������� ��Ʈ��ũ ������Ʈ�ν� ������Ű�� �޼����Դϴ�.
    /// </summary>
    /// <param name="obj">������ ��Ʈ��ũ ������Ʈ</param>
    /// <param name="position">������ ���� ��ġ</param>
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
    /// GameOver �޼��带 �����ϱ� ���� �ʿ� ���ǵ��� �����ϴ� �޼��� �Դϴ�.
    /// </summary>
    /// <returns>���� ����� bool ���·� ��ȯ�մϴ�</returns>
    private bool ValidateGameOverConditions()
    {
        if (ServerNetworkManager.Instance == null) return false;
        PlayerInGameData playerData = ServerNetworkManager.Instance.GetPlayerDataFromClientId(OwnerClientId);
        if (playerData.playerGameState != PlayerGameState.Playing) return false;
        if (playerAnimator == null) return false;
        if (playerClient == null) return false;
        if (rb == null || mCollider == null) return false;

        return true;
    }
    #endregion
}
