using System.Collections;
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
    public PlayerServer playerServer;
    public PlayerAnimator playerAnimator;

    public void InitPlayerHP(ICharacter character)
    {
        if (IsHost)
        {
            playerData = GameSingleplayer.Instance.GetPlayerDataFromClientId(OwnerClientId);
            playerData.hp = character.hp;
            playerData.maxHp = character.maxHp;
            GameSingleplayer.Instance.SetPlayerDataFromClientId(OwnerClientId, playerData);

            playerClient.UpdateHPBarClientRPC(playerData.hp, playerData.maxHp);
        }
        else
        {
            playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(OwnerClientId);
            playerData.hp = character.hp;
            playerData.maxHp = character.maxHp;
            GameMultiplayer.Instance.SetPlayerDataFromClientId(OwnerClientId, playerData);

            playerClient.UpdateHPBarClientRPC(playerData.hp, playerData.maxHp);
        }
    }

    /// <summary>
    /// �������� �����մϴ�
    /// </summary>
    /// <param name="healingValue"></param>
    public void ApplyHeal(sbyte healingValue)
    {
        // �� �����ϴ���
        //playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(OwnerClientId);
        sbyte newHP = (sbyte)(playerData.hp + healingValue);
        if (newHP > playerData.maxHp) newHP = playerData.maxHp;

        // ����� HP�� ������ ����
        playerData.hp = newHP;
        if (IsHost)
        {
            GameSingleplayer.Instance.SetPlayerDataFromClientId(OwnerClientId, playerData);
        }
        else
        {
            GameMultiplayer.Instance.SetPlayerDataFromClientId(OwnerClientId, playerData);
        }

        // �� Client �÷��̾��� HP�� UI ������Ʈ ClientRPC       
        playerClient.UpdateHPBarClientRPC(playerData.hp, playerData.maxHp);
    }

    /// <summary>
    /// �������� ȣ���ؾ��ϴ� �޼���. �������� �����մϴ�.
    /// </summary>
    public void TakingDamage(sbyte damage, ulong clientWhoAttacked)
    {
        // GamePlaying���� �ƴϸ� ���� ����. ������ ������ ����ó�� �ǵ���.
        if(IsHost)
        {
            if (!SingleplayerGameManager.Instance.IsGamePlaying()) return;
        }
        else
        {
            if (!MultiplayerGameManager.Instance.IsGamePlaying()) return;
        }
        
        //playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(OwnerClientId);
        if (playerData.playerGameState != PlayerGameState.Playing) return;

        // ��û�� �÷��̾� ���� HP�� �������� 
        sbyte newPlayerHP = playerData.hp;

        // HP���� Damage�� Ŭ ���(���ӿ��� ó���� Player���� HP�ܷ� �ľ��ؼ� �˾Ƽ� �Ѵ�.)
        if (newPlayerHP <= damage)
        {
            // HP 0
            newPlayerHP = 0;
            playerData.playerGameState = PlayerGameState.GameOver; // �Ʒ����� HP���� ������ �� ���� SetPlayerData�� �ϱ� ������...! �ϴ� ���⼭ �� ���� ���ӿ��� ������Ʈ�� �������ְ� �ִµ�, GameOver()���� �̹� ���ӿ���ó���� ���ְ� �ִ�. �ϴ��� ���������� ���� �ʿ�.

            // ���ӿ��� ó��
            playerServer.GameOver(clientWhoAttacked);
        }
        else
        {
            // HP ���� ���
            newPlayerHP -= (sbyte)damage;

            // �ǰ� �ִϸ��̼� ���� Server
            playerAnimator.UpdatePlayerMoveAnimationOnServer(PlayerMoveAnimState.Hit);
        }

        // ����� HP�� ������ ����
        playerData.hp = newPlayerHP;

        if (IsHost)
        {
            GameSingleplayer.Instance.SetPlayerDataFromClientId(OwnerClientId, playerData);
        }
        else
        {
            GameMultiplayer.Instance.SetPlayerDataFromClientId(OwnerClientId, playerData);
        }

        // �ǰ� ī�޶� ȿ�� ���� ClientRPC
        playerClient.ActivateHitByAttackCameraEffectClientRPC();

        // �ǰ� ī�޶� ����ũ ȿ�� ���� ClientRPC
        playerClient.ActivateHitByAttackCameraShakeClientRPC();

        // �ǰ� ���� ȿ�� ���� ClientRPC

        // �ǰ� ����� ���� ǥ�� ����. �� Client Damage Text Popup UI ������Ʈ ���� 
        playerClient.ShowDamageTextPopupClientRPC(damage);

        // �� Client �÷��̾��� HP�� UI ������Ʈ ClientRPC       
        playerClient.UpdateHPBarClientRPC(playerData.hp, playerData.maxHp);

        // �� Client�� ���̴� �ǰ� ����Ʈ ���� ClientRPC
        playerClient.ActivateHitByAttackEffectClientRPC();
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
    public void TakingDamageWithCameraShake(sbyte damage, ulong clientWhoAttacked, GameObject clientObjectWhoAttacked)
    {
        // �ǰ� ó�� �Ѱ�.
        TakingDamage(damage, clientWhoAttacked);

        // �����ڰ� Player��� ī�޶� ����ũ 
        if (clientObjectWhoAttacked.TryGetComponent<PlayerClient>(out PlayerClient playerClient))
        {
            playerClient.ActivateHitCameraShakeClientRPC();
        }
    }

    // ���̾ ��Ʈ ������� �޴� Coroutine
    public IEnumerator TakeDamageOverTime(sbyte damagePerSecond, float duration, ulong clientWhoAttacked)
    {
        float elapsed = 0;
        while (elapsed < duration)
        {
            // 1�� ���
            yield return new WaitForSeconds(1);

            TakingDamage(damagePerSecond, clientWhoAttacked);

            elapsed += 1;
        }
    }

    public sbyte GetHP()
    {
        return playerData.hp;
    }
}