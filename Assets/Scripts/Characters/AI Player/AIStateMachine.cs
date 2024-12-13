using System.Collections.Generic;

/// <summary>
/// AI의 상태 기계를 관리하는 클래스입니다.
/// 다양한 AI 상태를 관리하고 전환합니다.
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
    /// AIStateMachine의 생성자입니다.
    /// </summary>
    /// <param name="ai">AI 서버 인스턴스</param>
    public AIStateMachine(WizardRukeAIServer ai)
    {
        this.ai = ai;
        InitializeStates();
    }
    #endregion

    #region Initialization
    /// <summary>
    /// 초기 상태를 설정하고 진입합니다.
    /// </summary>
    /// <param name="initialState">초기 AI 상태</param>
    public void InitializeStateMachine(AIStateType initialState)
    {
        ChangeState(initialState);
    }

    /// <summary>
    /// 상태들을 초기화하고 딕셔너리에 저장합니다.
    /// 가비지 컬렉션 작업을 최소화하기 위해 미리 모든 상태를 생성합니다.
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
    /// 현재 상태를 새로운 상태로 변경합니다.
    /// </summary>
    /// <param name="newStateType">새로운 AI 상태</param>
    public void ChangeState(AIStateType newStateType)
    {
        if (currentState != null)
        {
            Logger.Log("이미 AIStateMachine에 설정된 State가 있습니다. 해당 State를 종료시킨 후 새로운 State를 설정합니다.");
            currentState.Exit();
        }

        currentState = states[newStateType];
        currentState.Enter();
    }

    /// <summary>
    /// 현재 상태의 Update 메서드를 호출합니다.
    /// </summary>
    public void UpdateState()
    {
        currentState?.Update();
    }
    #endregion
}