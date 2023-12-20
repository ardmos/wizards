using System.Collections;
using System.Collections.Generic;
using Unity.Services.Matchmaker.Models;
using UnityEngine;
/// <summary>
/// 1. Knight ���� ����
/// </summary>
public class Knight : Player
{
    public SpellNames.Knight[] ownedSpellList;
    public List<GameObject> ownedSpellPrefabList;
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

    private void InitStat()
    {
        hp = 7;
        moveSpeed = 7f;
    }


    public override void OnNetworkSpawn()
    {
        InitializePlayer();
        if (IsOwner)
        {
            InitStat();
            GetComponent<Rigidbody>().isKinematic = false;
            // HP �ʱ�ȭ
            hPBar.SetHP(hp);

            // GameMultiplayer.PlayerNetworkDataList.ownedSpellList ���ٰ� ���� ��� �������� ���� ������Ʈ�� ����ٴ� ���� �Ͽ� ����Ǵ� ����. ����� �Ʒ����� �迭 �ʱ�ȭ�� ���ش�. ���߿��� ���� �������� ����� ��.
            PlayerData playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(OwnerClientId);
            ownedSpellList = new SpellNames.Knight[]{
                SpellNames.Knight.SlashLv1,
                SpellNames.Knight.SlashLv1,
                SpellNames.Knight.SlashLv1
                };
            CharacterClasses.Class playerClass = playerData.playerClass;
            // GameAsset���κ��� Spell Prefab �ε�. playerClass�� spellName���� �ʿ��� �����ո� �ҷ��´�. 
            ownedSpellPrefabList = new List<GameObject>();

            foreach (var spellName in ownedSpellList)
            {
                ownedSpellPrefabList.Add(GameAssets.instantiate.GetKnightSpellPrefab(playerClass, spellName));
            }
            for (int i = 0; i < ownedSpellPrefabList.Count; i++)
            {
                spellController.SetCurrentSpell(ownedSpellPrefabList[i], i);
            }

        }
    }
}
