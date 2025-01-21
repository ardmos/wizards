/// <summary>
/// AI의 대기 상태를 관리하는 클래스입니다.
/// 이 상태에서 AI는 특별한 동작을 하지 않고 대기합니다.
/// </summary>
public class IdleState : AIState
{
    #region Constructor
    /// <summary>
    /// IdleState의 생성자입니다.
    /// </summary>
    /// <param name="ai">AI 서버 인스턴스</param>
    public IdleState(WizardRukeAIServer ai) : base(ai) { }
    #endregion

    #region AIState Override Methods
    /// <summary>
    /// 대기 상태에 진입할 때 호출되는 메서드입니다.
    /// </summary>
    public override void Enter()
    {
        //Logger.Log("Idle State에 진입합니다.");
    }

    /// <summary>
    /// 대기 상태가 활성화되어 있는 동안 주기적으로 호출되는 메서드입니다.
    /// 현재 대기 상태에서는 특별한 동작을 수행하지 않습니다.
    /// </summary>
    public override void Update()
    {
        //Logger.Log("Idle State 업데이트.");
        // 아이들 상태에서는 특별한 동작을 하지 않습니다.
    }

    /// <summary>
    /// 대기 상태에서 나갈 때 호출되는 메서드입니다.
    /// </summary>
    public override void Exit()
    {
        //Logger.Log("Idle State에서 벗어납니다.");
    }
    #endregion
}