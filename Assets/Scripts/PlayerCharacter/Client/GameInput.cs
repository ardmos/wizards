using System;
using UnityEngine;
using UnityEngine.InputSystem;
using static ComponentValidator;

/// <summary>
/// 플레이어의 입력을 관리하는 컴포넌트입니다.
/// New Input System을 사용하여 다양한 입력 이벤트를 처리합니다.
/// </summary>
public class GameInput : MonoBehaviour
{
    #region Events
    /// <summary>
    /// 공격 시작 이벤트. int 매개변수는 공격 타입을 나타냅니다 (1, 2, 3).
    /// </summary>
    public event EventHandler<int> OnAttackStarted;
    /// <summary>
    /// 공격 종료 이벤트. int 매개변수는 공격 타입을 나타냅니다 (1, 2, 3).
    /// </summary>
    public event EventHandler<int> OnAttackEnded;
    // 방어 시작 이벤트.
    public event EventHandler OnDefenceStarted;
    // 방어 종료 이벤트.
    public event EventHandler OnDefenceEnded;
    #endregion

    #region Constants & Fields
    // 에러 메세지 상수들...
    private const string ERROR_PLAYER_CLIENT_NOT_SET = "GameInput playerClient 설정이 안되어있습니다.";
    private const string ERROR_PLAYER_INPUT_ACTIONS_NOT_SET = "GameInput playerInputActions 설정이 안되어있습니다.";

    [SerializeField] private PlayerClient playerClient;
    private bool isPlayerControllable;
    private bool isAttackButtonClicked;
    private PlayerInputActions playerInputActions;
    #endregion

    #region Unity Lifecycle
    /// <summary>
    /// Unity의 Awake 메서드입니다. 컴포넌트 초기화를 수행합니다.
    /// </summary>
    private void Awake()
    {
        if (!ValidateComponent(playerClient, ERROR_PLAYER_CLIENT_NOT_SET)) return;

        SetupInputActions();
        isPlayerControllable = true;
        playerClient.OnPlayerGameOver += OnPlayerGameOver;
    }

    /// <summary>
    /// Unity의 OnDestroy 메서드입니다. 컴포넌트 정리 작업을 수행합니다.
    /// </summary>
    private void OnDestroy()
    {
        if (!ValidateComponent(playerInputActions, ERROR_PLAYER_INPUT_ACTIONS_NOT_SET)) return;
        if (!ValidateComponent(playerClient, ERROR_PLAYER_CLIENT_NOT_SET)) return;

        CleanupInputActions();
        playerClient.OnPlayerGameOver -= OnPlayerGameOver;
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// 플레이어의 이동 입력을 정규화된 벡터로 반환합니다.
    /// </summary>
    /// <returns>정규화된 이동 벡터</returns>
    public Vector2 GetMovementVectorNormalized()
    {
        if (!isPlayerControllable) return Vector2.zero;
        if (!ValidateComponent(playerInputActions, ERROR_PLAYER_INPUT_ACTIONS_NOT_SET)) return Vector2.zero;

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
    #endregion

    #region Input Handling
    /// <summary>
    /// 입력 액션을 설정하고 이벤트 핸들러를 등록합니다.
    /// 이 메서드는 컴포넌트 초기화 시 호출되어야 합니다.
    /// </summary>
    private void SetupInputActions()
    {
        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();
        playerInputActions.Player.Attack1.started += _ => HandleAttackStarted(1);
        playerInputActions.Player.Attack1.canceled += _ => HandleAttackEnded(1);
        playerInputActions.Player.Attack2.started += _ => HandleAttackStarted(2);
        playerInputActions.Player.Attack2.canceled += _ => HandleAttackEnded(2);
        playerInputActions.Player.Attack3.started += _ => HandleAttackStarted(3);
        playerInputActions.Player.Attack3.canceled += _ => HandleAttackEnded(3);
        playerInputActions.Player.Defence.started += Defence_started;
        playerInputActions.Player.Defence.canceled += Defence_canceled;
    }

    /// <summary>
    /// 등록된 입력 액션 이벤트 핸들러를 제거합니다.
    /// 이 메서드는 컴포넌트 정리 시 호출되어야 하며, 메모리 누수를 방지합니다.
    /// </summary>
    private void CleanupInputActions()
    {
        playerInputActions.Player.Attack1.started -= _ => HandleAttackStarted(1);
        playerInputActions.Player.Attack1.canceled -= _ => HandleAttackEnded(1);
        playerInputActions.Player.Attack2.started -= _ => HandleAttackStarted(2);
        playerInputActions.Player.Attack2.canceled -= _ => HandleAttackEnded(2);
        playerInputActions.Player.Attack3.started -= _ => HandleAttackStarted(3);
        playerInputActions.Player.Attack3.canceled -= _ => HandleAttackEnded(3);
        playerInputActions.Player.Defence.started -= Defence_started;
        playerInputActions.Player.Defence.canceled -= Defence_canceled;
        playerInputActions.Dispose();
    }

    /// <summary>
    /// 공격 버튼이 눌렸을 때 호출되는 메서드입니다.
    /// </summary>
    /// <param name="attackType">공격 타입 (1, 2, 3)</param>
    private void HandleAttackStarted(int attackType)
    {
        if (!isPlayerControllable) return;
        isAttackButtonClicked = true;
        OnAttackStarted?.Invoke(this, attackType);
    }

    /// <summary>
    /// 공격 버튼이 떼졌을 때 호출되는 메서드입니다.
    /// </summary>
    /// <param name="attackType">공격 타입 (1, 2, 3)</param>
    private void HandleAttackEnded(int attackType)
    {
        if (!isPlayerControllable || !isAttackButtonClicked) return;
        isAttackButtonClicked = false;
        OnAttackEnded?.Invoke(this, attackType);
    }

    /// <summary>
    /// Defence 버튼이 눌렸을 때 호출되는 메서드입니다.
    /// </summary>
    private void Defence_started(InputAction.CallbackContext context)
    {
        if (!isPlayerControllable) return;
        isAttackButtonClicked = true;
        OnDefenceStarted?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Defence 버튼이 떼졌을 때 호출되는 메서드입니다.
    /// </summary>
    private void Defence_canceled(InputAction.CallbackContext context)
    {
        if (!isPlayerControllable || !isAttackButtonClicked) return;
        isAttackButtonClicked = false;
        OnDefenceEnded?.Invoke(this, EventArgs.Empty);
    }
    #endregion

    #region Event Handlers
    /// <summary>
    /// 플레이어 게임 오버 시 호출되는 메서드입니다.
    /// </summary>
    private void OnPlayerGameOver(object sender, EventArgs e)
    {
        isPlayerControllable = false;
    }
    #endregion
}