/// <summary>
/// AI ������ �⺻ �߻� Ŭ�����Դϴ�.
/// ��� ��ü���� AI ���� Ŭ������ �� Ŭ������ ��ӹ޾ƾ� �մϴ�.
/// </summary>
public abstract class AIState
{
    protected WizardRukeAIServer ai;

    /// <summary>
    /// AIState�� �������Դϴ�.
    /// </summary>
    /// <param name="ai">AI ���� �ν��Ͻ�</param>
    protected AIState(WizardRukeAIServer ai)
    {
        this.ai = ai;
    }

    /// <summary>
    /// ���¿� ������ �� ȣ��Ǵ� �޼����Դϴ�.
    /// </summary>
    public abstract void Enter();

    /// <summary>
    /// ���°� Ȱ��ȭ�Ǿ� �ִ� ���� �ֱ������� ȣ��Ǵ� �޼����Դϴ�.
    /// </summary>
    public abstract void Update();

    /// <summary>
    /// ���¿��� ���� �� ȣ��Ǵ� �޼����Դϴ�.
    /// </summary>
    public abstract void Exit();
}