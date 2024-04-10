using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 플레이어 프리팹에 붙어있는 Input 관리 스크립트
/// </summary>
public class GameInput : MonoBehaviour
{
    private PlayerInputActions playerInputActions;

    public event EventHandler OnAttack1Started;
    public event EventHandler OnAttack2Started;
    public event EventHandler OnAttack3Started;
    public event EventHandler OnAttack1Ended;
    public event EventHandler OnAttack2Ended;
    public event EventHandler OnAttack3Ended;
    public event EventHandler OnDefenceClicked;

    public bool isPlayerControllable;

    private void Awake()
    {
        isPlayerControllable = true;

        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();
        playerInputActions.Player.Attack1.started += Attack1_started;
        playerInputActions.Player.Attack1.canceled += Attack1_canceled;
        playerInputActions.Player.Attack2.started += Attack2_started;
        playerInputActions.Player.Attack2.canceled += Attack2_canceled;
        playerInputActions.Player.Attack3.started += Attack3_started;
        playerInputActions.Player.Attack3.canceled += Attack3_canceled;
        playerInputActions.Player.Defence.started += Defence_started;

        GetComponent<Player>().OnPlayerGameOver += OnPlayerGameOver;
    }

    private void OnDestroy()
    {
        playerInputActions.Player.Attack1.started -= Attack1_started;
        playerInputActions.Player.Attack1.canceled -= Attack1_canceled;
        playerInputActions.Player.Attack2.started -= Attack2_started;
        playerInputActions.Player.Attack2.canceled -= Attack2_canceled;
        playerInputActions.Player.Attack3.started -= Attack3_started;
        playerInputActions.Player.Attack3.canceled -= Attack3_canceled;

        playerInputActions.Player.Defence.started -= Defence_started;

        playerInputActions.Dispose();
    }

    public Vector2 GetMovementVectorNormalized()
    {
        if (!isPlayerControllable) return Vector2.zero;

        Vector2 inputVector = playerInputActions.Player.Movement.ReadValue<Vector2>();

        inputVector = inputVector.normalized;

        return inputVector;
    }

    private void Defence_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (!isPlayerControllable) return;
        OnDefenceClicked?.Invoke(this, EventArgs.Empty);
    }

    private void Attack1_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (!isPlayerControllable) return;
        OnAttack1Started?.Invoke(this, EventArgs.Empty);
    }

    private void Attack1_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (!isPlayerControllable) return;
        OnAttack1Ended?.Invoke(this, EventArgs.Empty);
    }

    private void Attack2_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (!isPlayerControllable) return;
        OnAttack2Started?.Invoke(this, EventArgs.Empty);
    }

    private void Attack2_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (!isPlayerControllable) return;
        OnAttack2Ended?.Invoke(this, EventArgs.Empty);
    }

    private void Attack3_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (!isPlayerControllable) return;
        OnAttack3Started?.Invoke(this, EventArgs.Empty);
    }

    private void Attack3_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (!isPlayerControllable) return;
        OnAttack3Ended?.Invoke(this, EventArgs.Empty);
    }

    private void OnPlayerGameOver(object sender, EventArgs e)
    {
        isPlayerControllable = false;
    }
}
