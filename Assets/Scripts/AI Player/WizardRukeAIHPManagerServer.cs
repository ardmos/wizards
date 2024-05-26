using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class WizardRukeAIHPManagerServer : NetworkBehaviour
{
    PlayerInGameData playerData;
    public WizardRukeAIClient wizardRukeAIClient;
    public WizardRukeAIServer wizardRukeAIServer;
    public PlayerAnimator playerAnimator;

    public void InitPlayerHP(ICharacter character)
    {
        playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(wizardRukeAIServer.AIClientId);
        playerData.hp = character.hp;
        playerData.maxHp = character.maxHp;
        GameMultiplayer.Instance.SetPlayerDataFromClientId(wizardRukeAIServer.AIClientId, playerData);

        wizardRukeAIClient.SetHPClientRPC(playerData.hp, playerData.maxHp);
    }

    /// <summary>
    /// �������� �����մϴ�
    /// </summary>
    /// <param name="healingValue"></param>
    public void ApplyHeal(sbyte healingValue)
    {
        // �� �����ϴ���
        playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(wizardRukeAIServer.AIClientId);
        sbyte newHP = (sbyte)(playerData.hp + healingValue);
        if (newHP > playerData.maxHp) newHP = playerData.maxHp;

        // ����� HP�� ������ ����
        playerData.hp = newHP;
        GameMultiplayer.Instance.SetPlayerDataFromClientId(wizardRukeAIServer.AIClientId, playerData);

        // �� Client �÷��̾��� HP�� UI ������Ʈ ClientRPC       
        wizardRukeAIClient.SetHPClientRPC(playerData.hp, playerData.maxHp);
    }

    /// <summary>
    /// �������� ȣ���ؾ��ϴ� �޼���. �������� �����մϴ�.
    /// </summary>
    public void TakingDamage(sbyte damage, ulong clientWhoAttacked)
    {
        // GamePlaying���� �ƴϸ� ���� ����. ������ ������ ����ó�� �ǵ���.
        if (!GameManager.Instance.IsGamePlaying()) return;

        // ��û�� �÷��̾� ���� HP�� �������� 
        playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(wizardRukeAIServer.AIClientId);
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
        GameMultiplayer.Instance.SetPlayerDataFromClientId(wizardRukeAIServer.AIClientId, playerData);

        // �� Client �÷��̾��� HP�� UI ������Ʈ ClientRPC       
        wizardRukeAIClient.SetHPClientRPC(playerData.hp, playerData.maxHp);

        // �� Client�� ���̴� �ǰ� ����Ʈ ���� ClientRPC
        wizardRukeAIClient.ActivateHitByAttackEffectClientRPC();

        // �ǰ� �ִϸ��̼� ���� Server
        playerAnimator.UpdatePlayerMoveAnimationOnServer(PlayerMoveAnimState.Hit);

        // �ǰ� ���� ȿ�� ���� ClientRPC
    }

    public sbyte GetHP()
    {
        return playerData.hp;
    }

    // ���ӿ��� ó��. �������� ���.
    private void GameOver(ulong clientWhoAttacked)
    {
        // ų�� �÷��̾� ���ھ� ������Ʈ. ų�� �÷��̾ ������ ���, ���� �� ��� �÷��̾�鿡�� ������ �ݴϴ�. 
        // ų�� �÷��̾ ������ ���(ex, Water�� ���� ���)
        if (clientWhoAttacked == wizardRukeAIServer.AIClientId)
        {
            foreach (PlayerInGameData playerInGameData in GameMultiplayer.Instance.GetPlayerDataNetworkList())
            {
                GameMultiplayer.Instance.AddPlayerScore(playerInGameData.clientId, 300);
            }
        }
        else
        {
            GameMultiplayer.Instance.AddPlayerScore(clientWhoAttacked, 300);
        }

        // ���ӿ��� �÷��̾� ����� ������ ���.
        GameManager.Instance.UpdatePlayerGameOverOnServer(wizardRukeAIServer.AIClientId, clientWhoAttacked);
    }
}