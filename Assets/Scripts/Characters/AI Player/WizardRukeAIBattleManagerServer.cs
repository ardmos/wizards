using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WizardRukeAIBattleManagerServer : MonoBehaviour
{
    private const float ATTACK_COOLDOWN = 2f;
    private const float ATTACK_RANGE = 8f;

    public WizardRukeAIServer wizardRukeAIServer;
    public WizardRukeAISpellManagerServer wizardRukeAISpellManager;
    public float targetDistance;

    private float lastAttackTime = 0f;

    // Ÿ�ٰ� �Ÿ��� ���� �켱������ ���ݽ�ų �߻�
    public void Attack()
    {
        if (Time.time > lastAttackTime + ATTACK_COOLDOWN)
        {
            FireSpellByTargetDistance();
            lastAttackTime = Time.time;
        }
    }

    private void FireSpellByTargetDistance()
    {
        if (wizardRukeAIServer.GetTarget() == null) return;

        // Ÿ�ٰ��� �Ÿ� Ȯ��
        targetDistance = Vector3.Distance(transform.position, wizardRukeAIServer.GetTarget().transform.position);

        //3 ���� ���ڵ�
        if (targetDistance <= 3)
            FireBlizzard();
        //5 ���� ���̾
        else if (targetDistance <= 5)
            FireFireBall();
        //8 ���� ���ͺ�
        else if (targetDistance <= 8)
            FireWaterBall();
    }

    private void FireBlizzard()
    {
        //Debug.Log("FireBlizzard");
        // Ÿ���� �ٶ󺸱�
        transform.LookAt(wizardRukeAIServer.GetTarget().transform);
        // ��ų �غ�
        wizardRukeAISpellManager.CastBlizzard();
        // ��ų �߻�
        wizardRukeAISpellManager.FireBlizzard();
    }

    private void FireFireBall()
    {
        //Debug.Log("FireFireBall");
        // Ÿ���� �ٶ󺸱�
        transform.LookAt(wizardRukeAIServer.GetTarget().transform);
        // ��ų �غ�
        wizardRukeAISpellManager.CastSpell(0);
        // ��ų �߻�
        wizardRukeAISpellManager.FireSpell(0);
    }

    private void FireWaterBall()
    {
        //Debug.Log("FireWaterBall");
        // Ÿ���� �ٶ󺸱�
        transform.LookAt(wizardRukeAIServer.GetTarget().transform);
        // ��ų �غ�
        wizardRukeAISpellManager.CastSpell(1);
        // ��ų �߻�
        wizardRukeAISpellManager.FireSpell(1);
    }


    // �ǰݽ� ��ų �ߵ�. 
    public void Defence()
    {
        ActivateMagicShield();
    }

    // ���뽺ų ���� ����� ���Ŀ� �߰�. ����� �ǰݽ� �ߵ�
/*    private void DetectEnemySkill()
    {

    }*/

    private void ActivateMagicShield()
    {
        wizardRukeAISpellManager.StartActivateDefenceSpell();
    }

    public float GetAttackRange() => ATTACK_RANGE;
}
