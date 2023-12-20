using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary> 
/// 1. Wizard 스탯 관리
/// </summary>
public class Wizard : Player
{
    public SpellNames.Wizard[] ownedSpellList;
    public List<GameObject> ownedSpellPrefabList;
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
            // HP 초기화
            hPBar.SetHP(hp);

            // GameMultiplayer.PlayerNetworkDataList.ownedSpellList 에다가 장착 장비 기준으로 스펠 업데이트를 해줬다는 전제 하에 진행되는 로직. 현재는 아래에서 배열 초기화를 해준다. 나중에는 장착 아이템이 해줘야 함.
            PlayerData playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(OwnerClientId);
            ownedSpellList = new SpellNames.Wizard[]{
                SpellNames.Wizard.FireBallLv1,
                SpellNames.Wizard.WaterBallLv1,
                SpellNames.Wizard.IceBallLv1
                };
            CharacterClasses.Class playerClass = playerData.playerClass;
            // GameAsset으로부터 Spell Prefab 로딩. playerClass와 spellName으로 필요한 프리팹만 불러온다. 
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
