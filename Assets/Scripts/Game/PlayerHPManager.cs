using Unity.Netcode;
/// <summary>
/// ������ Server Auth ������� ������ �� �ֵ��� �����ִ� ��ũ��Ʈ �Դϴ�.
/// </summary>
public class PlayerHPManager : NetworkBehaviour
{
    public static PlayerHPManager Instance {  get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void SetPlayerHP(sbyte playerHP)
    {
        SetPlayerHPServerRPC(playerHP);
    }
    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerHPServerRPC(sbyte playerHP, ServerRpcParams serverRpcParams = default)
    {
        var clientId = serverRpcParams.Receive.SenderClientId;
        if (NetworkManager.ConnectedClients.ContainsKey(clientId))
        {
            UpdatePlayerHP(clientId, playerHP);
        }
    }

    /// <summary>
    /// �������� ȣ���ؾ��ϴ� �޼���.
    /// </summary>
    /// <param name="clientId"></param>
    /// <param name="playerTotalHP"></param>
    public void UpdatePlayerHP(ulong clientId, sbyte playerTotalHP)
    {
        // ����� HP�� ������ ����
        PlayerData playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(clientId);
        playerData.playerHP = playerTotalHP;
        GameMultiplayer.Instance.SetPlayerDataFromClientId(clientId, playerData);

        // �� Client �÷��̾��� HP�� UI ������Ʈ
        NetworkClient networkClient = NetworkManager.ConnectedClients[clientId];
        networkClient.PlayerObject.GetComponent<Player>().SetHPClientRPC(playerTotalHP);
    }
}