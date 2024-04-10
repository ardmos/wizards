using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 이동속도(moveSpeen)는 서버측에서 관리. 
/// </summary>
public class PlayerMovementServer : NetworkBehaviour
{
    [SerializeField] protected float moveSpeed;

    [ServerRpc (RequireOwnership = false)]
    public void HandleMovementServerRPC(Vector2 inputVector, bool isAttackButtonClicked, ServerRpcParams serverRpcParams = default)
    {
        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);

        // 서버권한방식의 네트워크에서 이동처리 할 때 서버의 DeltaTime이 클라이언트의 델타타임과는 다른 경우가 생김. 따라서 아래처럼 수정해야함
        //float moveDistance = moveSpeed * Time.deltaTime;
        float moveDistance = moveSpeed * NetworkManager.Singleton.ServerTime.FixedDeltaTime;
        transform.position += moveDir * moveDistance;

        // 서버(GameMultiplayer)에 새로운 Player Anim State 저장. (GameOver상태가 아닐 때에만!)
        if (GameMultiplayer.Instance.GetPlayerDataFromClientId(serverRpcParams.Receive.SenderClientId).playerMoveAnimState == PlayerMoveAnimState.GameOver) return;
        if (moveDir != Vector3.zero)
            GameMultiplayer.Instance.UpdatePlayerMoveAnimStateOnServer(serverRpcParams.Receive.SenderClientId, PlayerMoveAnimState.Walking);
        else
            GameMultiplayer.Instance.UpdatePlayerMoveAnimStateOnServer(serverRpcParams.Receive.SenderClientId, PlayerMoveAnimState.Idle);

        // 공격중이 아닐 때에만 진행방향으로 캐릭터 회전
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
