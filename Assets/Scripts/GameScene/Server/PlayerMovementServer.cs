using Unity.Netcode;
using UnityEngine;

/// <summary>
/// �̵��ӵ�(moveSpeen)�� ���������� ����. 
/// </summary>
public class PlayerMovementServer : NetworkBehaviour
{
    public PlayerClient playerClient;
    public PlayerAnimator playerAnimator;

    [ServerRpc (RequireOwnership = false)]
    public void HandleMovementServerRPC(Vector2 inputVector, bool isAttackButtonClicked, ServerRpcParams serverRpcParams = default)
    {
        //Debug.Log($"player{OwnerClientId}GameState: {GameMultiplayer.Instance.GetPlayerDataFromClientId(OwnerClientId).playerGameState}");
        // GameOver���°� �ƴ� ������!)
        if (GameMultiplayer.Instance.GetPlayerDataFromClientId(OwnerClientId).playerGameState == PlayerGameState.GameOver)
        {
            playerAnimator.UpdatePlayerMoveAnimationOnServer(PlayerMoveAnimState.GameOver);
            return;
        }

        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);

        // �������ѹ���� ��Ʈ��ũ���� �̵�ó�� �� �� ������ DeltaTime�� Ŭ���̾�Ʈ�� ��ŸŸ�Ӱ��� �ٸ� ��찡 ����. ���� �Ʒ�ó�� �����ؾ���
        //float moveDistance = moveSpeed * Time.deltaTime;
        float moveDistance = ((ICharacter)playerClient).moveSpeed * NetworkManager.Singleton.ServerTime.FixedDeltaTime;
        transform.position += moveDir * moveDistance;

        if (moveDir != Vector3.zero)
            //GameMultiplayer.Instance.UpdatePlayerMoveAnimStateOnServer(serverRpcParams.Receive.SenderClientId, PlayerMoveAnimState.Walking);
            playerAnimator.UpdatePlayerMoveAnimationOnServer(PlayerMoveAnimState.Walking);
        else
            //GameMultiplayer.Instance.UpdatePlayerMoveAnimStateOnServer(serverRpcParams.Receive.SenderClientId, PlayerMoveAnimState.Idle);
            playerAnimator.UpdatePlayerMoveAnimationOnServer(PlayerMoveAnimState.Idle);

        // �������� �ƴ� ������ ����������� ĳ���� ȸ��
        if (isAttackButtonClicked) return;
        Rotate(moveDir);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RotateByDragSpellButtonServerRPC(Vector3 dir)
    {
        Rotate(dir);
    }

    private void Rotate(Vector3 moveDir)
    {
        if (moveDir == Vector3.zero) return;

        float rotateSpeed = 30f;
        Vector3 slerpResult = Vector3.Slerp(transform.forward, moveDir, Time.deltaTime * rotateSpeed);
        transform.forward = slerpResult;
    }

    public void SetMoveSpeed(float moveSpeed)
    {
        ((ICharacter)playerClient).moveSpeed = moveSpeed;
    }
}
