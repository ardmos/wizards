using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class WizardRukeAIHPManagerServer : NetworkBehaviour
{
    PlayerInGameData playerData;
    public WizardRukeAIClient wizardRukeAIClient;
    public WizardRukeAIServer wizardRukeAIServer;
    public WizardRukeAIBattleManagerServer wizardRukeAIBattleSystemServer;
    public PlayerAnimator playerAnimator;

    public void InitPlayerHP(ICharacter character)
    {
        playerData = CurrentPlayerDataManager.Instance.GetPlayerDataByClientId(wizardRukeAIServer.GetClientID());
        playerData.hp = character.hp;
        playerData.maxHp = character.maxHp;
        CurrentPlayerDataManager.Instance.SetPlayerDataByClientId(wizardRukeAIServer.GetClientID(), playerData);

        wizardRukeAIClient.UpdateHPBarUIClientRPC(playerData.hp, playerData.maxHp);
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
        CurrentPlayerDataManager.Instance.SetPlayerDataByClientId(wizardRukeAIServer.GetClientID(), playerData);

        // �� Client �÷��̾��� HP�� UI ������Ʈ ClientRPC       
        wizardRukeAIClient.UpdateHPBarUIClientRPC(playerData.hp, playerData.maxHp);
    }

    /// <summary>
    /// �������� ȣ���ؾ��ϴ� �޼���. �������� �����մϴ�.
    /// </summary>
    public void TakingDamage(sbyte damage, ulong attackerClientID)
    {
        // GamePlaying���� �ƴϸ� ���� ����. ������ ������ ����ó�� �ǵ���.
        if (!MultiplayerGameManager.Instance.IsGamePlaying()) return;
        if (wizardRukeAIServer.GetAIGameState() != PlayerGameState.Playing) return;

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

            // ���ӿ��� ó��
            wizardRukeAIServer.GameOver(attackerClientID);
        }
        else
        {
            // HP ���� ���
            newPlayerHP -= (sbyte)damage;

            // �ǰ� �ִϸ��̼� ���� Server
            playerAnimator.UpdatePlayerAnimationOnServer(PlayerMoveAnimState.Hit);
        }

        // ����� HP�� ������ ����
        playerData.hp = newPlayerHP;
        CurrentPlayerDataManager.Instance.SetPlayerDataByClientId(wizardRukeAIServer.GetClientID(), playerData);

        // �� Client �÷��̾��� HP�� UI ������Ʈ ClientRPC       
        wizardRukeAIClient.UpdateHPBarUIClientRPC(playerData.hp, playerData.maxHp);

        // �� Client�� ���̴� �ǰ� ����Ʈ ���� ClientRPC
        wizardRukeAIClient.ActivateHitByAttackEffectClientRPC();
    }

    // �ǰ�ó��.
    public void TakingDamageWithCameraShake(sbyte damage, ulong attackerClientID, GameObject attackerClientObject)
    {
        // �ǰ� ó�� �Ѱ�.
        TakingDamage(damage, attackerClientID);

        // �����ڰ� �νĹ��� �ȿ� ������ Ÿ������ ����. 
        if (Vector3.Distance(transform.position, attackerClientObject.transform.position) <= wizardRukeAIServer.GetMaxDetectionDistance())
            wizardRukeAIServer.SetTarget(attackerClientObject);

        // ��ų �ߵ�
        wizardRukeAIBattleSystemServer.Defence();

        // �����ڰ� Player��� ī�޶� ����ũ 
        if (attackerClientObject.TryGetComponent<PlayerClient>(out PlayerClient playerClient))
        {
            playerClient.ActivateHitCameraShakeClientRPC();
        }
    }

    public void StartToTakeDotDamage(sbyte damagePerSecond, float duration, ulong attackerClientId)
    {
        StartCoroutine(TakeDamageOverTime(damagePerSecond, duration, attackerClientId));
    }

    // ��Ʈ ������� �޴� Coroutine (ex. Fire Type ������ ��ȭ)
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