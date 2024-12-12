using UnityEngine;
using static ComponentValidator;

/// <summary>
/// AI�� ���� ���¸� �����ϴ� Ŭ�����Դϴ�.
/// </summary>
public class PatrolState : AIState
{
    /// <summary>
    /// PatrolState�� �������Դϴ�.
    /// </summary>
    /// <param name="ai">AI ���� �ν��Ͻ�</param>
    public PatrolState(WizardRukeAIServer ai) : base(ai) { }

    #region AIState �κ��� ��ӹ��� �޼���
    /// <summary>
    /// ���� ���¿� ������ �� ȣ��Ǵ� �޼����Դϴ�.
    /// </summary>
    public override void Enter()
    {
        Logger.Log("Patrol State�� �����մϴ�.");

        InitState();
    }

    /// <summary>
    /// ���� ���°� Ȱ��ȭ�Ǿ� �ִ� ���� �ֱ������� ȣ��Ǵ� �޼����Դϴ�.
    /// </summary>
    public override void Update()
    {
        Logger.Log("Patrol State ������Ʈ.");

        Patrol();
    }

    /// <summary>
    /// ���� ���¿��� ���� �� ȣ��Ǵ� �޼����Դϴ�.
    /// </summary>
    public override void Exit()
    {
        Logger.Log("Patrol State���� ����ϴ�.");
    }
    #endregion

    #region Patrol State �޼���
    /// <summary>
    /// ���� ���¸� �ʱ�ȭ�ϴ� �޼����Դϴ�.
    /// </summary>
    private void InitState()
    {
        if (!ValidateComponent(ai, "AI ������ �ȵǾ��ֽ��ϴ�.")) return;

        ai.SetTarget(null);
    }

    /// <summary>
    /// ���� ������ ó���ϴ� �޼����Դϴ�.
    /// </summary>
    private void Patrol()
    {
        if (!ValidateAIComponents()) return;

        CycleThroughPatrolPoints();
        if (TryDetectAndSetTarget())
        {
            EvaluateTargetDistance();
        }
    }

    /// <summary>
    /// ���� �������� ��ȯ�ϴ� �޼����Դϴ�.
    /// </summary>
    private void CycleThroughPatrolPoints() => ai.GetMovementManager().Patrol();

    /// <summary>
    /// Ÿ���� �����ϰ� �����ϴ� �޼����Դϴ�.
    /// </summary>
    /// <returns>Ÿ���� �����Ǹ� true, �ƴϸ� false</returns>
    private bool TryDetectAndSetTarget()
    {
        GameObject detectedTarget = DetectTarget();
        if (detectedTarget != null)
        {
            ai.SetTarget(detectedTarget);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Ÿ���� �����ϴ� �޼����Դϴ�.
    /// </summary>
    /// <returns>������ Ÿ���� GameObject, ������ null</returns>
    private GameObject DetectTarget() => ai.GetTargetingSystem().DetectTarget<ITargetable>();

    /// <summary>
    /// Ÿ�ٰ��� �Ÿ��� ���ϰ� ������ �ൿ�� �����ϴ� �޼����Դϴ�.
    /// </summary>
    private void EvaluateTargetDistance()
    {
        float targetDistance = GetTargetDistance();

        // ��ǥ�� ���ݹ��� ���̸� ���ݻ��·� ��ȯ�մϴ�.
        if (IsTargetInAttackRange(targetDistance))
            SwitchToAttack();
        // ��ǥ�� ���ݹ��� ���̸鼭 �������� ���̸� �߰ݻ��·� ��ȯ�մϴ�.
        else if (IsTargetInDetectionRange(targetDistance))
            SwitchToChase();
    }

    /// <summary>
    /// Ÿ�ٰ��� �Ÿ��� ����ϴ� �޼����Դϴ�.
    /// </summary>
    /// <returns>AI�� Ÿ�� ������ �Ÿ�</returns>
    private float GetTargetDistance() => Vector3.Distance(ai.transform.position, ai.GetTarget().transform.position);

    /// <summary>
    /// Ÿ���� ���� ���� ���� �ִ��� Ȯ���ϴ� �޼����Դϴ�.
    /// </summary>
    /// <param name="targetDistance">Ÿ�ٰ��� �Ÿ�</param>
    /// <returns>Ÿ���� ���� ���� ���� ������ true, �ƴϸ� false</returns>
    private bool IsTargetInAttackRange(float targetDistance) => targetDistance <= ai.GetBattleManager().GetAttackRange();

    /// <summary>
    /// Ÿ���� ���� ���� ���� �ִ��� Ȯ���ϴ� �޼����Դϴ�.
    /// </summary>
    /// <param name="targetDistance">Ÿ�ٰ��� �Ÿ�</param>
    /// <returns>Ÿ���� ���� ���� ���� ������ true, �ƴϸ� false</returns>
    private bool IsTargetInDetectionRange(float targetDistance) => targetDistance <= ai.GetMaxDetectionDistance();

    /// <summary>
    /// ���� ���·� ��ȯ�ϴ� �޼����Դϴ�.
    /// </summary>
    private void SwitchToAttack() => ai.GetStateMachine().ChangeState(AIStateType.Attack);

    /// <summary>
    /// �߰ݻ��·� ��ȯ�ϴ� �޼����Դϴ�.
    /// </summary>
    private void SwitchToChase() => ai.GetStateMachine().ChangeState(AIStateType.Chase);

    /// <summary>
    /// AI ���� ����� ��ȿ���� �˻��ϴ� �޼����Դϴ�.
    /// </summary>
    /// <returns>��ȿ�� ������ ������ true, �ƴϸ� false</returns>
    private bool ValidateAIComponents()
    {
        return ValidateComponent(ai, "AI ������ �ȵǾ��ֽ��ϴ�.") &&
          ValidateComponent(ai.GetMovementManager(), "AI MovementManager ȹ�濡 �����߽��ϴ�.") &&
          ValidateComponent(ai.GetTargetingSystem(), "AI TargetingSystem ȹ�濡 �����߽��ϴ�.") &&
          ValidateComponent(ai.GetTarget(), "AI Target ȹ�濡 �����߽��ϴ�.") &&
          ValidateComponent(ai.GetBattleManager(), "AI BattleManager ȹ�濡 �����߽��ϴ�.") &&
          ValidateComponent(ai.GetStateMachine(), "AI StateMachine ȹ�濡 �����߽��ϴ�.");
    }
    #endregion
}