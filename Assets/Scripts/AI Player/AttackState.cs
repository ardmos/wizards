using UnityEngine;

public class AttackState : AIState
{
    public AttackState(WizardRukeAIServer ai) : base(ai) { }

    public override void Enter()
    {
        Logger.Log("Attack State에 진입합니다.");
    }

    public override void Update()
    {
        Logger.Log("Attack State 업데이트.");

        Attack();
    }

    public override void Exit()
    {
        Logger.Log("Attack State에서 벗어납니다.");
    }

    private void Attack()
    {
        if (!ValidateAIComponents()) return;

        KeepMoving();
        if (IsTargetInAttackRange()) AttackTarget();
        else ChaseTarget();
    }

    private bool ValidateAIComponents()
    {
        return ValidateComponent(ai, "AI 설정이 안되어있습니다.") &&
           ValidateComponent(ai.GetTarget(), "AI Target 획득에 실패했습니다.") &&
           ValidateComponent(ai.GetBattleManager(), "AI BattleManager 획득에 실패했습니다.") &&
           ValidateComponent(ai.GetStateMachine(), "AI StateMachine 획득에 실패했습니다.");
    }

    private bool ValidateComponent<T>(T component, string errorMessage)
    {
        if (component == null)
        {
            Logger.LogError(errorMessage);
            return false;
        }
        return true;
    }

    private void KeepMoving() => ai.MoveTowardsTarget();
    private bool IsTargetInAttackRange() => Vector3.Distance(ai.transform.position, ai.GetTarget().transform.position) <= ai.GetBattleManager().GetAttackRange();
    private void AttackTarget() => ai.AttackTarget();
    private void ChaseTarget() => ai.GetStateMachine().ChangeState(AIStateType.Chase);
}
