public class IdleState : AIState
{
    public IdleState(WizardRukeAIServer ai) : base(ai) { }

    public override void Enter()
    {
        Logger.Log("Idle State�� �����մϴ�.");
    }

    public override void Update()
    {
        Logger.Log("Idle State ������Ʈ.");
        // ���̵� ���¿����� Ư���� ������ ���� �ʽ��ϴ�.
        // �ʿ��� ��� �ֺ� ȯ���� �����ϰų� �ٸ� ���·� ��ȯ�� ������ üũ�� �� �ֽ��ϴ�.
    }

    public override void Exit()
    {
        Logger.Log("Idle State���� ����ϴ�.");
    }
}