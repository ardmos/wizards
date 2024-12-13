using UnityEngine;
using static ComponentValidator;

/// <summary>
/// AI�� �߰� ���¸� �����ϴ� Ŭ�����Դϴ�.
/// </summary>
public class ChaseState : AIState
{
    #region Constructor
    /// <summary>
    /// ChaseState�� �������Դϴ�.
    /// </summary>
    /// <param name="ai">AI ���� �ν��Ͻ�</param>
    public ChaseState(WizardRukeAIServer ai) : base(ai) { }
    #endregion

    #region AIState Override Methods
    /// <summary>
    /// �߰� ���¿� ������ �� ȣ��Ǵ� �޼����Դϴ�.
    /// </summary>
    public override void Enter()
    {
        Logger.Log("Chase State�� �����մϴ�.");
    }

    /// <summary>
    /// �߰� ���°� Ȱ��ȭ�Ǿ� �ִ� ���� �ֱ������� ȣ��Ǵ� �޼����Դϴ�.
    /// </summary>
    public override void Update()
    {
        Logger.Log("Chase State ������Ʈ.");

        // �߰� ������ �����մϴ�.
        Chase();
    }

    /// <summary>
    /// �߰� ���¿��� ���� �� ȣ��Ǵ� �޼����Դϴ�.
    /// </summary>
    public override void Exit()
    {
        Logger.Log("Chase State���� ����ϴ�.");
    }
    #endregion

    #region Core Chase Logic
    /// <summary>
    /// �߰� ������ ó���ϴ� �޼����Դϴ�.
    /// </summary>
    private void Chase()
    {
        if (!ValidateAIComponents()) return;

        ChaseTarget();
        EvaluateTargetDistance();
    }

    /// <summary>
    /// ��ǥ�� ���� �̵��ϴ� �޼����Դϴ�.
    /// </summary>
    private void ChaseTarget() => ai.MoveTowardsTarget();
    #endregion

    #region Target Distance Evaluation
    /// <summary>
    /// ��ǥ���� �Ÿ��� Ȯ���ϰ� ������ �ൿ�� �����ϴ� �޼����Դϴ�.
    /// </summary>
    private void EvaluateTargetDistance()
    {
        float targetDistance = GetTargetDistance();

        // ��ǥ�� �߰ݹ��� ���̸� �������·� ��ȯ�մϴ�.
        if (IsTargetOutOfChaseRange(targetDistance))
            SwitchToPatrol();
        // ��ǥ�� ���ݹ��� ���̸� ���ݻ��·� ��ȯ�մϴ�.
        else if (IsTargetInAttackRange(targetDistance))
            SwitchToAttack();
    }
    #endregion

    #region Target Distance Calculation
    /// <summary>
    /// ��ǥ���� �Ÿ��� ����ϴ� �޼����Դϴ�.
    /// </summary>
    /// <returns>AI�� ��ǥ ������ �Ÿ�</returns>
    private float GetTargetDistance() => Vector3.Distance(ai.transform.position, ai.GetTarget().transform.position);

    /// <summary>
    /// ��ǥ�� �߰� ������ ������� Ȯ���ϴ� �޼����Դϴ�.
    /// </summary>
    /// <param name="targetDistance">��ǥ���� �Ÿ�</param>
    /// <returns>��ǥ�� �߰� ������ ������� true, �ƴϸ� false</returns>
    private bool IsTargetOutOfChaseRange(float targetDistance) => targetDistance > ai.GetMaxDetectionDistance();

    /// <summary>
    /// ��ǥ�� ���� ���� ���� �ִ��� Ȯ���ϴ� �޼����Դϴ�.
    /// </summary>
    /// <param name="targetDistance">��ǥ���� �Ÿ�</param>
    /// <returns>��ǥ�� ���� ���� ���� ������ true, �ƴϸ� false</returns>
    private bool IsTargetInAttackRange(float targetDistance) => targetDistance <= ai.GetBattleManager().GetAttackRange();
    #endregion

    #region State Transition
    /// <summary>
    /// ���� ���·� ��ȯ�ϴ� �޼����Դϴ�.
    /// </summary>
    private void SwitchToPatrol() => ai.GetStateMachine().ChangeState(AIStateType.Patrol);

    /// <summary>
    /// ���� ���·� ��ȯ�ϴ� �޼����Դϴ�.
    /// </summary>
    private void SwitchToAttack() => ai.GetStateMachine().ChangeState(AIStateType.Attack);


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