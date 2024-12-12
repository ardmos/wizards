using Unity.Netcode;
using UnityEngine;
using static ComponentValidator;

/// <summary>
/// 클라이언트 측에서 플레이어의 이동을 처리하는 컴포넌트입니다.
/// 서버 권한 방식으로 처리하는 플레이어 이동의 클라이언트 로직입니다.
/// </summary>
public class PlayerMovementClient : NetworkBehaviour
{
    #region Constants & Fields
    // 에러 메세지 상수들...
    private const string ERROR_PLAYER_MOVEMENT_SERVER_NOT_SET = "PlayerMovementClient playerMovementServer 설정이 안되어있습니다.";
    private const string ERROR_GAME_INPUT_NOT_SET = "PlayerMovementClient gameInput 설정이 안되어있습니다.";

    [SerializeField] private GameInput gameInput;
    [SerializeField] private PlayerMovementServer playerMovementServer;
    #endregion

    #region Unity Lifecycle
    /// <summary>
    /// NetworkBehaviour.IsOwner값을 검증해 현 컴포넌트의 소유자만 입력을 처리하도록 합니다.
    /// </summary>
    private void Update()
    {
        if (!IsOwner) return;

        HandleMovementServerAuth();
    }
    #endregion

    #region Movement Handling
    /// <summary>
    /// 입력을 받아 서버 RPC를 호출하는 메서드입니다.
    /// 서버 권한 방식으로 플레이어의 이동을 처리합니다.
    /// </summary>
    public void HandleMovementServerAuth()
    {
        if (!ValidateComponent(gameInput, ERROR_GAME_INPUT_NOT_SET)) return;
        if (!ValidateComponent(playerMovementServer, ERROR_PLAYER_MOVEMENT_SERVER_NOT_SET)) return;

        Vector2 inputVector = gameInput.GetMovementVectorNormalized();
        playerMovementServer.HandleMovementServerRPC(inputVector, gameInput.GetIsAttackButtonClicked());
    }

    /// <summary>
    /// GamePad UI의 스킬버튼 드래그를 통해 플레이어를 회전시키는 메서드입니다.
    /// </summary>
    /// <param name="dir">회전 방향</param>
    public void RotateByDragSpellButton(Vector3 dir)
    {
        if (!ValidateComponent(playerMovementServer, ERROR_PLAYER_MOVEMENT_SERVER_NOT_SET)) return;

        playerMovementServer.RotateByDragSpellButtonServerRPC(dir);
    }
    #endregion
}