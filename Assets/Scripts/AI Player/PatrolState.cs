using UnityEngine;

public class PatrolState : AIState
{
    public PatrolState(WizardRukeAIServer ai) : base(ai) { }

    public override void Enter()
    {
        //Debug.Log("Entering Patrol State");
    }

    public override void Update()
    {
        Patrol();

        if (TryDetectAndSetTarget())
        {
            EvaluateTargetDistance();
        }
    }

    public override void Exit()
    {
        //Debug.Log("Exiting Patrol State");
    }

    private void Patrol()
    {
        ai.GetMovementSystem().Patrol();
    }

    private bool TryDetectAndSetTarget()
    {
        GameObject detectedTarget = ai.GetTargetingSystem().DetectTarget();
        if (detectedTarget != null)
        {
            ai.SetTarget(detectedTarget);
            return true;
        }
        return false;
    }

    private void EvaluateTargetDistance()
    {
        float targetDistance = Vector3.Distance(ai.transform.position, ai.GetTarget().transform.position);

        if (targetDistance <= ai.GetAttackRange())
        {
            ai.GetStateMachine().ChangeState(AIStateType.Attack);
        }
        else if (targetDistance <= ai.GetMaxDetectionDistance())
        {
            ai.GetStateMachine().ChangeState(AIStateType.Chase);
        }
    }
}
