using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class PlayerServer : NetworkBehaviour
{
    public PlayerClient playerClient;
    public PlayerHPManagerServer playerHPManager;
    public SkillSpellManagerServer skillSpellManagerServer;
    [Header("���� ����")]
    public Rigidbody rb;
    public Collider _collider;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        ICharacter character = (ICharacter)playerClient; //GetComponent<ICharacter>();
        InitializePlayerOnServer(character);
    }

    /// <summary>
    /// ������ InitializePlayer
    /// 1. ������ġ �ʱ�ȭ
    /// 2. HP �ʱ�ȭ & ��ε�ĳ����
    /// 3. Ư�� �÷��̾ ������ ��ų ��� ���� & �ش��÷��̾�� ����
    /// </summary>
    public void InitializePlayerOnServer(ICharacter character)
    {
        //Debug.Log($"OwnerClientId{OwnerClientId} Player (class : {character.characterClass.ToString()}) InitializeAIPlayerOnServer");

        PlayerSpawnPointsController spawnPointsController = FindObjectOfType<PlayerSpawnPointsController>();

        if (GameAssetsManager.Instance == null)
        {
            Debug.Log($"{nameof(InitializePlayerOnServer)}, GameAssets�� ã�� ���߽��ϴ�.");
            return;
        }

        if (spawnPointsController == null)
        {
            Debug.LogError($"{nameof(InitializePlayerOnServer)}, ������ġ�� Ư������ ���߽��ϴ�.");
            return;
        }

        // ���� ��ġ �ʱ�ȭ   
        //transform.position = spawnPointsController.GetSpawnPoint(GameMultiplayer.Instance.GetPlayerDataIndexFromClientId(OwnerClientId));
        transform.position = spawnPointsController.GetSpawnPoint();

        // HP �ʱ�ȭ
        // ���� HP ���� �� ����
        playerHPManager.InitPlayerHP(character);

        // �÷��̾ ������ ��ų ��� ����
        skillSpellManagerServer.InitPlayerSpellInfoArrayOnServer(character.skills);

        // �÷��̾� InitializePlayer ����, ��ų ����� Ŭ���̾�Ʈ��(SpellController)�� ���� ( �����ؾ���
        playerClient.InitializePlayerClientRPC(character.skills);
    }

    // ��ũ�� Ȱ��. ��ų ��ȭ VFX ����
    [ServerRpc(RequireOwnership = false)]
    public void StartApplyScrollVFXServerRPC()
    {
        GameObject vfxHeal = Instantiate(GameAssetsManager.Instance.gameAssets.vfx_SpellUpgrade, transform);
        vfxHeal.GetComponent<NetworkObject>().Spawn();
        vfxHeal.transform.SetParent(transform);
        //vfxHeal.transform.localPosition = new Vector3(0f, 0.1f, 0f);
    }

    public sbyte GetPlayerHP()
    {
        return playerHPManager.GetHP();
    }

    public void GameOver()
    {
        PlayerInGameData playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(OwnerClientId);

        if (playerData.playerGameState != PlayerGameState.Playing) return;
        Debug.Log($"Player{OwnerClientId} is GameOver");
        //playerData.playerGameState = PlayerGameState.GameOver;

        // �����浹 ����
        rb.isKinematic = true;
        _collider.enabled = false;
        // ���������� �ʵ��� Tag ����
        tag = "GameOver";
    }
}
