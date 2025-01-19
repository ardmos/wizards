using UnityEngine;
using static ComponentValidator;

/// <summary>
/// AI�� ���� ���¸� �����ϴ� Ŭ�����Դϴ�.
/// </summary>
public class AttackState : AIState
{
    #region Constructor
    /// <summary>
    /// AttackState�� �������Դϴ�.
    /// </summary>
    /// <param name="ai">AI ���� �ν��Ͻ�</param>
    public AttackState(WizardRukeAIServer ai) : base(ai) { }
    #endregion

    #region AIState Override Methods
    /// <summary>
    /// ���� ���¿� ������ �� ȣ��Ǵ� �޼����Դϴ�.
    /// </summary>
    public override void Enter()
    {
        Logger.Log("Attack State�� �����մϴ�.");
    }

    /// <summary>
    /// ���� ���°� Ȱ��ȭ�Ǿ� �ִ� ���� �ֱ������� ȣ��Ǵ� �޼����Դϴ�.
    /// </summary>
    public override void Update()
    {
        //Logger.Log("Attack State ������Ʈ.");

        // ���� ������ �����մϴ�.
        Attack();
    }

    /// <summary>
    /// ���� ���¿��� ���� �� ȣ��Ǵ� �޼����Դϴ�.
    /// </summary>
    public override void Exit()
    {
        Logger.Log("Attack State���� ����ϴ�.");
    }
    #endregion

    #region Core Attack Logic
    /// <summary>
    /// ���� ������ ó���ϴ� �޼����Դϴ�.
    /// </summary>
    private void Attack()
    {
        if (!ValidateAIComponents()) return;

        KeepMoving();
        EvaluateTargetDistance();
    }

    /// <summary>
    /// ��ǥ�� ���� �̵��ϴ� �޼����Դϴ�.
    /// </summary>
    private void KeepMoving() => ai.MoveTowardsTarget();

    /// <summary>
    /// ��ǥ�� �����ϴ� �޼����Դϴ�.
    /// </summary>
    private void AttackTarget() => ai.AttackTarget();
    #endregion

    #region Target Distance Evaluation
    /// <summary>
    /// ��ǥ���� �Ÿ��� Ȯ���ϰ� ������ �ൿ�� �����ϴ� �޼����Դϴ�.
    /// </summary>
    private void EvaluateTargetDistance()
    {
        float targetDistance = GetTargetDistance();

        // ��ǥ�� ���ݹ��� ���̸� ���� ������ �����մϴ�.
        if (IsTargetInAttackRange(targetDistance))
            AttackTarget();
        // ��ǥ�� ���ݹ��� ���̸� �߰ݻ��·� ��ȯ�մϴ�.
        else
            SwitchToChase();
    }
    #endregion

    #region Target Distance Calculation
    /// <summary>
    /// ��ǥ���� �Ÿ��� ����ϴ� �޼����Դϴ�.
    /// </summary>
    /// <returns>AI�� ��ǥ ������ �Ÿ�</returns>
    private float GetTargetDistance() => Vector3.Distance(ai.transform.position, ai.GetTarget().transform.position);

    /// <summary>
    /// ��ǥ�� ���� ���� ���� �ִ��� Ȯ���ϴ� �޼����Դϴ�.
    /// </summary>
    /// <returns>target�� ���ݹ��� ���� ������ true, �ƴϸ� false</returns>
    private bool IsTargetInAttackRange(float targetDistance) => targetDistance <= ai.GetBattleManager().GetAttackRange();
    #endregion

    #region State Transition
    /// <summary>
    /// �߰ݻ��·� ��ȯ�ϴ� �޼����Դϴ�.
    /// </summary>
    private void SwitchToChase() => ai.GetStateMachine().ChangeState(AIStateType.Chase);
    #endregion

    #region Validation Check
    /// <summary>
    /// AI ���� ����� ��ȿ���� �˻��ϴ� �޼����Դϴ�.
    /// </summary>
    /// <returns>��ȿ�� ������ ������ true, �ƴϸ� false</returns>
    private bool ValidateAIComponents()
    {
        return ValidateComponent(ai, "AI ������ �ȵǾ��ֽ��ϴ�.") &&
          ValidateComponent(ai.GetTarget(), "AI Target ȹ�濡 �����߽��ϴ�.") &&
          ValidateComponent(ai.GetBattleManager(), "AI BattleManager ȹ�濡 �����߽��ϴ�.") &&
          ValidateComponent(ai.GetStateMachine(), "AI StateMachine ȹ�濡 �����߽��ϴ�.");
    }
    #endregion
}