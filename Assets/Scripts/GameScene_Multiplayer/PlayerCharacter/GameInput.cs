using System;
using UnityEngine;

/// <summary>
/// 플레이어의 입력을 관리하는 컴포넌트입니다.
/// New Input System을 사용하여 다양한 입력 이벤트를 처리합니다.
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

    /// <summary>
    /// 플레이어의 이동 입력을 정규화된 벡터로 반환합니다.
    /// </summary>
    /// <returns>정규화된 이동 벡터</returns>
    public Vector2 GetMovementVectorNormalized()
    {
        if (!isPlayerControllable) return Vector2.zero;

        Vector2 inputVector = playerInputActions.Player.Movement.ReadValue<Vector2>();

        inputVector = inputVector.normalized;

        return inputVector;
    }

    /// <summary>
    /// 공격 버튼이 클릭되었는지 여부를 반환합니다.
    /// </summary>
    /// <returns>공격 버튼 클릭 상태</returns>
    public bool GetIsAttackButtonClicked()
    {
        return isAttackButtonClicked;
    }

    /// <summary>
    /// Attack1 버튼이 눌렸을 때 호출되는 메서드입니다.
    /// </summary>
    private void Attack1_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (!isPlayerControllable) return;
        isAttackButtonClicked = true;
        OnAttack1Started?.Invoke(this, EventArgs.Empty);
    }
    /// <summary>
    /// Attack2 버튼이 눌렸을 때 호출되는 메서드입니다.
    /// </summary>
    private void Attack2_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (!isPlayerControllable) return;
        isAttackButtonClicked = true;
        OnAttack2Started?.Invoke(this, EventArgs.Empty);
    }
    /// <summary>
    /// Attack3 버튼이 눌렸을 때 호출되는 메서드입니다.
    /// </summary>
    private void Attack3_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (!isPlayerControllable) return;
        isAttackButtonClicked = true;
        OnAttack3Started?.Invoke(this, EventArgs.Empty);
    }
    /// <summary>
    /// Defence 버튼이 눌렸을 때 호출되는 메서드입니다.
    /// </summary>
    private void Defence_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (!isPlayerControllable) return;
        isAttackButtonClicked = true;
        OnDefenceStarted?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Attack1 버튼이 떼졌을 때 호출되는 메서드입니다.
    /// </summary>
    private void Attack1_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (!isPlayerControllable) return;
        if (!isAttackButtonClicked) return;
        isAttackButtonClicked = false;
        OnAttack1Ended?.Invoke(this, EventArgs.Empty);
    }
    /// <summary>
    /// Attack2 버튼이 떼졌을 때 호출되는 메서드입니다.
    /// </summary>
    private void Attack2_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (!isPlayerControllable) return;
        if (!isAttackButtonClicked) return;
        isAttackButtonClicked = false;
        OnAttack2Ended?.Invoke(this, EventArgs.Empty);
    }
    /// <summary>
    /// Attack3 버튼이 떼졌을 때 호출되는 메서드입니다.
    /// </summary>
    private void Attack3_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (!isPlayerControllable) return;
        if (!isAttackButtonClicked) return;
        isAttackButtonClicked = false;
        OnAttack3Ended?.Invoke(this, EventArgs.Empty);
    }
    /// <summary>
    /// Defence 버튼이 떼졌을 때 호출되는 메서드입니다.
    /// </summary>
    private void Defence_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (!isPlayerControllable) return;
        if (!isAttackButtonClicked) return;
        isAttackButtonClicked = false;
        OnDefenceEnded?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// 플레이어 게임 오버 시 호출되는 메서드입니다.
    /// </summary>
    private void OnPlayerGameOver(object sender, EventArgs e)
    {
        isPlayerControllable = false;
    }
}
