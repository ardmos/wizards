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
        // ���̵� ���¿����� Ư���� ������ ���� �ʽ��ϴ�.
        // �ʿ��� ��� �ֺ� ȯ���� �����ϰų� �ٸ� ���·� ��ȯ�� ������ üũ�� �� �ֽ��ϴ�.
    }

    public override void Exit()
    {
        //Debug.Log("Exiting Idle State");
    }
}