using System;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 1. Knight 스탯 관리
/// </summary>
public class Knight : PlayerClient, ICharacter
{
    public SkillManagerClientKnight skillManagerClientKnight;
    public Character characterClass { get; set; } = Character.Knight;
    public sbyte hp { get; set; } = 7;
    public sbyte maxHp { get; set; } = 7;
    public float moveSpeed { get; set; } = 4f;
    public SpellName[] skills { get; set; } = new SpellName[]{
                SpellName.ElectricSlashAttackVertical_Lv1,
                SpellName.ElectricSlashAttackWhirlwind_Lv1,
                SpellName.ElectricSlashAttackChargeSlash_Lv1,
                SpellName.Dash_Lv1
                };

/*    [ClientRpc]
    public override void InitializePlayerClientRPC(SkillName[] skills)
    {
        base.InitializePlayerClientRPC(skills);
        // 보유 skill 정보를 클라이언트측 캐릭터의 스킬 컨트롤러에 전달.
        skillManagerClientKnight.InitPlayerSpellInfoListClient(skills);
    }*/

    public ICharacter GetCharacterData()
    {
        return this;
    }

    protected override void GameInput_OnAttack1Started(object sender, EventArgs e)
    {
        //Debug.Log("GameInput_OnAttack1Started");
        skillManagerClientKnight.AimingSkill(0);
    }

    protected override void GameInput_OnAttack1Ended(object sender, EventArgs e)
    {
        //Debug.Log("GameInput_OnAttack1Ended");
        skillManagerClientKnight.ActivateAttackSkillOnClient(0);
    }

    protected override void GameInput_OnAttack2Started(object sender, EventArgs e)
    {
        skillManagerClientKnight.AimingSkill(1);
    }

    protected override void GameInput_OnAttack2Ended(object sender, EventArgs e)
    {
        skillManagerClientKnight.ActivateAttackSkillOnClient(1);
    }

    protected override void GameInput_OnAttack3Started(object sender, EventArgs e)
    {
        skillManagerClientKnight.AimingSkill(2);
    }

    protected override void GameInput_OnAttack3Ended(object sender, EventArgs e)
    {
        skillManagerClientKnight.ActivateAttackSkillOnClient(2);
    }

    protected override void GameInput_OnDefenceStarted(object sender, EventArgs e)
    {
        Debug.Log("GameInput_OnDefenceStarted");
        //skillManagerClientKnight.AimingSkill(SkillSpellManagerServer.DEFENCE_SPELL_INDEX_DEFAULT);
        skillManagerClientKnight.ActivateDefenceSkillOnClient();
    }

    protected override void GameInput_OnDefenceEnded(object sender, EventArgs e)
    {
        //Debug.Log("GameInput_OnDefenceEnded");
        //skillManagerClientKnight.ActivateDefenceSkillOnClient();
    }
}
