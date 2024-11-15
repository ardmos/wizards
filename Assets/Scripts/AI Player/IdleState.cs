public class IdleState : AIState
{
    public IdleState(WizardRukeAIServer ai) : base(ai) { }

    public override void Enter()
    {
        Logger.Log("Idle State에 진입합니다.");
    }

    public override void Update()
    {
        Logger.Log("Idle State 업데이트.");
        // 아이들 상태에서는 특별한 동작을 하지 않습니다.
        // 필요한 경우 주변 환경을 감지하거나 다른 상태로 전환할 조건을 체크할 수 있습니다.
    }

    public override void Exit()
    {
        Logger.Log("Idle State에서 벗어납니다.");
    }
}