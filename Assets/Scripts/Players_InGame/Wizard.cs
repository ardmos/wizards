using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary> 
/// 1. Wizard ���� ����
/// </summary>
public class Wizard : Player
{
    public SpellNames.Wizard[] ownedSpellList;
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
        hp = 5;
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
            ownedSpellList = new SpellNames.Wizard[]{
                SpellNames.Wizard.FireBallLv1,
                SpellNames.Wizard.WaterBallLv1,
                SpellNames.Wizard.IceBallLv1
                };
            CharacterClasses.Class playerClass = playerData.playerClass;
            // GameAsset���κ��� Spell Prefab �ε�. playerClass�� spellName���� �ʿ��� �����ո� �ҷ��´�. 
            ownedSpellPrefabList = new List<GameObject>();

            foreach (var spellName in ownedSpellList)
            {
                ownedSpellPrefabList.Add(GameAssets.instantiate.GetWizardSpellPrefab(playerClass, spellName));
            }
            for (int i = 0; i < ownedSpellPrefabList.Count; i++)
            {
                spellController.SetCurrentSpell(ownedSpellPrefabList[i], i);
            }

        }
    }
}
