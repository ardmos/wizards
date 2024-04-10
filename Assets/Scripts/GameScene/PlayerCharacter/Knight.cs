using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
/// <summary>
/// 1. Knight 스탯 관리
/// </summary>
public class Knight : Player
{
    public SpellName[] ownedSpellList;

    private void Update()
    {
        if (!IsOwner) return;

        // == 이동 ==
        // Server Auth 방식의 이동 처리
        GetComponent<PlayerMovementClient>().HandleMovementServerAuth();
        //테스트용 Client Auth 방식 이동 처리
        //HandleMovement(); 

        // == 공격 == 
        // 스킬발동체크방식도 지금 Update에서 좀 더 낭비 없는 방식으로 바꿀 필요가 있음.!  일단 테스트용으로 Update에서 실행. 
        //UpdateAttackInput();
/*        for (int i = 0; i < ownedSpellPrefabList.Count; i++)
        {
            spellController.StartCastingSpell(i);
        }   */  // Udate에서 좀 더 낭비없도록 옵저버패턴 형식으로 수정했음. player.cs에서 실행합니다
    }

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
