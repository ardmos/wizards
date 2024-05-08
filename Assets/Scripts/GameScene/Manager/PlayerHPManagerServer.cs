using System.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using UnityEngine;
/// <summary>
/// Player HP��  Server Auth ������� �����Ҽ� �ֵ��� �����ִ� ��ũ��Ʈ �Դϴ�.
/// �������� �����մϴ�.
/// �� �÷��̾�� ĳ���� ������Ʈ�� ������Ʈ�� �ٿ��� ����մϴ�
/// </summary>
public class PlayerHPManagerServer : NetworkBehaviour
{
    PlayerInGameData playerData;
    public PlayerClient playerClient;

    public void InitPlayerHP(ICharacter character)
    {
        playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(OwnerClientId);
        playerData.hp = character.hp; 
        playerData.maxHp = character.maxHp;
        GameMultiplayer.Instance.SetPlayerDataFromClientId(OwnerClientId, playerData);

        playerClient.SetHPClientRPC(playerData.hp, playerData.maxHp);
    }

    public void ApplyHeal(sbyte healingValue)
    {
        /// �� �����ϴ���
        playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(OwnerClientId);
        sbyte newHP = (sbyte)(playerData.hp + healingValue);
        if (newHP > playerData.maxHp) newHP = playerData.maxHp;   
    }

    /// <summary>
    /// �������� ȣ���ؾ��ϴ� �޼���. �������� �����մϴ�.
    /// </summary>
    public void TakingDamage(sbyte damage, ulong clientWhoAttacked)
    {
        // ��û�� �÷��̾� ���� HP�� �������� 
        playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(OwnerClientId);
        sbyte newPlayerHP = playerData.hp;

        if (newPlayerHP == 0) return;

        // HP���� Damage�� Ŭ ���(���ӿ��� ó���� Player���� HP�ܷ� �ľ��ؼ� �˾Ƽ� �Ѵ�.)
        if (newPlayerHP <= damage)
        {
            // HP 0
            newPlayerHP = 0;
            playerData.playerGameState = PlayerGameState.GameOver; // �Ʒ����� HP���� ������ �� ���� SetPlayerData�� �ϱ� ������...! �ϴ� ���⼭ �� ���� ���ӿ��� ������Ʈ�� �������ְ� �ִµ�, GameOver()���� �̹� ���ӿ���ó���� ���ְ� �ִ�. �ϴ��� ���������� ���� �ʿ�.

            // ���ӿ��� ó��
            GameOver(clientWhoAttacked);
        }
        else
        {
            // HP ���� ���
            newPlayerHP -= (sbyte)damage;
        }

        // ����� HP�� ������ ����
        playerData.hp = newPlayerHP;
        GameMultiplayer.Instance.SetPlayerDataFromClientId(OwnerClientId, playerData);

        // �� Client �÷��̾��� HP�� UI ������Ʈ ClientRPC
        /*NetworkClient networkClient = NetworkManager.ConnectedClients[OwnerClientId];
        networkClient.PlayerObject.GetComponent<PlayerClient>().SetHPClientRPC(playerData.hp, playerData.maxHp);*/
        playerClient.SetHPClientRPC(playerData.hp, playerData.maxHp);

        // �� Client�� ȭ�鿡�� ���̴� �ǰ� ����Ʈ ���� ClientRPC
        playerClient.ActivateHitEffectClientRPC();

        // �ǰ� �ִϸ��̼� ���� Server

        // �ǰ� ī�޶� ȿ�� ���� ClientRPC
        playerClient.ActivateHitCameraEffectClientRPC();

        // �ǰ� ���� ȿ�� ���� ClientRPC

    }



    // ���ӿ��� ó��. �������� ���.
    private void GameOver(ulong clientWhoAttacked)
    {
        // ų�� �÷��̾� ���ھ� ������Ʈ. ų�� �÷��̾ ������ ���, ���� �� ��� �÷��̾�鿡�� ������ �ݴϴ�. 
        // ų�� �÷��̾ ������ ���(ex, Water�� ���� ���)
        if(clientWhoAttacked == OwnerClientId)
        {
            foreach(PlayerInGameData playerInGameData in GameMultiplayer.Instance.GetPlayerDataNetworkList()){
                GameMultiplayer.Instance.AddPlayerScore(playerInGameData.clientId, 300);
            }
        }
        else
        {
            GameMultiplayer.Instance.AddPlayerScore(clientWhoAttacked, 300);
        }
        

        // ���ӿ��� �÷��̾� ����� ������ ���.
        GameManager.Instance.UpdatePlayerGameOverOnServer(OwnerClientId, clientWhoAttacked);

        // GameOver �÷��̾� AnimState GameOver ���·� ������ ���. ���ӿ��� �ִϸ��̼� ����. 
        GameMultiplayer.Instance.UpdatePlayerMoveAnimStateOnServer(OwnerClientId, PlayerMoveAnimState.GameOver);

        // �ش� �÷��̾� ���� �Ұ� ó�� �� ���ӿ��� �˾� ����.
        playerClient.SetPlayerGameOverClientRPC();
    }
}