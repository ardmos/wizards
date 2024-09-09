using Unity.Netcode;
using UnityEngine;

/// <summary>
/// ���� ������ �÷��̾��� �̵��� ó���ϴ� ������Ʈ�Դϴ�.
/// ���� ���� ������� �̵� �ӵ��� ȸ���� �����մϴ�.
/// </summary>
public class PlayerMovementServer : NetworkBehaviour
{
    /// <summary>
    /// �÷��̾��� ȸ�� �ӵ��� �����ϴ� ����Դϴ�.
    /// </summary>
    private const float ROTATE_SPEED = 30f;

    [SerializeField] private PlayerClient playerClient;
    [SerializeField] private PlayerAnimator playerAnimator;

    /// <summary>
    /// �������� �÷��̾��� �̵��� ó���մϴ�.
    /// </summary>
    /// <param name="inputVector">�̵� �Է� ����</param>
    /// <param name="isAttackButtonClicked">���� ��ư Ŭ�� ����</param>
    [ServerRpc (RequireOwnership = false)]
    public void HandleMovementServerRPC(Vector2 inputVector, bool isAttackButtonClicked, ServerRpcParams serverRpcParams = default)
    {
        // GameOver������ ��� �̵�ó���� �ȵǵ��� �մϴ�.
        if (GameMultiplayer.Instance.GetPlayerDataFromClientId(OwnerClientId).playerGameState == PlayerGameState.GameOver) return;

        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);
        float moveDistance = ((ICharacter)playerClient).moveSpeed * NetworkManager.Singleton.ServerTime.FixedDeltaTime;
        transform.position += moveDir * moveDistance;

        // �̵����� ���� �ִϸ��̼��� ��������ݴϴ�.
        if (moveDir != Vector3.zero)
            playerAnimator.UpdatePlayerMoveAnimationOnServer(PlayerMoveAnimState.Walking);
        else
            playerAnimator.UpdatePlayerMoveAnimationOnServer(PlayerMoveAnimState.Idle);

        // �̵�ó���� �������� ȸ��ó���� ���ݴϴ�. �������� �ƴ� ������ ����������� ĳ���͸� ȸ����ŵ�ϴ�.
        if (isAttackButtonClicked) return;
        Rotate(moveDir);
    }

    /// <summary>
    /// �������� �÷��̾ ȸ����ŵ�ϴ�.
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

    /// <summary>
    /// �÷��̾��� �̵� �ӵ��� ���ҽ�ŵ�ϴ�.
    /// </summary>
    /// <param name="reduceValue">���ҽ�ų �ӵ� ��</param>
    public void ReduceMoveSpeed(float reduceValue)
    {
        ((ICharacter)playerClient).moveSpeed -= reduceValue;
    }

    /// <summary>
    /// �÷��̾��� �̵� �ӵ��� ������ŵ�ϴ�.
    /// </summary>
    /// <param name="addValue">������ų �ӵ� ��</param>
    public void AddMoveSpeed(float addValue)
    {
        ((ICharacter)playerClient).moveSpeed += addValue; 
    }
}
