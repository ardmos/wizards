using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class WizardRukeAIHPManagerServer : NetworkBehaviour
{
    PlayerInGameData playerData;
    public WizardRukeAIClient wizardRukeAIClient;
    public WizardRukeAIServer wizardRukeAIServer;
    public WizardRukeAIBattleSystemServer wizardRukeAIBattleSystemServer;
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
        //playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(wizardRukeAIServer.AIClientId);
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
        if (wizardRukeAIServer.gameState != PlayerGameState.Playing) return;

        // �ǰ� ���� ȿ�� ���� ClientRPC

        // �ǰ� ����� ���� ǥ�� ����. �� Client Damage Text Popup UI ������Ʈ ���� 
        wizardRukeAIClient.ShowDamageTextPopupClientRPC(damage);

        // ��û�� �÷��̾� ���� HP�� �������� 
        //playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(wizardRukeAIServer.AIClientId);
        sbyte newPlayerHP = playerData.hp;

        // HP���� Damage�� Ŭ ��� GameOver
        if (newPlayerHP <= damage)
        {
            // HP 0
            newPlayerHP = 0;
            playerData.playerGameState = PlayerGameState.GameOver; // �Ʒ����� HP���� ������ �� ���� SetPlayerData�� �ϱ� ������...! �ϴ� ���⼭ �� ���� ���ӿ��� ������Ʈ�� �������ְ� �ִµ�, GameOver()���� �̹� ���ӿ���ó���� ���ְ� �ִ�. �ϴ��� ���������� ���� �ʿ�.

            // ���ӿ��� ó��, GameOver �ִϸ��̼� ����
            GameOver(clientWhoAttacked);
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
        GameMultiplayer.Instance.SetPlayerDataFromClientId(wizardRukeAIServer.AIClientId, playerData);

        // �� Client �÷��̾��� HP�� UI ������Ʈ ClientRPC       
        wizardRukeAIClient.SetHPClientRPC(playerData.hp, playerData.maxHp);

        // �� Client�� ���̴� �ǰ� ����Ʈ ���� ClientRPC
        wizardRukeAIClient.ActivateHitByAttackEffectClientRPC();
    }

    // �ǰ�ó��.
    public void TakingDamageWithCameraShake(sbyte damage, ulong clientIDWhoAttacked, GameObject clientObjectWhoAttacked)
    {
        // �ǰ� ó�� �Ѱ�.
        TakingDamage(damage, clientIDWhoAttacked);

        // �����ڰ� �νĹ��� �ȿ� ������ Ÿ������ ����. 
        if (Vector3.Distance(transform.position, clientObjectWhoAttacked.transform.position) <= wizardRukeAIServer.maxDistanceDetect)
            wizardRukeAIServer.target = clientObjectWhoAttacked;

        // ��ų �ߵ�
        wizardRukeAIBattleSystemServer.Defence();

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

    // ���ӿ��� ó��. �������� ���.
    private void GameOver(ulong clientWhoAttacked)
    {
        // ������ ���ӿ��� ���� ���, ���� �� ��� �÷��̾�鿡�� ������ �ݴϴ�. 
        if (clientWhoAttacked == wizardRukeAIServer.AIClientId)
        {
            foreach (PlayerInGameData playerInGameData in GameMultiplayer.Instance.GetPlayerDataNetworkList())
            {
                GameMultiplayer.Instance.AddPlayerScore(playerInGameData.clientId, 300);
            }
        }
        // �Ϲ����� ��� ��� �÷��̾� 300���ھ� ȹ��
        else
        {
            GameMultiplayer.Instance.AddPlayerScore(clientWhoAttacked, 300);
        }

        // GameOver �ִϸ��̼� ����
        playerAnimator.UpdatePlayerMoveAnimationOnServer(PlayerMoveAnimState.GameOver);
        Debug.Log($"AI ���ӿ���! ");
        // ���� ����
        wizardRukeAIServer.GameOver();

        // ���ӿ��� �÷��̾� ����� ������ ���.
        GameManager.Instance.UpdatePlayerGameOverOnServer(wizardRukeAIServer.AIClientId, clientWhoAttacked);

        // ��ũ�� ������ ���
        DropScrollItem();
    }

    private void DropScrollItem()
    {
        // ���ʷ���Ʈ ������
        GameObject scrollObject = Instantiate(GameAssetsManager.Instance.GetItemScrollObject());

        if (!scrollObject) return;
        
        if(scrollObject.TryGetComponent<NetworkObject>(out NetworkObject networkObject))
        {
            networkObject.Spawn();
            if (GameManager.Instance)
            {
                scrollObject.transform.parent = GameManager.Instance.transform;
                scrollObject.transform.position = transform.position;
            }
        }
    }
}
