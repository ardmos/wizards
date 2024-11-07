using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Ŭ���̾�Ʈ ������ �÷��̾��� �̵��� ó���ϴ� ������Ʈ�Դϴ�.
/// ���� ���� ������� �̵��� ó���մϴ�.
/// </summary>
public class PlayerMovementClient : NetworkBehaviour
{
    #region Fields
    [SerializeField] private GameInput gameInput;
    [SerializeField] private PlayerMovementServer playerMovementServer;
    #endregion

    #region Unity Lifecycle
    private void Update()
    {
        // NetworkBehaviour.IsOwner���� ������ �� ������Ʈ�� �����ڸ� �Է��� ó���ϵ��� �մϴ�.
        if (!IsOwner) return;

        HandleMovementServerAuth();
    }
    #endregion

    #region Movement Handling
    /// <summary>
    /// GamePad UI ��ų��ư �巡�׿��� ȣ���Ͽ� �÷��̾ ȸ����Ű�� �޼����Դϴ�.
    /// </summary>
    /// <param name="dir">ȸ�� ����</param>
    public void RotateByDragSpellButton(Vector3 dir)
    {
        playerMovementServer.RotateByDragSpellButtonServerRPC(dir);
    }

    /// <summary>
    /// �Է��� �޾� ���� RPC�� ȣ���ϴ� �޼����Դϴ�.
    /// ���� ���� ������� �÷��̾��� �̵��� ó���մϴ�.
    /// </summary>
    public void HandleMovementServerAuth()
    {
        Vector2 inputVector = gameInput.GetMovementVectorNormalized();
        playerMovementServer.HandleMovementServerRPC(inputVector, gameInput.GetIsAttackButtonClicked());
    }
    #endregion
}


