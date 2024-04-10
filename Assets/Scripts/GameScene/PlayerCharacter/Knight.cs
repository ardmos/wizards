using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
/// <summary>
/// 1. Knight ���� ����
/// </summary>
public class Knight : Player
{
    public SpellName[] ownedSpellList;

    private void Update()
    {
        if (!IsOwner) return;

        // == �̵� ==
        // Server Auth ����� �̵� ó��
        GetComponent<PlayerMovementClient>().HandleMovementServerAuth();
        //�׽�Ʈ�� Client Auth ��� �̵� ó��
        //HandleMovement(); 

        // == ���� == 
        // ��ų�ߵ�üũ��ĵ� ���� Update���� �� �� ���� ���� ������� �ٲ� �ʿ䰡 ����.!  �ϴ� �׽�Ʈ������ Update���� ����. 
        //UpdateAttackInput();
/*        for (int i = 0; i < ownedSpellPrefabList.Count; i++)
        {
            spellController.StartCastingSpell(i);
        }   */  // Udate���� �� �� ��������� ���������� �������� ��������. player.cs���� �����մϴ�
    }

    public override void OnNetworkSpawn()
    {
        // �������� �������ش�
        PlayerSpawnServerRPC();
    }

    [ServerRpc]
    private void PlayerSpawnServerRPC(ServerRpcParams serverRpcParams = default)
    {
        // �׽�Ʈ �������� ���縮��Ʈ �ϵ��ڵ�. �κ������ �����������.
        ownedSpellList = new SpellName[]{
                SpellName.StoneSlashLv1,
                SpellName.StoneSlashLv1,
                SpellName.StoneSlashLv1
                };
        InitializePlayerOnServer(ownedSpellList, serverRpcParams.Receive.SenderClientId);
    }
}
