using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 플레이어 프리팹에 붙어있는 Input 관리 스크립트
/// </summary>
public class GameInput : MonoBehaviour
{
    public event EventHandler OnAttack1Started;
    public event EventHandler OnAttack2Started;
    public event EventHandler OnAttack3Started;
    public event EventHandler OnAttack1Ended;
    public event EventHandler OnAttack2Ended;
    public event EventHandler OnAttack3Ended;
    public event EventHandler OnDefenceStarted;
    public event EventHandler OnDefenceEnded;

    private bool isPlayerControllable;
    private bool isAttackButtonClicked;

    private PlayerInputActions playerInputActions;



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
        playerInputActions.Player.Defence.canceled += Defence_canceled;

        GetComponent<PlayerClient>().OnPlayerGameOver += OnPlayerGameOver;
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
        playerInputActions.Player.Defence.canceled -= Defence_canceled;

        GetComponent<PlayerClient>().OnPlayerGameOver -= OnPlayerGameOver;

        playerInputActions.Dispose();
    }

    public Vector2 GetMovementVectorNormalized()
    {
        if (!isPlayerControllable) return Vector2.zero;

        Vector2 inputVector = playerInputActions.Player.Movement.ReadValue<Vector2>();

        inputVector = inputVector.normalized;

        return inputVector;
    }

    public bool GetIsAttackButtonClicked()
    {
        return isAttackButtonClicked;
    }

    private void Attack1_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (!isPlayerControllable) return;
        isAttackButtonClicked = true;
        OnAttack1Started?.Invoke(this, EventArgs.Empty);
    }
    private void Attack2_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (!isPlayerControllable) return;
        isAttackButtonClicked = true;
        OnAttack2Started?.Invoke(this, EventArgs.Empty);
    }
    private void Attack3_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (!isPlayerControllable) return;
        isAttackButtonClicked = true;
        OnAttack3Started?.Invoke(this, EventArgs.Empty);
    }
    private void Defence_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (!isPlayerControllable) return;
        OnDefenceStarted?.Invoke(this, EventArgs.Empty);
    }

    private void Attack1_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (!isPlayerControllable) return;
        if (!isAttackButtonClicked) return;
        isAttackButtonClicked = false;
        OnAttack1Ended?.Invoke(this, EventArgs.Empty);
    }
    private void Attack2_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (!isPlayerControllable) return;
        if (!isAttackButtonClicked) return;
        isAttackButtonClicked = false;
        OnAttack2Ended?.Invoke(this, EventArgs.Empty);
    }
    private void Attack3_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (!isPlayerControllable) return;
        if (!isAttackButtonClicked) return;
        isAttackButtonClicked = false;
        OnAttack3Ended?.Invoke(this, EventArgs.Empty);
    }
    private void Defence_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (!isPlayerControllable) return;
        OnDefenceEnded?.Invoke(this, EventArgs.Empty);
    }

    private void OnPlayerGameOver(object sender, EventArgs e)
    {
        isPlayerControllable = false;
    }
}
