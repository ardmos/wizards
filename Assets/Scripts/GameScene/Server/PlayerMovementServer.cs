using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// �̵��ӵ�(moveSpeen)�� ���������� ����. 
/// </summary>
public class PlayerMovementServer : NetworkBehaviour
{
    [SerializeField] protected float moveSpeed;

    [ServerRpc (RequireOwnership = false)]
    public void HandleMovementServerRPC(Vector2 inputVector, bool isAttackButtonClicked, ServerRpcParams serverRpcParams = default)
    {
        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);

        // �������ѹ���� ��Ʈ��ũ���� �̵�ó�� �� �� ������ DeltaTime�� Ŭ���̾�Ʈ�� ��ŸŸ�Ӱ��� �ٸ� ��찡 ����. ���� �Ʒ�ó�� �����ؾ���
        //float moveDistance = moveSpeed * Time.deltaTime;
        float moveDistance = moveSpeed * NetworkManager.Singleton.ServerTime.FixedDeltaTime;
        transform.position += moveDir * moveDistance;

        // ����(GameMultiplayer)�� ���ο� Player Anim State ����. (GameOver���°� �ƴ� ������!)
        if (GameMultiplayer.Instance.GetPlayerDataFromClientId(serverRpcParams.Receive.SenderClientId).playerMoveAnimState == PlayerMoveAnimState.GameOver) return;
        if (moveDir != Vector3.zero)
            GameMultiplayer.Instance.UpdatePlayerMoveAnimStateOnServer(serverRpcParams.Receive.SenderClientId, PlayerMoveAnimState.Walking);
        else
            GameMultiplayer.Instance.UpdatePlayerMoveAnimStateOnServer(serverRpcParams.Receive.SenderClientId, PlayerMoveAnimState.Idle);

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
}
