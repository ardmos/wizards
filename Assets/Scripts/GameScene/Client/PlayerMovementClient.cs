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
    /// GamePad UI ��ų��ư �巡�׿��� ȣ���Ͽ� �÷��̾ ȸ����Ű�� �޼ҵ�.
    /// </summary>
    public void RotateByDragSpellButton(Vector3 dir)
    {
        //Debug.Log($"RotateByDragSpellBtn dir:{dir}");
        playerMovementServer.RotateByDragSpellButtonServerRPC(dir);
    }

    // Server Auth ����� �̵� ó�� (�� ������Ʈ�� Network Transform�� �ʿ�)
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


