using UnityEngine;

public class ChaseState : AIState
{
    public ChaseState(WizardRukeAIServer ai) : base(ai) { }

    public override void Enter()
    {
        Logger.Log("Chase State�� �����մϴ�.");
    }

    public override void Update()
    {
        Logger.Log("Chase State ������Ʈ.");

        Chase();
    }

    public override void Exit()
    {
        Logger.Log("Chase State���� ����ϴ�.");
    }

    private void Chase()
    {
        if (!ValidateAIComponents()) return;

        ChaseTarget();
        EvaluateTargetDistance();
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

    private void ChaseTarget() => ai.MoveTowardsTarget();

    private void EvaluateTargetDistance()
    {
        float targetDistance = GetTargetDistance();

        if (IsTargetOutOfChaseRange(targetDistance))
        {
            HandleTargetOutOfChaseRange();
        }
        else if (IsTargetInAttackRange(targetDistance))
        {
            HandleTargetInAttackRange();
        }
    }

    private float GetTargetDistance() => Vector3.Distance(ai.transform.position, ai.GetTarget().transform.position);
    private bool IsTargetOutOfChaseRange(float targetDistance) => targetDistance > ai.GetMaxDetectionDistance();
    private bool IsTargetInAttackRange(float targetDistance) => targetDistance <= ai.GetBattleManager().GetAttackRange();
    private void HandleTargetOutOfChaseRange() => ai.GetStateMachine().ChangeState(AIStateType.Patrol);
    private void HandleTargetInAttackRange() => ai.GetStateMachine().ChangeState(AIStateType.Attack);
}
