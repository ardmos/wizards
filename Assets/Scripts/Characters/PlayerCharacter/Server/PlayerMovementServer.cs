using Unity.Netcode;
using UnityEngine;
using static ComponentValidator;

/// <summary>
/// ���� ������ �÷��̾��� �̵��� ó���ϴ� ������Ʈ�Դϴ�.
/// ���� ���� ������� ó���ϴ� �÷��̾� �̵��� ������ �����Դϴ�.
/// </summary>
public class PlayerMovementServer : NetworkBehaviour
{
    #region Constants & Fields
    // ���� �޼��� �����...
    private const string ERROR_PLAYER_SERVER_NOT_SET = "PlayerMovementServer playerServer ������ �ȵǾ��ֽ��ϴ�.";
    private const string ERROR_PLAYER_ANIMATOR_NOT_SET = "PlayerMovementServer playerAnimator ������ �ȵǾ��ֽ��ϴ�.";
    private const string ERROR_CURRENT_PLAYER_DATA_MANAGER_NOT_SET = "PlayerMovementServer CurrentPlayerDataManager.Instance ������ �ȵǾ��ֽ��ϴ�.";
    // �÷��̾��� ȸ�� �ӵ��� �����ϴ� ����Դϴ�.
    private const float ROTATE_SPEED = 30f;

    [SerializeField] private PlayerServer playerServer;
    [SerializeField] private PlayerAnimator playerAnimator;
    #endregion

    #region Movement Handling
    /// <summary>
    /// �������� �÷��̾��� �̵��� ó���մϴ�.
    /// </summary>
    /// <param name="inputVector">�̵� �Է� ����</param>
    /// <param name="isAttackButtonClicked">���� ��ư Ŭ�� ����</param>
    [ServerRpc(RequireOwnership = false)]
    public void HandleMovementServerRPC(Vector2 inputVector, bool isAttacking, ServerRpcParams serverRpcParams = default)
    {
        Vector3 moveDir = Move(inputVector);
        StartAnimation(moveDir);
        RotateCharacterIfNotAttacking(isAttacking, moveDir);
    }

    /// <summary>
    /// �÷��̾ �Է� ���Ϳ� ���� �̵���ŵ�ϴ�.
    /// </summary>
    /// <param name="inputVector">�̵� �Է� ����</param>
    /// <returns>���� �̵� ���� ����</returns>
    private Vector3 Move(Vector2 inputVector)
    {
        if (!ValidateComponent(playerServer, ERROR_PLAYER_SERVER_NOT_SET)) return Vector3.zero;
        if (!ValidateComponent(CurrentPlayerDataManager.Instance, ERROR_CURRENT_PLAYER_DATA_MANAGER_NOT_SET)) return Vector3.zero;
        // GameOver������ ��� �̵�ó���� �ȵǵ��� �մϴ�.
        if (CurrentPlayerDataManager.Instance.GetPlayerDataByClientId(OwnerClientId).playerGameState == PlayerGameState.GameOver) return Vector3.zero;

        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);
        float moveDistance = ((ICharacter)playerServer).moveSpeed * NetworkManager.Singleton.ServerTime.FixedDeltaTime;
        transform.position += moveDir * moveDistance;

        return moveDir;
    }

    /// <summary>
    /// �������� �ƴ� ������ ����������� ĳ���͸� ȸ����ŵ�ϴ�.
    /// </summary>
    private void RotateCharacterIfNotAttacking(bool isAttacking, Vector3 moveDir)
    {
        if (!isAttacking) Rotate(moveDir);
    }

    /// <summary>
    /// �÷��̾ ȸ����Ű�� ServerRPC�Դϴ�.
    /// </summary>
    /// <param name="dir">ȸ�� ����</param>
    [ServerRpc(RequireOwnership = false)]
    public void RotateByDragSpellButtonServerRPC(Vector3 dir)
    {
        Rotate(dir);
    }

    /// <summary>
    /// �÷��̾ ������ �������� ȸ����ŵ�ϴ�.
    /// </summary>
    /// <param name="moveDir">�̵� ����</param>
    private void Rotate(Vector3 moveDir)
    {
        if (moveDir == Vector3.zero) return;

        Vector3 slerpResult = Vector3.Slerp(transform.forward, moveDir, Time.deltaTime * ROTATE_SPEED);
        transform.forward = slerpResult;
    }
    #endregion

    #region Animation Management
    /// <summary>
    /// �̵����� ���� �ִϸ��̼��� ��������ݴϴ�.
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
    /// �÷��̾��� �̵� �ӵ��� ���ҽ�ŵ�ϴ�.
    /// </summary>
    /// <param name="reduceValue">���ҽ�ų �ӵ� ��</param>
    public void ReduceMoveSpeed(float reduceValue)
    {
        if (!ValidateComponent(playerServer, ERROR_PLAYER_SERVER_NOT_SET)) return;

        ((ICharacter)playerServer).moveSpeed -= reduceValue;
    }

    /// <summary>
    /// �÷��̾��� �̵� �ӵ��� ������ŵ�ϴ�.
    /// </summary>
    /// <param name="addValue">������ų �ӵ� ��</param>
    public void AddMoveSpeed(float addValue)
    {
        if (!ValidateComponent(playerServer, ERROR_PLAYER_SERVER_NOT_SET)) return;

        ((ICharacter)playerServer).moveSpeed += addValue;
    }
    #endregion
}