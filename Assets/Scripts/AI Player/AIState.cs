/// <summary>
/// AI 상태의 기본 추상 클래스입니다.
/// 모든 구체적인 AI 상태 클래스는 이 클래스를 상속받아야 합니다.
/// </summary>
public abstract class AIState
{
    protected WizardRukeAIServer ai;

    /// <summary>
    /// AIState의 생성자입니다.
    /// </summary>
    /// <param name="ai">AI 서버 인스턴스</param>
    protected AIState(WizardRukeAIServer ai)
    {
        this.ai = ai;
    }

    /// <summary>
    /// 상태에 진입할 때 호출되는 메서드입니다.
    /// </summary>
    public abstract void Enter();

    /// <summary>
    /// 상태가 활성화되어 있는 동안 주기적으로 호출되는 메서드입니다.
    /// </summary>
    public abstract void Update();

    /// <summary>
    /// 상태에서 나갈 때 호출되는 메서드입니다.
    /// </summary>
    public abstract void Exit();
}