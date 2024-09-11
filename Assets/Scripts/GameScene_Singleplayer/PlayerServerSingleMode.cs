using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


public class PlayerServerSingleMode : NetworkBehaviour
{
    private const int DEFAULT_SCORE = 300;

    public PlayerClient playerClient;
    public PlayerHPManagerServer playerHPManager;
    public PlayerAnimator playerAnimator;
    public SpellManagerServer skillSpellManagerServer;
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
        transform.position = spawnPointsController.GetSpawnPoint();

        // HP �ʱ�ȭ
        // ���� HP ���� �� ����
        playerHPManager.InitPlayerHP(character);

        // �÷��̾ ������ ��ų ��� ����
        skillSpellManagerServer.InitPlayerSpellInfoArrayOnServer(character.skills);

        Debug.Log("0");
        // �÷��̾� InitializePlayer ����, ��ų ����� Ŭ���̾�Ʈ��(SpellController)�� ���� ( �����ؾ���
        playerClient.InitializePlayerClientRPC();
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

    // ���ӿ��� ó��. �������� ���.
    public void GameOver(ulong clientWhoAttacked)
    {
        PlayerInGameData playerData = GameSingleplayer.Instance.GetPlayerDataFromClientId(OwnerClientId);

        if (playerData.playerGameState != PlayerGameState.Playing) return;
        Debug.Log($"Player{OwnerClientId} is GameOver");
        //playerData.playerGameState = PlayerGameState.GameOver;

        // �����浹 ����
        rb.isKinematic = true;
        _collider.enabled = false;
        // ���������� �ʵ��� Tag ����
        tag = "GameOver";

        // ���� ���
        CalcScore(clientWhoAttacked);
        // �÷��̾� ���ӿ��� �ִϸ��̼� ����
        playerAnimator.UpdatePlayerMoveAnimationOnServer(PlayerMoveAnimState.GameOver);
        // �ش� �÷��̾� ���� �Ұ� ó�� �� ���ӿ��� �˾� ����.
        playerClient.SetPlayerGameOverClientRPC();
        // �÷��̾� �̸� & HP UI off
        playerClient.OffPlayerUIClientRPC();
        // ���ӿ��� �÷��̾� ����� ������ ���.
        SingleplayerGameManager.Instance.UpdatePlayerGameOverOnServer(OwnerClientId, clientWhoAttacked);

        // ������ ���
        DropItem();
    }

    private void CalcScore(ulong clientWhoAttacked)
    {
        // ������ ���ӿ��� ���� ���, ���� �� ��� �÷��̾�鿡�� ������ �ݴϴ�. 
        if (clientWhoAttacked == OwnerClientId)
        {
            foreach (PlayerInGameData playerInGameData in GameSingleplayer.Instance.GetPlayerDataNetworkList())
            {
                GameSingleplayer.Instance.AddPlayerScore(playerInGameData.clientId, DEFAULT_SCORE);
            }
        }
        // �Ϲ����� ��� ��� �÷��̾� 300���ھ� ȹ��
        else
        {
            GameSingleplayer.Instance.AddPlayerScore(clientWhoAttacked, DEFAULT_SCORE);
        }
    }

    private void DropItem()
    {
        // ���ʷ���Ʈ ������ ����
        Vector3 newItemHPPotionPos = new Vector3(transform.position.x + 0.5f, transform.position.y, transform.position.z);
        GameObject hpPotionObject = Instantiate(GameAssetsManager.Instance.GetItemHPPotionObject(), newItemHPPotionPos, transform.rotation, SingleplayerGameManager.Instance.transform);

        if (!hpPotionObject) return;

        if (hpPotionObject.TryGetComponent<NetworkObject>(out NetworkObject hpPotionObjectNetworkObject))
        {
            hpPotionObjectNetworkObject.Spawn();
            if (SingleplayerGameManager.Instance)
            {
                hpPotionObject.transform.parent = SingleplayerGameManager.Instance.transform;
                hpPotionObject.transform.position = newItemHPPotionPos;
            }
        }

        // ���ʷ���Ʈ ������ ��ũ��
        Vector3 newItemScrollPos = new Vector3(transform.position.x - 0.5f, transform.position.y, transform.position.z);
        GameObject scrollObject = Instantiate(GameAssetsManager.Instance.GetItemScrollObject(), newItemScrollPos, transform.rotation, SingleplayerGameManager.Instance.transform);

        if (!scrollObject) return;

        if (scrollObject.TryGetComponent<NetworkObject>(out NetworkObject scrollObjectNetworkObject))
        {
            scrollObjectNetworkObject.Spawn();
            if (SingleplayerGameManager.Instance)
            {
                scrollObject.transform.parent = SingleplayerGameManager.Instance.transform;
                scrollObject.transform.position = newItemScrollPos;
            }
        }
    }
}
