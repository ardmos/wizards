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
        // Clear Statics의 일부. 인풋 액션이 추가되면 여기서 전부 -= 해줘야 한다. 
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
        return isAttack;
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
