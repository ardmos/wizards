using Unity.Netcode;
using UnityEngine;
/// <summary>
/// Player HP��  Server Auth ������� �����Ҽ� �ֵ��� �����ִ� ��ũ��Ʈ �Դϴ�.
/// �������� �����մϴ�.
/// </summary>
public class PlayerHPManager : NetworkBehaviour
{
    public static PlayerHPManager Instance {  get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// �������� ȣ���ؾ��ϴ� �޼���. �������� �����մϴ�.
    /// </summary>
    public void UpdatePlayerHP(ulong clientId, sbyte currentHP, sbyte maxHP)
    {
        if (!NetworkManager.ConnectedClients.ContainsKey(clientId))
        {
            Debug.Log($"{nameof(UpdatePlayerHP)} �������� �ʴ� ClientId���Դϴ�.");
            return;
        }


        // ����� HP�� ������ ����
        PlayerInGameData playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(clientId);
        playerData.hp = currentHP;
        playerData.maxHp = maxHP;
        GameMultiplayer.Instance.SetPlayerDataFromClientId(clientId, playerData);

        // �� Client �÷��̾��� HP�� UI ������Ʈ
        NetworkClient networkClient = NetworkManager.ConnectedClients[clientId];
        networkClient.PlayerObject.GetComponent<PlayerClient>().SetHPClientRPC(playerData.hp, playerData.maxHp);
        

        // ���ӿ��� ó��. �������� ���.
        if (currentHP <= 0)
        {
            // ���ӿ��� �÷��̾� ����� ������ ���.
            GameManager.Instance.UpdatePlayerGameOverOnServer(clientId);

            // GameOver �÷��̾� AnimState GameOver ���·� ������ ���. ���ӿ��� �ִϸ��̼� ����. 
            GameMultiplayer.Instance.UpdatePlayerMoveAnimStateOnServer(clientId, PlayerMoveAnimState.GameOver);

            // �ش� �÷��̾� ���� �Ұ� ó�� �� ���ӿ��� �˾� ����.
            networkClient.PlayerObject.GetComponent<PlayerClient>().SetPlayerGameOverClientRPC();
        }

    }
}