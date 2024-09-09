using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 서버 측에서 플레이어의 이동을 처리하는 컴포넌트입니다.
/// 서버 권한 방식으로 이동 속도과 회전을 관리합니다.
/// </summary>
public class PlayerMovementServer : NetworkBehaviour
{
    /// <summary>
    /// 플레이어의 회전 속도를 정의하는 상수입니다.
    /// </summary>
    private const float ROTATE_SPEED = 30f;

    [SerializeField] private PlayerClient playerClient;
    [SerializeField] private PlayerAnimator playerAnimator;

    /// <summary>
    /// 서버에서 플레이어의 이동을 처리합니다.
    /// </summary>
    /// <param name="inputVector">이동 입력 벡터</param>
    /// <param name="isAttackButtonClicked">공격 버튼 클릭 여부</param>
    [ServerRpc (RequireOwnership = false)]
    public void HandleMovementServerRPC(Vector2 inputVector, bool isAttackButtonClicked, ServerRpcParams serverRpcParams = default)
    {
        // GameOver상태일 경우 이동처리가 안되도록 합니다.
        if (GameMultiplayer.Instance.GetPlayerDataFromClientId(OwnerClientId).playerGameState == PlayerGameState.GameOver) return;

        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);
        float moveDistance = ((ICharacter)playerClient).moveSpeed * NetworkManager.Singleton.ServerTime.FixedDeltaTime;
        transform.position += moveDir * moveDistance;

        // 이동값에 따라 애니메이션을 실행시켜줍니다.
        if (moveDir != Vector3.zero)
            playerAnimator.UpdatePlayerMoveAnimationOnServer(PlayerMoveAnimState.Walking);
        else
            playerAnimator.UpdatePlayerMoveAnimationOnServer(PlayerMoveAnimState.Idle);

        // 이동처리가 끝났으면 회전처리를 해줍니다. 공격중이 아닐 때에만 진행방향으로 캐릭터를 회전시킵니다.
        if (isAttackButtonClicked) return;
        Rotate(moveDir);
    }

    /// <summary>
    /// 서버에서 플레이어를 회전시킵니다.
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

    /// <summary>
    /// 플레이어의 이동 속도를 감소시킵니다.
    /// </summary>
    /// <param name="reduceValue">감소시킬 속도 값</param>
    public void ReduceMoveSpeed(float reduceValue)
    {
        ((ICharacter)playerClient).moveSpeed -= reduceValue;
    }

    /// <summary>
    /// 플레이어의 이동 속도를 증가시킵니다.
    /// </summary>
    /// <param name="addValue">증가시킬 속도 값</param>
    public void AddMoveSpeed(float addValue)
    {
        ((ICharacter)playerClient).moveSpeed += addValue; 
    }
}
