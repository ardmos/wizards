using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInput : MonoBehaviour
{
    private PlayerInputActions playerInputActions;


    private void Awake()
    {
        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();
    }

    private void OnDestroy()
    {
        // 재접속 구현의 일부 인풋 액션이 추가되면 여기서 전부 -= 해줘야 한다. 
        playerInputActions.Dispose();
    }

    public Vector2 GetMovementVectorNormalized()
    {
        Vector2 inputVector = playerInputActions.Player.Movement.ReadValue<Vector2>();

        inputVector = inputVector.normalized;

        return inputVector;
    }

    public float GetIsAttack1()
    {
        float isAttack = playerInputActions.Player.Attack1.ReadValue<float>();


        // 스킬 드래그 시스템 만들기!!  여기부터 시작하면 될듯. started 콜백을 이용해서 eventHander를 Invoke하고, 이 eventHandler를 통해 옵저버 패턴을 구현하면 되겠다.
        // 스킬 터치 start 단계 -> 드래그 UI ON! (이 때 Player는 회전을 중지한다.)
        // 드래그 UI 하는 일. 
        // 1. 드래그(터치) 방향에 따라 UI회전.
        // 회전값을 Player오브젝트에도 반영. 회전 시킨다.
        // 스킬 터치 Performed? or Cancled? 둘 중 하나가 정상종료이면, 취소와 함께 해야함. 어쨌든 끝마녀 다시 Player는 이동방향을 바라보는 회전을 시작한다.
        playerInputActions.Player.Attack1.started += Attack1_started;
        Debug.Log($"isAttack.ReadValue: {isAttack}, " +
            $"Attack1.IsPressed? {playerInputActions.Player.Attack1.IsPressed()}, " +
            $"Attack1.IsInProgress? {playerInputActions.Player.Attack1.IsInProgress()}");
        return isAttack;
    }

    private void Attack1_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        throw new System.NotImplementedException();
    }

    public float GetIsAttack2()
    {
        float isAttack = playerInputActions.Player.Attack2.ReadValue<float>();
        return isAttack;
    }
    public float GetIsAttack3()
    {
        float isAttack = playerInputActions.Player.Attack3.ReadValue<float>();
        return isAttack;
    }
}
