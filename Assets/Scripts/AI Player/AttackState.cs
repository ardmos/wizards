using UnityEngine;

public class AttackState : AIState
{
    public AttackState(WizardRukeAIServer ai) : base(ai) { }

    public override void Enter()
    {
        Logger.Log("Attack State�� �����մϴ�.");
    }

    public override void Update()
    {
        Logger.Log("Attack State ������Ʈ.");

        Attack();
    }

    public override void Exit()
    {
        Logger.Log("Attack State���� ����ϴ�.");
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
        return ValidateComponent(ai, "AI ������ �ȵǾ��ֽ��ϴ�.") &&
           ValidateComponent(ai.GetTarget(), "AI Target ȹ�濡 �����߽��ϴ�.") &&
           ValidateComponent(ai.GetBattleManager(), "AI BattleManager ȹ�濡 �����߽��ϴ�.") &&
           ValidateComponent(ai.GetStateMachine(), "AI StateMachine ȹ�濡 �����߽��ϴ�.");
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
