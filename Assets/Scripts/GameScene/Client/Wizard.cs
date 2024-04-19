using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
/// <summary> 
/// 1. Wizard 스탯 관리
/// </summary>
public class Wizard : PlayerClient, ICharacter
{
    public SpellControllerClientWizard spellController;
    public CharacterClass characterClass { get; set; } = CharacterClass.Wizard;
    public sbyte hp { get; set; } = 5;
    public sbyte maxHp { get; set; } = 5;
    public float moveSpeed { get; set; } = 10f;
    public SkillName[] skills { get; set; } = new SkillName[]{
                SkillName.FireBallLv1,
                SkillName.WaterBallLv1,
                SkillName.IceBallLv1,
                SkillName.MagicShieldLv1
                };

    public override void InitializePlayerClientRPC(SkillName[] skills)
    {
        base.InitializePlayerClientRPC(skills);
        // 보유 skill 정보를 클라이언트측 캐릭터의 스킬 컨트롤러에 전달.
        spellController.InitPlayerSpellInfoListClient(skills);
    }

    public ICharacter GetCharacterData()
    {
        return this;
    }

    protected override void GameInput_OnAttack1Started(object sender, EventArgs e)
    {
        spellController.StartCastingSpellOnClient(0);
    }

    protected override void GameInput_OnAttack1Ended(object sender, EventArgs e)
    {
        spellController.ShootCurrentCastingSpellOnClient(0);
    }

    protected override void GameInput_OnAttack2Started(object sender, EventArgs e)
    {
        spellController.StartCastingSpellOnClient(1);
    }

    protected override void GameInput_OnAttack2Ended(object sender, EventArgs e)
    {
        spellController.ShootCurrentCastingSpellOnClient(1);
    }

    protected override void GameInput_OnAttack3Started(object sender, EventArgs e)
    {
        spellController.StartCastingSpellOnClient(2);
    }

    protected override void GameInput_OnAttack3Ended(object sender, EventArgs e)
    {
        spellController.ShootCurrentCastingSpellOnClient(2);
    }

    protected override void GameInput_OnDefenceStarted(object sender, EventArgs e)
    {
        spellController.ActivateDefenceSpellOnClient();
    }

    protected override void GameInput_OnDefenceEnded(object sender, EventArgs e)
    {

    }
}
