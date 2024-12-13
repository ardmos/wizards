using UnityEngine;
using static ComponentValidator;

/// <summary>
/// AI의 추격 상태를 관리하는 클래스입니다.
/// </summary>
public class ChaseState : AIState
{
    #region Constructor
    /// <summary>
    /// ChaseState의 생성자입니다.
    /// </summary>
    /// <param name="ai">AI 서버 인스턴스</param>
    public ChaseState(WizardRukeAIServer ai) : base(ai) { }
    #endregion

    #region AIState Override Methods
    /// <summary>
    /// 추격 상태에 진입할 때 호출되는 메서드입니다.
    /// </summary>
    public override void Enter()
    {
        Logger.Log("Chase State에 진입합니다.");
    }

    /// <summary>
    /// 추격 상태가 활성화되어 있는 동안 주기적으로 호출되는 메서드입니다.
    /// </summary>
    public override void Update()
    {
        Logger.Log("Chase State 업데이트.");

        // 추격 로직을 실행합니다.
        Chase();
    }

    /// <summary>
    /// 추격 상태에서 나갈 때 호출되는 메서드입니다.
    /// </summary>
    public override void Exit()
    {
        Logger.Log("Chase State에서 벗어납니다.");
    }
    #endregion

    #region Core Chase Logic
    /// <summary>
    /// 추격 로직을 처리하는 메서드입니다.
    /// </summary>
    private void Chase()
    {
        if (!ValidateAIComponents()) return;

        ChaseTarget();
        EvaluateTargetDistance();
    }

    /// <summary>
    /// 목표를 향해 이동하는 메서드입니다.
    /// </summary>
    private void ChaseTarget() => ai.MoveTowardsTarget();
    #endregion

    #region Target Distance Evaluation
    /// <summary>
    /// 목표와의 거리를 확인하고 적절한 행동을 결정하는 메서드입니다.
    /// </summary>
    private void EvaluateTargetDistance()
    {
        float targetDistance = GetTargetDistance();

        // 목표가 추격범위 밖이면 순찰상태로 전환합니다.
        if (IsTargetOutOfChaseRange(targetDistance))
            SwitchToPatrol();
        // 목표가 공격범위 안이면 공격상태로 전환합니다.
        else if (IsTargetInAttackRange(targetDistance))
            SwitchToAttack();
    }
    #endregion

    #region Target Distance Calculation
    /// <summary>
    /// 목표와의 거리를 계산하는 메서드입니다.
    /// </summary>
    /// <returns>AI와 목표 사이의 거리</returns>
    private float GetTargetDistance() => Vector3.Distance(ai.transform.position, ai.GetTarget().transform.position);

    /// <summary>
    /// 목표가 추격 범위를 벗어났는지 확인하는 메서드입니다.
    /// </summary>
    /// <param name="targetDistance">목표와의 거리</param>
    /// <returns>목표가 추격 범위를 벗어났으면 true, 아니면 false</returns>
    private bool IsTargetOutOfChaseRange(float targetDistance) => targetDistance > ai.GetMaxDetectionDistance();

    /// <summary>
    /// 목표가 공격 범위 내에 있는지 확인하는 메서드입니다.
    /// </summary>
    /// <param name="targetDistance">목표와의 거리</param>
    /// <returns>목표가 공격 범위 내에 있으면 true, 아니면 false</returns>
    private bool IsTargetInAttackRange(float targetDistance) => targetDistance <= ai.GetBattleManager().GetAttackRange();
    #endregion

    #region State Transition
    /// <summary>
    /// 순찰 상태로 전환하는 메서드입니다.
    /// </summary>
    private void SwitchToPatrol() => ai.GetStateMachine().ChangeState(AIStateType.Patrol);

    /// <summary>
    /// 공격 상태로 전환하는 메서드입니다.
    /// </summary>
    private void SwitchToAttack() => ai.GetStateMachine().ChangeState(AIStateType.Attack);


    #endregion

    #region Validation Check
    /// <summary>
    /// AI 구성 요소의 유효성을 검사하는 메서드입니다.
    /// </summary>
    /// <returns>유효성 문제가 없으면 true, 아니면 false</returns>
    private bool ValidateAIComponents()
    {
        return ValidateComponent(ai, "AI 설정이 안되어있습니다.") &&
          ValidateComponent(ai.GetTarget(), "AI Target 획득에 실패했습니다.") &&
          ValidateComponent(ai.GetBattleManager(), "AI BattleManager 획득에 실패했습니다.") &&
          ValidateComponent(ai.GetStateMachine(), "AI StateMachine 획득에 실패했습니다.");
    }
    #endregion
}