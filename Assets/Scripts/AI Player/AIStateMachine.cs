using System.Collections.Generic;

public enum AIStateType
{
    Idle,
    Patrol,
    Chase,
    Attack
}

public class AIStateMachine
{
    private Dictionary<AIStateType, AIState> states = new Dictionary<AIStateType, AIState>(); 
    private AIState currentState;
    private WizardRukeAIServer ai;

    public AIStateMachine(WizardRukeAIServer ai)
    {
        this.ai = ai;
        // GC �۾��� �ּ�ȭ�� ���� ĳ��
        states[AIStateType.Idle] = new IdleState(ai);
        states[AIStateType.Patrol] = new PatrolState(ai);
        states[AIStateType.Chase] = new ChaseState(ai);
        states[AIStateType.Attack] = new AttackState(ai);
    }

    public void Initialize(AIStateType initialState)
    {
        currentState = states[initialState];
        currentState.Enter();
    }

    public void ChangeState(AIStateType newStateType)
    {
        if (currentState != null)
            currentState.Exit();

        currentState = states[newStateType];
        currentState.Enter();
    }

    public void UpdateState()
    {
        currentState?.Update();
    }
}