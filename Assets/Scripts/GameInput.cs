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
        // Clear Statics�� �Ϻ�. ��ǲ �׼��� �߰��Ǹ� ���⼭ ���� -= ����� �Ѵ�. 
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
