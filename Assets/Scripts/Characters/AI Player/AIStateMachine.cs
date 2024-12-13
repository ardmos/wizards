using System.Collections.Generic;

/// <summary>
/// AI�� ���� ��踦 �����ϴ� Ŭ�����Դϴ�.
/// �پ��� AI ���¸� �����ϰ� ��ȯ�մϴ�.
/// </summary>
public class AIStateMachine
{
    #region Fields
    private Dictionary<AIStateType, AIState> states = new Dictionary<AIStateType, AIState>();
    private AIState currentState;
    private WizardRukeAIServer ai;
    #endregion

    #region Constructor
    /// <summary>
    /// AIStateMachine�� �������Դϴ�.
    /// </summary>
    /// <param name="ai">AI ���� �ν��Ͻ�</param>
    public AIStateMachine(WizardRukeAIServer ai)
    {
        this.ai = ai;
        InitializeStates();
    }
    #endregion

    #region Initialization
    /// <summary>
    /// �ʱ� ���¸� �����ϰ� �����մϴ�.
    /// </summary>
    /// <param name="initialState">�ʱ� AI ����</param>
    public void InitializeStateMachine(AIStateType initialState)
    {
        ChangeState(initialState);
    }

    /// <summary>
    /// ���µ��� �ʱ�ȭ�ϰ� ��ųʸ��� �����մϴ�.
    /// ������ �÷��� �۾��� �ּ�ȭ�ϱ� ���� �̸� ��� ���¸� �����մϴ�.
    /// </summary>
    private void InitializeStates()
    {
        states[AIStateType.Idle] = new IdleState(ai);
        states[AIStateType.Patrol] = new PatrolState(ai);
        states[AIStateType.Chase] = new ChaseState(ai);
        states[AIStateType.Attack] = new AttackState(ai);
    }
    #endregion

    #region State Management
    /// <summary>
    /// ���� ���¸� ���ο� ���·� �����մϴ�.
    /// </summary>
    /// <param name="newStateType">���ο� AI ����</param>
    public void ChangeState(AIStateType newStateType)
    {
        if (currentState != null)
        {
            Logger.Log("�̹� AIStateMachine�� ������ State�� �ֽ��ϴ�. �ش� State�� �����Ų �� ���ο� State�� �����մϴ�.");
            currentState.Exit();
        }

        currentState = states[newStateType];
        currentState.Enter();
    }

    /// <summary>
    /// ���� ������ Update �޼��带 ȣ���մϴ�.
    /// </summary>
    public void UpdateState()
    {
        currentState?.Update();
    }
    #endregion
}