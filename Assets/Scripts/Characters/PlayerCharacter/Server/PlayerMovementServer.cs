using Unity.Netcode;
using UnityEngine;
using static ComponentValidator;

/// <summary>
/// 서버 측에서 플레이어의 이동을 처리하는 컴포넌트입니다.
/// 서버 권한 방식으로 처리하는 플레이어 이동의 서버측 로직입니다.
/// </summary>
public class PlayerMovementServer : NetworkBehaviour
{
    #region Constants & Fields
    // 에러 메세지 상수들...
    private const string ERROR_PLAYER_SERVER_NOT_SET = "PlayerMovementServer playerServer 설정이 안되어있습니다.";
    private const string ERROR_PLAYER_ANIMATOR_NOT_SET = "PlayerMovementServer playerAnimator 설정이 안되어있습니다.";
    private const string ERROR_CURRENT_PLAYER_DATA_MANAGER_NOT_SET = "PlayerMovementServer CurrentPlayerDataManager.Instance 설정이 안되어있습니다.";
    // 플레이어의 회전 속도를 정의하는 상수입니다.
    private const float ROTATE_SPEED = 30f;

    [SerializeField] private PlayerServer playerServer;
    [SerializeField] private PlayerAnimator playerAnimator;
    #endregion

    #region Movement Handling
    /// <summary>
    /// 서버에서 플레이어의 이동을 처리합니다.
    /// </summary>
    /// <param name="inputVector">이동 입력 벡터</param>
    /// <param name="isAttackButtonClicked">공격 버튼 클릭 여부</param>
    [ServerRpc(RequireOwnership = false)]
    public void HandleMovementServerRPC(Vector2 inputVector, bool isAttacking, ServerRpcParams serverRpcParams = default)
    {
        Vector3 moveDir = Move(inputVector);
        StartAnimation(moveDir);
        RotateCharacterIfNotAttacking(isAttacking, moveDir);
    }

    /// <summary>
    /// 플레이어를 입력 벡터에 따라 이동시킵니다.
    /// </summary>
    /// <param name="inputVector">이동 입력 벡터</param>
    /// <returns>실제 이동 방향 벡터</returns>
    private Vector3 Move(Vector2 inputVector)
    {
        if (!ValidateComponent(playerServer, ERROR_PLAYER_SERVER_NOT_SET)) return Vector3.zero;
        if (!ValidateComponent(CurrentPlayerDataManager.Instance, ERROR_CURRENT_PLAYER_DATA_MANAGER_NOT_SET)) return Vector3.zero;
        // GameOver상태일 경우 이동처리가 안되도록 합니다.
        if (CurrentPlayerDataManager.Instance.GetPlayerDataByClientId(OwnerClientId).playerGameState == PlayerGameState.GameOver) return Vector3.zero;

        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);
        float moveDistance = ((ICharacter)playerServer).moveSpeed * NetworkManager.Singleton.ServerTime.FixedDeltaTime;
        transform.position += moveDir * moveDistance;

        return moveDir;
    }

    /// <summary>
    /// 공격중이 아닐 때에만 진행방향으로 캐릭터를 회전시킵니다.
    /// </summary>
    private void RotateCharacterIfNotAttacking(bool isAttacking, Vector3 moveDir)
    {
        if (!isAttacking) Rotate(moveDir);
    }

    /// <summary>
    /// 플레이어를 회전시키는 ServerRPC입니다.
    /// </summary>
    /// <param name="dir">회전 방향</param>
    [ServerRpc(RequireOwnership = false)]
    public void RotateByDragSpellButtonServerRPC(Vector3 dir)
    {
        Rotate(dir);
    }

    /// <summary>
    /// 플레이어를 지정된 방향으로 회전시킵니다.
    /// </summary>
    /// <param name="moveDir">이동 방향</param>
    private void Rotate(Vector3 moveDir)
    {
        if (moveDir == Vector3.zero) return;

        Vector3 slerpResult = Vector3.Slerp(transform.forward, moveDir, Time.deltaTime * ROTATE_SPEED);
        transform.forward = slerpResult;
    }
    #endregion

    #region Animation Management
    /// <summary>
    /// 이동값에 따라 애니메이션을 실행시켜줍니다.
    /// </summary>
    private void StartAnimation(Vector3 moveDir)
    {
        if (!ValidateComponent(playerAnimator, ERROR_PLAYER_ANIMATOR_NOT_SET)) return;

        if (moveDir != Vector3.zero) playerAnimator.UpdatePlayerAnimationOnServer(PlayerMoveAnimState.Walking);
        else playerAnimator.UpdatePlayerAnimationOnServer(PlayerMoveAnimState.Idle);
    }
    #endregion

    #region Speed Management
    /// <summary>
    /// 플레이어의 이동 속도를 감소시킵니다.
    /// </summary>
    /// <param name="reduceValue">감소시킬 속도 값</param>
    public void ReduceMoveSpeed(float reduceValue)
    {
        if (!ValidateComponent(playerServer, ERROR_PLAYER_SERVER_NOT_SET)) return;

        ((ICharacter)playerServer).moveSpeed -= reduceValue;
    }

    /// <summary>
    /// 플레이어의 이동 속도를 증가시킵니다.
    /// </summary>
    /// <param name="addValue">증가시킬 속도 값</param>
    public void AddMoveSpeed(float addValue)
    {
        if (!ValidateComponent(playerServer, ERROR_PLAYER_SERVER_NOT_SET)) return;

        ((ICharacter)playerServer).moveSpeed += addValue;
    }
    #endregion
}