using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 1. Knight 스탯 관리
/// </summary>
public class Knight : Player
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
        UpdateAttackInput();
        for (int i = 0; i < ownedSpellPrefabList.Count; i++)
        {
            spellController.CheckCastSpell(i);
        }     
    }

    public override void OnNetworkSpawn()
    {
        // 테스트 목적으로 스펠리스트 하드코딩. 로비씬에서 선택해줘야함.
        ownedSpellList = new SpellName[]{
                SpellName.SlashLv1,
                SpellName.SlashLv1,
                SpellName.SlashLv1
                };
        InitializePlayer(ownedSpellList);
    }
}
