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

    #region Fields
    public PlayerClient playerClient;
    public PlayerHPManagerServer playerHPManager;
    public PlayerAnimator playerAnimator;
    public SpellManagerServer spellManagerServer;

    [Header("���� ����")]
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
        GameObject vfxHeal = Instantiate(GameAssetsManager.Instance.gameAssets.vfx_SpellUpgrade, transform);
        vfxHeal.GetComponent<NetworkObject>().Spawn();
        vfxHeal.transform.SetParent(transform);
    }
    #endregion

    #region Game State Management
    /// <summary>
    /// ���ӿ��� ó���� �����մϴ�.
    /// </summary>
    /// <param name="attackerClientId">������ Ŭ���̾�Ʈ�� ID</param>
    public void GameOver(ulong attackerClientId)
    {
        PlayerInGameData playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(OwnerClientId);
        if (playerData.playerGameState != PlayerGameState.Playing) return;

        rb.isKinematic = true;
        _collider.enabled = false;
        tag = "GameOver";

        // �� �÷��̾ óġ�� �÷��̾�鿡�� ������ �ο�
        GiveScoreToAttacker(attackerClientId);
        // �� �÷��̾� ���ӿ��� �ִϸ��̼� ����
        playerAnimator.UpdatePlayerMoveAnimationOnServer(PlayerMoveAnimState.GameOver);
        // �� �÷��̾� ���� �Ұ� ó�� �� ���ӿ��� �˾� ����.
        playerClient.SetPlayerGameOverClientRPC();
        // �� �÷��̾��� '�̸� & HP' UI off
        playerClient.OffPlayerUIClientRPC();
        // �� �÷��̾��� ���ӿ��� ����� ������ ���.
        MultiplayerGameManager.Instance.UpdatePlayerGameOverOnServer(OwnerClientId, attackerClientId);
        // ������ ��� ���� ����
        GenerateAndDropItemWhenGameOver();
    }
    #endregion

    #region Player Data
    /// <summary>
    /// �÷��̾��� ���� HP�� ��ȯ�մϴ�.
    /// </summary>
    public sbyte GetPlayerHP()
    {
        return playerHPManager.GetHP();
    }
    #endregion

    #region Game Over Handling
    /// <summary>
    /// �� �÷��̾ óġ�� �÷��̾�鿡�� ������ �ο��ϴ� �޼����Դϴ�.
    /// </summary>
    /// <param name="attackerClientId"></param>
    private void GiveScoreToAttacker(ulong attackerClientId)
    {
        // ȯ�� ��ҵ����� ���� ���ӿ��� ���� ���, ���� �� ��� �÷��̾�鿡�� ������ �ݴϴ�. 
        if (attackerClientId == OwnerClientId)
        {
            foreach (PlayerInGameData playerInGameData in GameMultiplayer.Instance.GetPlayerDataNetworkList())
            {
                GameMultiplayer.Instance.AddPlayerScore(playerInGameData.clientId, DEFAULT_SCORE);
            }
        }
        // �Ϲ����� ��� ��� �÷��̾� 300���ھ� ȹ��
        else
        {
            GameMultiplayer.Instance.AddPlayerScore(attackerClientId, DEFAULT_SCORE);
        }
    }

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
    /// �Ű������� ���޹��� ������ ����� �����ϴ� �޼��� �Դϴ�.
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
}
