using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 클라이언트 측에서 플레이어의 이동을 처리하는 컴포넌트입니다.
/// 서버 권한 방식으로 이동을 처리합니다.
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
        // NetworkBehaviour.IsOwner값을 검증해 현 컴포넌트의 소유자만 입력을 처리하도록 합니다.
        if (!IsOwner) return;

        HandleMovementServerAuth();
    }
    #endregion

    #region Movement Handling
    /// <summary>
    /// GamePad UI 스킬버튼 드래그에서 호출하여 플레이어를 회전시키는 메서드입니다.
    /// </summary>
    /// <param name="dir">회전 방향</param>
    public void RotateByDragSpellButton(Vector3 dir)
    {
        playerMovementServer.RotateByDragSpellButtonServerRPC(dir);
    }

    /// <summary>
    /// 입력을 받아 서버 RPC를 호출하는 메서드입니다.
    /// 서버 권한 방식으로 플레이어의 이동을 처리합니다.
    /// </summary>
    public void HandleMovementServerAuth()
    {
        Vector2 inputVector = gameInput.GetMovementVectorNormalized();
        playerMovementServer.HandleMovementServerRPC(inputVector, gameInput.GetIsAttackButtonClicked());
    }
    #endregion
}


