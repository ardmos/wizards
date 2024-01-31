using Unity.Netcode;
using UnityEngine;
/// <summary> 
/// 1. Wizard 스탯 관리
/// </summary>
public class Wizard : Player
{
    public SpellName[] ownedSpellList;

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;

        // == 이동 ==
        // Server Auth 방식의 이동 처리
        HandleMovementServerAuth();
        //테스트용 Client Auth 방식 이동 처리
        //HandleMovement(); 

        // == 공격 == 
        // 스킬발동체크방식도 지금 Update에서 좀 더 낭비 없는 방식으로 바꿀 필요가 있음.!  일단 테스트용으로 Update에서 실행. 
/*        UpdateAttackInput();
        for (int i = 0; i < ownedSpellPrefabList.Count; i++)
        {
            spellController.CheckCastSpell(i);
        }*/
    }


    public override void OnNetworkSpawn()
    {
        
        // 클라이언트 각자가 서버에 player data 초기화 요청. 서버에서 초기화해준다
        if(IsClient&&IsOwner)
        {
            Debug.Log($"Player{OwnerClientId} OnNetworkSpawn!");
            InitializePlayerServerRPC();
        }
            
    }

    [ServerRpc]
    private void InitializePlayerServerRPC(ServerRpcParams serverRpcParams = default)
    {
        Debug.Log($"InitializePlayerServerRPC. Player{serverRpcParams.Receive.SenderClientId} requested Initialize PlayerInGameData.");
        // 테스트 목적으로 스펠리스트 하드코딩. 로비씬에서 선택해줘야함. (스킬트리)
        ownedSpellList = new SpellName[]{
                SpellName.FireBallLv1,
                SpellName.WaterBallLv1,
                SpellName.IceBallLv1,
                SpellName.MagicShieldLv1
                };
        InitializePlayerOnServer(ownedSpellList, serverRpcParams.Receive.SenderClientId);
    }
}
