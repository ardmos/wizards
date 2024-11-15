using UnityEngine;

public class PatrolState : AIState
{
    public PatrolState(WizardRukeAIServer ai) : base(ai) { }

    public override void Enter()
    {
        Logger.Log("Patrol State에 진입합니다.");

        InitState();
    }

    public override void Update()
    {
        Logger.Log("Patrol State 업데이트.");

        Patrol();
    }

    public override void Exit()
    {
        Logger.Log("Patrol State에서 벗어납니다.");
    }

    private void InitState()
    {
        if (!ValidateComponent(ai, "AI 설정이 안되어있습니다.")) return;

        ai.SetTarget(null);
    }

    private void Patrol()
    {
        if (!ValidateAIComponents()) return;

        CycleThroughPatrolPoints();
        if (TryDetectAndSetTarget())
        {
            EvaluateTargetDistance();
        }
    }

    private bool ValidateAIComponents()
    {
        return ValidateComponent(ai, "AI 설정이 안되어있습니다.") &&
           ValidateComponent(ai.GetMovementManager(), "AI MovementManager 획득에 실패했습니다.") &&
           ValidateComponent(ai.GetTargetingSystem(), "AI TargetingSystem 획득에 실패했습니다.") &&
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

    private void CycleThroughPatrolPoints() => ai.GetMovementManager().Patrol();

    private bool TryDetectAndSetTarget()
    {
        GameObject detectedTarget = DetectTarget();
        return SetTargetIfDetected(detectedTarget);
    }

    private GameObject DetectTarget() => ai.GetTargetingSystem().DetectTarget<ITargetable>();

    private bool SetTargetIfDetected(GameObject detectedTarget)
    {
        if (detectedTarget != null)
        {
            ai.SetTarget(detectedTarget);
            return true;
        }
        return false;
    }

    private void EvaluateTargetDistance()
    {
        float targetDistance = GetTargetDistance();
        if (IsTargetInAttackRange(targetDistance))
        {
            HandleTargetInAttackRange();
        }
        else if (IsTargetInDetectionRange(targetDistance))
        {
            HandleTargetInDetectionRange();
        }
    }

    private float GetTargetDistance() => Vector3.Distance(ai.transform.position, ai.GetTarget().transform.position);
    private bool IsTargetInAttackRange(float targetDistance) => targetDistance <= ai.GetBattleManager().GetAttackRange();
    private bool IsTargetInDetectionRange(float targetDistance) => targetDistance <= ai.GetMaxDetectionDistance();
    private void HandleTargetInAttackRange() => ai.GetStateMachine().ChangeState(AIStateType.Attack);
    private void HandleTargetInDetectionRange() => ai.GetStateMachine().ChangeState(AIStateType.Chase);
}
