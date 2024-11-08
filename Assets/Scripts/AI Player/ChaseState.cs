using UnityEngine;

public class ChaseState : AIState
{
    public ChaseState(WizardRukeAIServer ai) : base(ai) { }

    public override void Enter()
    {
        //Debug.Log("Entering Move State");
    }

    public override void Update()
    {
        if (ai.GetTarget() == null) return;

        ai.MoveTowardsTarget();
        
        float targetDistance = Vector3.Distance(ai.transform.position, ai.GetTarget().transform.position);
        if (targetDistance > ai.GetMaxDetectionDistance())
        {
            ai.SetTarget(null);
            ai.GetStateMachine().ChangeState(AIStateType.Patrol);
        }

        if (targetDistance <= ai.GetAttackRange())
        {
            ai.GetStateMachine().ChangeState(AIStateType.Attack);
        }
    }

    public override void Exit()
    {
        //Debug.Log("Exiting Move State");
    }
}
