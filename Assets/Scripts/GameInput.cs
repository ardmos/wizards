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
        // ������ ������ �Ϻ� ��ǲ �׼��� �߰��Ǹ� ���⼭ ���� -= ����� �Ѵ�. 
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


        // ��ų �巡�� �ý��� �����!!  ������� �����ϸ� �ɵ�. started �ݹ��� �̿��ؼ� eventHander�� Invoke�ϰ�, �� eventHandler�� ���� ������ ������ �����ϸ� �ǰڴ�.
        // ��ų ��ġ start �ܰ� -> �巡�� UI ON! (�� �� Player�� ȸ���� �����Ѵ�.)
        // �巡�� UI �ϴ� ��. 
        // 1. �巡��(��ġ) ���⿡ ���� UIȸ��.
        // ȸ������ Player������Ʈ���� �ݿ�. ȸ�� ��Ų��.
        // ��ų ��ġ Performed? or Cancled? �� �� �ϳ��� ���������̸�, ��ҿ� �Բ� �ؾ���. ��·�� ������ �ٽ� Player�� �̵������� �ٶ󺸴� ȸ���� �����Ѵ�.
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
