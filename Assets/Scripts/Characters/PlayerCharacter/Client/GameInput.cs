using System;
using UnityEngine;
using UnityEngine.InputSystem;
using static ComponentValidator;

/// <summary>
/// �÷��̾��� �Է��� �����ϴ� ������Ʈ�Դϴ�.
/// New Input System�� ����Ͽ� �پ��� �Է� �̺�Ʈ�� ó���մϴ�.
/// </summary>
public class GameInput : MonoBehaviour
{
    #region Events
    /// <summary>
    /// ���� ���� �̺�Ʈ. int �Ű������� ���� Ÿ���� ��Ÿ���ϴ� (1, 2, 3).
    /// </summary>
    public event EventHandler<int> OnAttackStarted;
    /// <summary>
    /// ���� ���� �̺�Ʈ. int �Ű������� ���� Ÿ���� ��Ÿ���ϴ� (1, 2, 3).
    /// </summary>
    public event EventHandler<int> OnAttackEnded;
    // ��� ���� �̺�Ʈ.
    public event EventHandler OnDefenceStarted;
    // ��� ���� �̺�Ʈ.
    public event EventHandler OnDefenceEnded;
    #endregion

    #region Constants & Fields
    // ���� �޼��� �����...
    private const string ERROR_PLAYER_CLIENT_NOT_SET = "GameInput playerClient ������ �ȵǾ��ֽ��ϴ�.";
    private const string ERROR_PLAYER_INPUT_ACTIONS_NOT_SET = "GameInput playerInputActions ������ �ȵǾ��ֽ��ϴ�.";

    [SerializeField] private PlayerClient playerClient;
    private bool isPlayerControllable;
    private bool isAttackButtonClicked;
    private PlayerInputActions playerInputActions;
    #endregion

    #region Unity Lifecycle
    /// <summary>
    /// Unity�� Awake �޼����Դϴ�. ������Ʈ �ʱ�ȭ�� �����մϴ�.
    /// </summary>
    private void Awake()
    {
        if (!ValidateComponent(playerClient, ERROR_PLAYER_CLIENT_NOT_SET)) return;

        SetupInputActions();
        isPlayerControllable = true;
        playerClient.OnPlayerGameOver += OnPlayerGameOver;
    }

    /// <summary>
    /// Unity�� OnDestroy �޼����Դϴ�. ������Ʈ ���� �۾��� �����մϴ�.
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
    /// �÷��̾��� �̵� �Է��� ����ȭ�� ���ͷ� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>����ȭ�� �̵� ����</returns>
    public Vector2 GetMovementVectorNormalized()
    {
        if (!isPlayerControllable) return Vector2.zero;
        if (!ValidateComponent(playerInputActions, ERROR_PLAYER_INPUT_ACTIONS_NOT_SET)) return Vector2.zero;

        Vector2 inputVector = playerInputActions.Player.Movement.ReadValue<Vector2>();
        inputVector = inputVector.normalized;
        return inputVector;
    }

    /// <summary>
    /// ���� ��ư�� Ŭ���Ǿ����� ���θ� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>���� ��ư Ŭ�� ����</returns>
    public bool GetIsAttackButtonClicked()
    {
        return isAttackButtonClicked;
    }
    #endregion

    #region Input Handling
    /// <summary>
    /// �Է� �׼��� �����ϰ� �̺�Ʈ �ڵ鷯�� ����մϴ�.
    /// �� �޼���� ������Ʈ �ʱ�ȭ �� ȣ��Ǿ�� �մϴ�.
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
    /// ��ϵ� �Է� �׼� �̺�Ʈ �ڵ鷯�� �����մϴ�.
    /// �� �޼���� ������Ʈ ���� �� ȣ��Ǿ�� �ϸ�, �޸� ������ �����մϴ�.
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
    /// ���� ��ư�� ������ �� ȣ��Ǵ� �޼����Դϴ�.
    /// </summary>
    /// <param name="attackType">���� Ÿ�� (1, 2, 3)</param>
    private void HandleAttackStarted(int attackType)
    {
        if (!isPlayerControllable) return;
        isAttackButtonClicked = true;
        OnAttackStarted?.Invoke(this, attackType);
    }

    /// <summary>
    /// ���� ��ư�� ������ �� ȣ��Ǵ� �޼����Դϴ�.
    /// </summary>
    /// <param name="attackType">���� Ÿ�� (1, 2, 3)</param>
    private void HandleAttackEnded(int attackType)
    {
        if (!isPlayerControllable || !isAttackButtonClicked) return;
        isAttackButtonClicked = false;
        OnAttackEnded?.Invoke(this, attackType);
    }

    /// <summary>
    /// Defence ��ư�� ������ �� ȣ��Ǵ� �޼����Դϴ�.
    /// </summary>
    private void Defence_started(InputAction.CallbackContext context)
    {
        if (!isPlayerControllable) return;
        isAttackButtonClicked = true;
        OnDefenceStarted?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Defence ��ư�� ������ �� ȣ��Ǵ� �޼����Դϴ�.
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
    /// �÷��̾� ���� ���� �� ȣ��Ǵ� �޼����Դϴ�.
    /// </summary>
    private void OnPlayerGameOver(object sender, EventArgs e)
    {
        isPlayerControllable = false;
    }
    #endregion
}