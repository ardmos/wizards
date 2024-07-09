using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AIState
{
    protected WizardRukeAIServer ai;

    public AIState(WizardRukeAIServer ai)
    {
        this.ai = ai;
    }

    public abstract void Enter();
    public abstract void Update();
    public abstract void Exit();
}

public class IdleState : AIState
{
    public IdleState(WizardRukeAIServer ai) : base(ai) { }

    public override void Enter()
    {
        //Debug.Log("Entering Idle State");
    }

    public override void Update()
    {
        //Debug.Log("IdleState Update");
    }

    public override void Exit()
    {
        //Debug.Log("Exiting Idle State");
    }
}

public class PatrolState : AIState
{
    public PatrolState(WizardRukeAIServer ai) : base(ai) { }

    public override void Enter()
    {
        //Debug.Log("Entering Patrol State");
    }

    public override void Update()
    {
        //Debug.Log("PatrolState Update");
        // »öÀû
        ai.DetectAndSetTarget();

        if (ai.target)
        {
            float targetDistance = Vector3.Distance(ai.transform.position, ai.target.transform.position);

            if ( targetDistance <= ai.maxDistanceDetect) 
            {
                ai.SetState(new ChaseState(ai));
            }    
            if(targetDistance < ai.attackRange)
            {
                ai.SetState(new AttackState(ai));
            }
        }
    }

    public override void Exit()
    {
        //Debug.Log("Exiting Patrol State");
    }
}

public class ChaseState : AIState
{
    public ChaseState(WizardRukeAIServer ai) : base(ai) { }

    public override void Enter()
    {
        //Debug.Log("Entering Move State");
    }

    public override void Update()
    {
        if (ai.target == null) return;
        //Debug.Log("MoveState Update");
        ai.MoveTowardsTarget();
        float targetDistance = Vector3.Distance(ai.transform.position, ai.target.transform.position);
        if (targetDistance > ai.maxDistanceDetect)
        {
            ai.target = null;
            //Debug.Log($"Lost target!");
            ai.SetState(new PatrolState(ai));
        }

        if (targetDistance <= ai.attackRange)
        {
            ai.SetState(new AttackState(ai));
        }
    }

    public override void Exit()
    {
        //Debug.Log("Exiting Move State");
    }
}

public class AttackState : AIState
{
    public AttackState(WizardRukeAIServer ai) : base(ai) { }

    public override void Enter()
    {
        //Debug.Log("Entering Attack State");
    }

    public override void Update()
    {
        if (ai.target == null) return;
        //Debug.Log("AttackState Update");
        ai.MoveTowardsTarget();
        ai.AttackTarget();
        if (Vector3.Distance(ai.transform.position, ai.target.transform.position) > ai.attackRange)
        {
            ai.SetState(new ChaseState(ai));
        }
    }

    public override void Exit()
    {
        //Debug.Log("Exiting Attack State");
    }
}

