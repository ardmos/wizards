using UnityEngine;

public class AttackState : AIState
{
    public AttackState(WizardRukeAIServer ai) : base(ai) { }

    public override void Enter()
    {
        //Debug.Log("Entering Attack State");
    }

    public override void Update()
    {
        if (ai.GetTarget() == null) return;

        ai.MoveTowardsTarget();
        ai.AttackTarget();
        if (Vector3.Distance(ai.transform.position, ai.GetTarget().transform.position) > ai.GetBattleManager().GetAttackRange())
        {
            ai.GetStateMachine().ChangeState(AIStateType.Chase);
        }
    }

    public override void Exit()
    {
        //Debug.Log("Exiting Attack State");
    }
}
