/// <summary>
/// AI�� ��� ���¸� �����ϴ� Ŭ�����Դϴ�.
/// �� ���¿��� AI�� Ư���� ������ ���� �ʰ� ����մϴ�.
/// </summary>
public class IdleState : AIState
{
    #region Constructor
    /// <summary>
    /// IdleState�� �������Դϴ�.
    /// </summary>
    /// <param name="ai">AI ���� �ν��Ͻ�</param>
    public IdleState(WizardRukeAIServer ai) : base(ai) { }
    #endregion

    #region AIState Override Methods
    /// <summary>
    /// ��� ���¿� ������ �� ȣ��Ǵ� �޼����Դϴ�.
    /// </summary>
    public override void Enter()
    {
        //Logger.Log("Idle State�� �����մϴ�.");
    }

    /// <summary>
    /// ��� ���°� Ȱ��ȭ�Ǿ� �ִ� ���� �ֱ������� ȣ��Ǵ� �޼����Դϴ�.
    /// ���� ��� ���¿����� Ư���� ������ �������� �ʽ��ϴ�.
    /// </summary>
    public override void Update()
    {
        //Logger.Log("Idle State ������Ʈ.");
        // ���̵� ���¿����� Ư���� ������ ���� �ʽ��ϴ�.
    }

    /// <summary>
    /// ��� ���¿��� ���� �� ȣ��Ǵ� �޼����Դϴ�.
    /// </summary>
    public override void Exit()
    {
        //Logger.Log("Idle State���� ����ϴ�.");
    }
    #endregion
}