using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerMovementClient : MonoBehaviour
{
    /*private GameInput gameInput;
    private PlayerMovementServer playerMovementServer;

    private void Awake()
    {
        gameInput = GetComponent<GameInput>();      
        playerMovementServer = GetComponent<PlayerMovementServer>();
    }

    // Server Auth 방식의 이동 처리 (현 오브젝트에 Network Transform이 필요)
    protected void HandleMovementServerAuth()
    {
        if (Player.Instance.GetPlayerGameOver()) return;

        Vector2 inputVector = gameInput.GetMovementVectorNormalized();
        playerMovementServer.HandleMovementServerRPC(inputVector, isBtnAttack1Clicked, isBtnAttack2Clicked, isBtnAttack3Clicked);
    }
    
    /// <summary>
    /// GamePad UI 스킬버튼 드래그에서 호출하여 플레이어를 회전시키는 메소드.
    /// </summary>
    public void RotateByDragSpellBtn(Vector3 dir)
    {
        //Debug.Log($"RotateByDragSpellBtn dir:{dir}");
        RotateByDragSpellBtnServerRPC(dir);
    }


    private void Rotate(Vector3 moveDir)
    {
        if (moveDir == Vector3.zero) return;

        float rotateSpeed = 30f;
        Vector3 slerpResult = Vector3.Slerp(transform.forward, moveDir, Time.deltaTime * rotateSpeed);
        transform.forward = slerpResult;
    }*/
}


