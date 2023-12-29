using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 1. Knight ���� ����
/// </summary>
public class Knight : Player
{
    public SpellName[] ownedSpellList;

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;

        // == �̵� ==
        // Server Auth ����� �̵� ó��
        HandleMovementServerAuth();
        //�׽�Ʈ�� Client Auth ��� �̵� ó��
        //HandleMovement(); 

        // == ���� == 
        // ��ų�ߵ�üũ��ĵ� ���� Update���� �� �� ���� ���� ������� �ٲ� �ʿ䰡 ����.!  �ϴ� �׽�Ʈ������ Update���� ����. 
        UpdateAttackInput();
        for (int i = 0; i < ownedSpellPrefabList.Count; i++)
        {
            spellController.CheckCastSpell(i);
        }     
    }

    public override void OnNetworkSpawn()
    {
        // �׽�Ʈ �������� ���縮��Ʈ �ϵ��ڵ�. �κ������ �����������.
        ownedSpellList = new SpellName[]{
                SpellName.SlashLv1,
                SpellName.SlashLv1,
                SpellName.SlashLv1
                };
        InitializePlayer(ownedSpellList);
    }
}
