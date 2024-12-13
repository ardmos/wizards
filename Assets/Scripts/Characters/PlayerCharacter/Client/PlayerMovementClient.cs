using Unity.Netcode;
using UnityEngine;
using static ComponentValidator;

/// <summary>
/// Ŭ���̾�Ʈ ������ �÷��̾��� �̵��� ó���ϴ� ������Ʈ�Դϴ�.
/// ���� ���� ������� ó���ϴ� �÷��̾� �̵��� Ŭ���̾�Ʈ �����Դϴ�.
/// </summary>
public class PlayerMovementClient : NetworkBehaviour
{
    #region Constants & Fields
    // ���� �޼��� �����...
    private const string ERROR_PLAYER_MOVEMENT_SERVER_NOT_SET = "PlayerMovementClient playerMovementServer ������ �ȵǾ��ֽ��ϴ�.";
    private const string ERROR_GAME_INPUT_NOT_SET = "PlayerMovementClient gameInput ������ �ȵǾ��ֽ��ϴ�.";

    [SerializeField] private GameInput gameInput;
    [SerializeField] private PlayerMovementServer playerMovementServer;
    #endregion

    #region Unity Lifecycle
    /// <summary>
    /// NetworkBehaviour.IsOwner���� ������ �� ������Ʈ�� �����ڸ� �Է��� ó���ϵ��� �մϴ�.
    /// </summary>
    private void Update()
    {
        if (!IsOwner) return;

        HandleMovementServerAuth();
    }
    #endregion

    #region Movement Handling
    /// <summary>
    /// �Է��� �޾� ���� RPC�� ȣ���ϴ� �޼����Դϴ�.
    /// ���� ���� ������� �÷��̾��� �̵��� ó���մϴ�.
    /// </summary>
    public void HandleMovementServerAuth()
    {
        if (!ValidateComponent(gameInput, ERROR_GAME_INPUT_NOT_SET)) return;
        if (!ValidateComponent(playerMovementServer, ERROR_PLAYER_MOVEMENT_SERVER_NOT_SET)) return;

        Vector2 inputVector = gameInput.GetMovementVectorNormalized();
        playerMovementServer.HandleMovementServerRPC(inputVector, gameInput.GetIsAttackButtonClicked());
    }

    /// <summary>
    /// GamePad UI�� ��ų��ư �巡�׸� ���� �÷��̾ ȸ����Ű�� �޼����Դϴ�.
    /// </summary>
    /// <param name="dir">ȸ�� ����</param>
    public void RotateByDragSpellButton(Vector3 dir)
    {
        if (!ValidateComponent(playerMovementServer, ERROR_PLAYER_MOVEMENT_SERVER_NOT_SET)) return;

        playerMovementServer.RotateByDragSpellButtonServerRPC(dir);
    }
    #endregion
}