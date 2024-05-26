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
    public Rigidbody rb;

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
        Debug.Log($"OwnerClientId{OwnerClientId} Player (class : {character.characterClass.ToString()}) InitializeAIPlayerOnServer");

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

        // �÷��̾� Layer ����
        switch (OwnerClientId)
        {
            case 0:
                gameObject.layer = LayerMask.NameToLayer("Player0");
                break;
            case 1:
                gameObject.layer = LayerMask.NameToLayer("Player1");
                break;
            case 2:
                gameObject.layer = LayerMask.NameToLayer("Player2");
                break;
            case 3:
                gameObject.layer = LayerMask.NameToLayer("Player3");
                break;
            default: 
                break;
        }
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
    public void PlayerGotHitOnServer(sbyte damage, ulong clientWhoAttacked)
    {
        // �ǰ� ó�� �Ѱ�.
        playerHPManager.TakingDamage(damage, clientWhoAttacked);
        // �� Client UI ������Ʈ ���� Damage Text Popup. ���� HPManager.TakingDamage���� ���ݴϴ�.
        //playerClient.ShowDamageTextPopupClientRPC(damage);
        // ���� �÷��̾� ī�޶� ����ũ
        NetworkClient networkClient = NetworkManager.ConnectedClients[clientWhoAttacked];
        networkClient.PlayerObject.GetComponent<PlayerClient>().ActivateHitCameraShakeClientRPC();
    }

    public sbyte GetPlayerHP()
    {
        return playerHPManager.GetHP();
    }
}
