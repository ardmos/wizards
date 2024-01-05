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

    public void SetPlayerHPOnServer(sbyte playerHP, ulong clientId)
    {    
        if (NetworkManager.ConnectedClients.ContainsKey(clientId))
        {
            UpdatePlayerHP(clientId, playerHP);
        }
    }

    /// <summary>
    /// �������� ȣ���ؾ��ϴ� �޼���. �������� �����մϴ�.
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
        networkClient.PlayerObject.GetComponent<Player>().SetHPClientRPC(clientId, playerTotalHP);
        

        // ���ӿ��� ó��. �������� ���.
        if (playerTotalHP <= 0)
        {
            // ���ӿ��� �÷��̾� ���ӿ����� ��� ������ ���
            GameManager.Instance.UpdatePlayerGameOverListOnServer(clientId);

            // GameOver �÷��̾� AnimState ������ ���. 
            GameMultiplayer.Instance.UpdatePlayerAnimStateOnServer(clientId, PlayerMoveAnimState.GameOver);

            // �ش� �÷��̾� ���� �Ұ� ó��
            networkClient.PlayerObject.GetComponent<Player>().SetPlayerGameOverClientRPC();
        }

    }
}