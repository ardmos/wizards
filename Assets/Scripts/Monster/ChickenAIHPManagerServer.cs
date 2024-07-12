using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ChickenAIHPManagerServer : NetworkBehaviour
{
    public ChickenAIServer chickenAIServer;
    public ChickenAIClient chickenAIClient;
    public ChickenAIHPManagerClient chickenAIHPManagerClient;

    public sbyte hp;
    private sbyte maxHp;

    public void Start()
    {
        InitHP();
    }

    private void InitHP()
    {
        hp = 3;
        maxHp = 3;
    }

    public void TakingDamage(sbyte damage)
    {
        sbyte newPlayerHP = hp;

        // HP���� Damage�� Ŭ ��� GameOver
        if (newPlayerHP <= damage)
        {
            // HP 0
            newPlayerHP = 0;

            // ���ӿ��� ó��, GameOver �ִϸ��̼� ����
            chickenAIServer.GameOver();
        }
        else
        {
            // HP ���� ���
            newPlayerHP -= (sbyte)damage;
        }

        // ����� HP�� ������ ����
        hp = newPlayerHP;

        // �ǰ� ����� ���� ǥ�� ����. ClientRPC
        chickenAIClient.ShowDamageTextPopupClientRPC(damage);
        // HP�� UI ������Ʈ ClientRPC       
        chickenAIHPManagerClient.SetHPClientRPC(hp, maxHp);
        // ���̴� �ǰ� ����Ʈ ���� ClientRPC
        chickenAIClient.ActivateHitByAttackEffectClientRPC();
    }

    /// <summary>
    /// ������ �÷��̾��� ȭ�� ����ũ ����� �߰��� �޼���.
    /// </summary>
    /// <param name="damage"></param>
    /// <param name="clientObjectWhoAttacked"></param>
    public void TakingDamageWithCameraShake(sbyte damage, GameObject clientObjectWhoAttacked)
    {
        Debug.Log($"{gameObject}.TakingDamageWithCameraShake! damage:{damage}, clientWhoAttacked:{clientObjectWhoAttacked}");

        TakingDamage(damage);

        // �����ڰ� Player��� ī�޶� ����ũ 
        if (clientObjectWhoAttacked.TryGetComponent<PlayerClient>(out PlayerClient playerClient))
        {
            playerClient.ActivateHitCameraShakeClientRPC();
        }
    }

    // ���̾ ��Ʈ ������� �޴� Coroutine
    public IEnumerator TakeDamageOverTime(sbyte damagePerSecond, float duration)
    {
        float elapsed = 0;
        while (elapsed < duration)
        {
            // 1�� ���
            yield return new WaitForSeconds(1);

            TakingDamage(damagePerSecond);

            elapsed += 1;
        }
    }
}
