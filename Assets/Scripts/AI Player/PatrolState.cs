using UnityEngine;
using static ComponentValidator;

/// <summary>
/// AI의 순찰 상태를 관리하는 클래스입니다.
/// </summary>
public class PatrolState : AIState
{
    /// <summary>
    /// PatrolState의 생성자입니다.
    /// </summary>
    /// <param name="ai">AI 서버 인스턴스</param>
    public PatrolState(WizardRukeAIServer ai) : base(ai) { }

    #region AIState 로부터 상속받은 메서드
    /// <summary>
    /// 순찰 상태에 진입할 때 호출되는 메서드입니다.
    /// </summary>
    public override void Enter()
    {
        Logger.Log("Patrol State에 진입합니다.");

        InitState();
    }

    /// <summary>
    /// 순찰 상태가 활성화되어 있는 동안 주기적으로 호출되는 메서드입니다.
    /// </summary>
    public override void Update()
    {
        Logger.Log("Patrol State 업데이트.");

        Patrol();
    }

    /// <summary>
    /// 순찰 상태에서 나갈 때 호출되는 메서드입니다.
    /// </summary>
    public override void Exit()
    {
        Logger.Log("Patrol State에서 벗어납니다.");
    }
    #endregion

    #region Patrol State 메서드
    /// <summary>
    /// 순찰 상태를 초기화하는 메서드입니다.
    /// </summary>
    private void InitState()
    {
        if (!ValidateComponent(ai, "AI 설정이 안되어있습니다.")) return;

        ai.SetTarget(null);
    }

    /// <summary>
    /// 순찰 로직을 처리하는 메서드입니다.
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
    /// 순찰 지점들을 순환하는 메서드입니다.
    /// </summary>
    private void CycleThroughPatrolPoints() => ai.GetMovementManager().Patrol();

    /// <summary>
    /// 타겟을 감지하고 설정하는 메서드입니다.
    /// </summary>
    /// <returns>타겟이 감지되면 true, 아니면 false</returns>
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
    /// 타겟을 감지하는 메서드입니다.
    /// </summary>
    /// <returns>감지된 타겟의 GameObject, 없으면 null</returns>
    private GameObject DetectTarget() => ai.GetTargetingSystem().DetectTarget<ITargetable>();

    /// <summary>
    /// 타겟과의 거리를 평가하고 적절한 행동을 결정하는 메서드입니다.
    /// </summary>
    private void EvaluateTargetDistance()
    {
        float targetDistance = GetTargetDistance();

        // 목표가 공격범위 안이면 공격상태로 전환합니다.
        if (IsTargetInAttackRange(targetDistance))
            SwitchToAttack();
        // 목표가 공격범위 밖이면서 감지범위 안이면 추격상태로 전환합니다.
        else if (IsTargetInDetectionRange(targetDistance))
            SwitchToChase();
    }

    /// <summary>
    /// 타겟과의 거리를 계산하는 메서드입니다.
    /// </summary>
    /// <returns>AI와 타겟 사이의 거리</returns>
    private float GetTargetDistance() => Vector3.Distance(ai.transform.position, ai.GetTarget().transform.position);

    /// <summary>
    /// 타겟이 공격 범위 내에 있는지 확인하는 메서드입니다.
    /// </summary>
    /// <param name="targetDistance">타겟과의 거리</param>
    /// <returns>타겟이 공격 범위 내에 있으면 true, 아니면 false</returns>
    private bool IsTargetInAttackRange(float targetDistance) => targetDistance <= ai.GetBattleManager().GetAttackRange();

    /// <summary>
    /// 타겟이 감지 범위 내에 있는지 확인하는 메서드입니다.
    /// </summary>
    /// <param name="targetDistance">타겟과의 거리</param>
    /// <returns>타겟이 감지 범위 내에 있으면 true, 아니면 false</returns>
    private bool IsTargetInDetectionRange(float targetDistance) => targetDistance <= ai.GetMaxDetectionDistance();

    /// <summary>
    /// 공격 상태로 전환하는 메서드입니다.
    /// </summary>
    private void SwitchToAttack() => ai.GetStateMachine().ChangeState(AIStateType.Attack);

    /// <summary>
    /// 추격상태로 전환하는 메서드입니다.
    /// </summary>
    private void SwitchToChase() => ai.GetStateMachine().ChangeState(AIStateType.Chase);

    /// <summary>
    /// AI 구성 요소의 유효성을 검사하는 메서드입니다.
    /// </summary>
    /// <returns>유효성 문제가 없으면 true, 아니면 false</returns>
    private bool ValidateAIComponents()
    {
        return ValidateComponent(ai, "AI 설정이 안되어있습니다.") &&
          ValidateComponent(ai.GetMovementManager(), "AI MovementManager 획득에 실패했습니다.") &&
          ValidateComponent(ai.GetTargetingSystem(), "AI TargetingSystem 획득에 실패했습니다.") &&
          ValidateComponent(ai.GetTarget(), "AI Target 획득에 실패했습니다.") &&
          ValidateComponent(ai.GetBattleManager(), "AI BattleManager 획득에 실패했습니다.") &&
          ValidateComponent(ai.GetStateMachine(), "AI StateMachine 획득에 실패했습니다.");
    }
    #endregion
}