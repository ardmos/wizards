using UnityEngine;

public class ChaseState : AIState
{
    public ChaseState(WizardRukeAIServer ai) : base(ai) { }

    public override void Enter()
    {
        Logger.Log("Chase State에 진입합니다.");
    }

    public override void Update()
    {
        Logger.Log("Chase State 업데이트.");

        Chase();
    }

    public override void Exit()
    {
        Logger.Log("Chase State에서 벗어납니다.");
    }

    private void Chase()
    {
        if (!ValidateAIComponents()) return;

        ChaseTarget();
        EvaluateTargetDistance();
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
