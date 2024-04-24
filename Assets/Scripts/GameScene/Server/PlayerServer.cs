using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class PlayerServer : NetworkBehaviour
{
    public PlayerClient playerClient;
    public SkillSpellManagerServer skillSpellManagerServer;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        ICharacter character = (ICharacter)playerClient; //GetComponent<ICharacter>();
        InitializePlayerOnServer(character, OwnerClientId);
    }

    /// <summary>
    /// ������ InitializePlayer
    /// 1. ������ġ �ʱ�ȭ
    /// 2. HP �ʱ�ȭ & ��ε�ĳ����
    /// 3. Ư�� �÷��̾ ������ ��ų ��� ���� & �ش��÷��̾�� ����
    /// </summary>
    public void InitializePlayerOnServer(ICharacter character, ulong requestedInitializeClientId)
    {
        Debug.Log($"OwnerClientId{OwnerClientId} Player (class : {character.characterClass.ToString()}) InitializePlayerOnServer");

        PlayerSpawnPointsController spawnPointsController = FindObjectOfType<PlayerSpawnPointsController>();

        if (GameAssets.instantiate == null)
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
        transform.position = spawnPointsController.GetSpawnPoint(GameMultiplayer.Instance.GetPlayerDataIndexFromClientId(OwnerClientId));

        // HP �ʱ�ȭ
        PlayerInGameData playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(requestedInitializeClientId);
        playerData.hp = character.hp;
        playerData.maxHp = character.maxHp;
        GameMultiplayer.Instance.SetPlayerDataFromClientId(requestedInitializeClientId, playerData);

        // ���� HP ���� �� ����
        PlayerHPManager.Instance.UpdatePlayerHP(requestedInitializeClientId, playerData.hp, playerData.maxHp);

        // �÷��̾ ������ ��ų ��� ����
        skillSpellManagerServer.InitPlayerSpellInfoArrayOnServer(character.skills);

        // �÷��̾� InitializePlayer ����, ��ų ����� Ŭ���̾�Ʈ��(SpellController)�� ���� ( �����ؾ���
        playerClient.InitializePlayerClientRPC(character.skills);
    }

    // ��ũ�� Ȱ��. ��ų ��ȭ VFX ����
    [ServerRpc (RequireOwnership = false)]
    public void StartApplyScrollVFXServerRPC()
    {
        GameObject vfxHeal = Instantiate(GameAssets.instantiate.vfx_SpellUpgrade, transform);
        vfxHeal.GetComponent<NetworkObject>().Spawn();
        vfxHeal.transform.SetParent(transform);
        vfxHeal.transform.localPosition = new Vector3(0f, 0.1f, 0f);
    }

    /// <summary>
    /// ��ų �浹 ó��(�������� ����)
    /// �÷��̾� ���߽� ( �ٸ� �����̳� ���������� �浹 ó���� Spell.cs�� �ִ�. �ڵ� ���� �ʿ�)
    /// clientID�� HP �����ؼ� ó��. 
    /// �浹 �༮�� �÷��̾��� ��� ����. 
    /// ClientID�� ����Ʈ �˻� �� HP ������Ű�� ������Ʈ�� ���� ��ε�ĳ����.
    /// �������� ClientID�� �÷��̾� HP ������Ʈ. 
    /// �������� �����Ǵ� ��ũ��Ʈ.
    /// </summary>
    /// <param name="damage"></param>
    /// <param name="clientId"></param>
    public void PlayerGotHitOnServer(byte damage, PlayerServer player)
    {
        ulong clientId = player.OwnerClientId;
        if (NetworkManager.ConnectedClients.ContainsKey(clientId))
        {
            // ��û�� �÷��̾� ���� HP�� �������� 
            PlayerInGameData playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(clientId);
            sbyte playerHP = playerData.hp;
            sbyte playerMaxHP = playerData.maxHp;

            // HP���� Damage�� Ŭ ���(���ӿ��� ó���� Player���� HP�ܷ� �ľ��ؼ� �˾Ƽ� �Ѵ�.)
            if (playerHP <= damage)
            {
                // HP 0
                playerHP = 0;
            }
            else
            {
                // HP ���� ���
                playerHP -= (sbyte)damage;
            }

            // �� Client UI ������Ʈ ����. HPBar & Damage Popup
            PlayerHPManager.Instance.UpdatePlayerHP(clientId, playerHP, playerMaxHP);
            player.GetComponent<PlayerClient>().ShowDamagePopupClientRPC(damage);

            Debug.Log($"GameMultiplayer.PlayerGotHitOnServer()  Player{clientId} got {damage} damage new HP:{playerHP}");
        }
    }
}
