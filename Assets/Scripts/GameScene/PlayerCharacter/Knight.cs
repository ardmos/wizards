using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
/// <summary>
/// 1. Knight 스탯 관리
/// </summary>
public class Knight : Player
{
    public SpellName[] ownedSpellList;

    public override void OnNetworkSpawn()
    {
        // 서버에서 생성해준다
        PlayerSpawnServerRPC();
    }

    [ServerRpc]
    private void PlayerSpawnServerRPC(ServerRpcParams serverRpcParams = default)
    {
        // 테스트 목적으로 스펠리스트 하드코딩. 로비씬에서 선택해줘야함.
        ownedSpellList = new SpellName[]{
                SpellName.StoneSlashLv1,
                SpellName.StoneSlashLv1,
                SpellName.StoneSlashLv1
                };
        InitializePlayerOnServer(ownedSpellList, serverRpcParams.Receive.SenderClientId);
    }
}
