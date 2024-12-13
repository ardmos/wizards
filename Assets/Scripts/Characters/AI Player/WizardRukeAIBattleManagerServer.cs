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

    // 타겟과 거리에 따른 우선순위로 공격스킬 발사
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

        // 타겟과의 거리 확인
        targetDistance = Vector3.Distance(transform.position, wizardRukeAIServer.GetTarget().transform.position);

        //3 이하 블리자드
        if (targetDistance <= 3)
            FireBlizzard();
        //5 이하 파이어볼
        else if (targetDistance <= 5)
            FireFireBall();
        //8 이하 워터볼
        else if (targetDistance <= 8)
            FireWaterBall();
    }

    private void FireBlizzard()
    {
        //Debug.Log("FireBlizzard");
        // 타겟쪽 바라보기
        transform.LookAt(wizardRukeAIServer.GetTarget().transform);
        // 스킬 준비
        wizardRukeAISpellManager.CastBlizzard();
        // 스킬 발사
        wizardRukeAISpellManager.FireBlizzard();
    }

    private void FireFireBall()
    {
        //Debug.Log("FireFireBall");
        // 타겟쪽 바라보기
        transform.LookAt(wizardRukeAIServer.GetTarget().transform);
        // 스킬 준비
        wizardRukeAISpellManager.CastSpell(0);
        // 스킬 발사
        wizardRukeAISpellManager.FireSpell(0);
    }

    private void FireWaterBall()
    {
        //Debug.Log("FireWaterBall");
        // 타겟쪽 바라보기
        transform.LookAt(wizardRukeAIServer.GetTarget().transform);
        // 스킬 준비
        wizardRukeAISpellManager.CastSpell(1);
        // 스킬 발사
        wizardRukeAISpellManager.FireSpell(1);
    }


    // 피격시 방어스킬 발동. 
    public void Defence()
    {
        ActivateMagicShield();
    }

    // 적대스킬 감지 기능은 추후에 추가. 현재는 피격시 발동
/*    private void DetectEnemySkill()
    {

    }*/

    private void ActivateMagicShield()
    {
        wizardRukeAISpellManager.StartActivateDefenceSpell();
    }

    public float GetAttackRange() => ATTACK_RANGE;
}
