using Unity.Netcode;
using UnityEngine;

public class PlayerMovementClient : NetworkBehaviour
{
    private GameInput gameInput;
    private PlayerMovementServer playerMovementServer;

    private void Awake()
    {
        gameInput = GetComponent<GameInput>();
        playerMovementServer = GetComponent<PlayerMovementServer>();
        GetComponent<PlayerClient>().OnPlayerGameOver += OnPlayerGameOver;
    }

    private void Update()
    {
        if (!IsOwner) return;

        HandleMovementServerAuth();
    }

    private void OnDisable()
    {
        GetComponent<PlayerClient>().OnPlayerGameOver -= OnPlayerGameOver;
    }

    /// <summary>
    /// GamePad UI 스킬버튼 드래그에서 호출하여 플레이어를 회전시키는 메소드.
    /// </summary>
    public void RotateByDragSpellButton(Vector3 dir)
    {
        //Debug.Log($"RotateByDragSpellBtn dir:{dir}");
        playerMovementServer.RotateByDragSpellButtonServerRPC(dir);
    }

    // Server Auth 방식의 이동 처리 (현 오브젝트에 Network Transform이 필요)
    public void HandleMovementServerAuth()
    {
        Vector2 inputVector = gameInput.GetMovementVectorNormalized();
        playerMovementServer.HandleMovementServerRPC(inputVector, PlayerClient.Instance.GetComponent<GameInput>().GetIsAttackButtonClicked());
    }

    private void OnPlayerGameOver(object sender, System.EventArgs e)
    {
        playerMovementServer.HandleMovementServerRPC(Vector2.zero, PlayerClient.Instance.GetComponent<GameInput>().GetIsAttackButtonClicked());
    }
}


