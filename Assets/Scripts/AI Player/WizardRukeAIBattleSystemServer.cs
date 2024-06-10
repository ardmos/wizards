using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WizardRukeAIBattleSystemServer : MonoBehaviour
{
    public WizardRukeAIServer wizardRukeAIServer;
    public WizardRukeAISpellManagerServer wizardRukeAISpellManager;
    public float targetDistance;

    // Ÿ�ٰ� �Ÿ��� ���� �켱������ ���ݽ�ų �߻�
    public void Attack()
    {
        if (!wizardRukeAIServer.target) return;

        // Ÿ�ٰ��� �Ÿ� Ȯ��
        targetDistance = Vector3.Distance(transform.position, wizardRukeAIServer.target.transform.position);

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
        transform.LookAt(wizardRukeAIServer.target.transform);
        // ��ų �غ�
        wizardRukeAISpellManager.CastBlizzard();
        // ��ų �߻�
        wizardRukeAISpellManager.FireBlizzard();
    }

    private void FireFireBall()
    {
        //Debug.Log("FireFireBall");
        // Ÿ���� �ٶ󺸱�
        transform.LookAt(wizardRukeAIServer.target.transform);
        // ��ų �غ�
        wizardRukeAISpellManager.CastSpell(0);
        // ��ų �߻�
        wizardRukeAISpellManager.FireSpell(0);
    }

    private void FireWaterBall()
    {
        //Debug.Log("FireWaterBall");
        // Ÿ���� �ٶ󺸱�
        transform.LookAt(wizardRukeAIServer.target.transform);
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
}
