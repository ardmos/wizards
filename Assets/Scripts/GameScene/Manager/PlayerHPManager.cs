using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;
/// <summary>
/// Player HP��  Server Auth ������� �����Ҽ� �ֵ��� �����ִ� ��ũ��Ʈ �Դϴ�.
/// �������� �����մϴ�.
/// </summary>
public class PlayerHPManager : NetworkBehaviour
{
/*    public static PlayerHPManager Instance {  get; private set; }

    private void Awake()
    {
        Instance = this;
    }*/

    public void InitPlayerHP(ICharacter character)
    {
        PlayerInGameData playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(OwnerClientId);
        playerData.hp = character.hp; 
        playerData.maxHp = character.maxHp;
        GameMultiplayer.Instance.SetPlayerDataFromClientId(OwnerClientId, playerData);

        GetComponent<PlayerClient>().SetHPClientRPC(playerData.hp, playerData.maxHp);
    }

    public void ApplyHeal(sbyte healingValue)
    {
        /// �����ϱ�   �� �����ϴ���
        PlayerInGameData playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(OwnerClientId);
        sbyte newHP = (sbyte)(playerData.hp + healingValue);
        if (newHP > playerData.maxHp) newHP = playerData.maxHp;

        
    }

    /// <summary>
    /// �������� ȣ���ؾ��ϴ� �޼���. �������� �����մϴ�.
    /// </summary>
    public void TakingDamage(sbyte damage, ulong clientWhoAttacked)
    {
        // ��û�� �÷��̾� ���� HP�� �������� 
        PlayerInGameData playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(OwnerClientId);
        sbyte newPlayerHP = playerData.hp;

        // HP���� Damage�� Ŭ ���(���ӿ��� ó���� Player���� HP�ܷ� �ľ��ؼ� �˾Ƽ� �Ѵ�.)
        if (newPlayerHP <= damage)
        {
            // HP 0
            newPlayerHP = 0;

            // ���ӿ��� ó��
            GameOver(clientWhoAttacked);
        }
        else
        {
            // HP ���� ���
            newPlayerHP -= (sbyte)damage;
        }

        // ����� HP�� ������ ����
        playerData.hp = newPlayerHP;
        GameMultiplayer.Instance.SetPlayerDataFromClientId(OwnerClientId, playerData);

        // �� Client �÷��̾��� HP�� UI ������Ʈ
        /*NetworkClient networkClient = NetworkManager.ConnectedClients[OwnerClientId];
        networkClient.PlayerObject.GetComponent<PlayerClient>().SetHPClientRPC(playerData.hp, playerData.maxHp);*/
        GetComponent<PlayerClient>().SetHPClientRPC(playerData.hp, playerData.maxHp);
    }

    // ���ӿ��� ó��. �������� ���.
    private void GameOver(ulong clientWhoAttacked)
    {
        // ų�� �÷��̾� ���ھ� ������Ʈ
        GameMultiplayer.Instance.AddPlayerScore(clientWhoAttacked, 300);

        // ���ӿ��� �÷��̾� ����� ������ ���.
        GameManager.Instance.UpdatePlayerGameOverOnServer(OwnerClientId);

        // GameOver �÷��̾� AnimState GameOver ���·� ������ ���. ���ӿ��� �ִϸ��̼� ����. 
        GameMultiplayer.Instance.UpdatePlayerMoveAnimStateOnServer(OwnerClientId, PlayerMoveAnimState.GameOver);

        // �ش� �÷��̾� ���� �Ұ� ó�� �� ���ӿ��� �˾� ����.
        GetComponent<PlayerClient>().SetPlayerGameOverClientRPC();
    }
}