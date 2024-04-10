using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
/// <summary>
/// 1. Knight ���� ����
/// </summary>
public class Knight : Player
{
    public SpellName[] ownedSpellList;

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
