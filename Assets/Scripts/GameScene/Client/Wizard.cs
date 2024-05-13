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
    public SpellManagerClientWizard spellManagerClientWizard;
    public Character characterClass { get; set; } = Character.Wizard;
    public sbyte hp { get; set; } = 5;
    public sbyte maxHp { get; set; } = 5;
    public float moveSpeed { get; set; } = 4f;
    public SkillName[] skills { get; set; } = new SkillName[]{
                SkillName.FireBallLv1,
                SkillName.WaterBallLv1,
                SkillName.BlizzardLv1,
                SkillName.MagicShieldLv1
                };

    [ClientRpc]
    public override void InitializePlayerClientRPC(SkillName[] skills)
    {
        base.InitializePlayerClientRPC(skills);
        // 보유 skill 정보를 클라이언트측 캐릭터의 스킬 컨트롤러에 전달.
        spellManagerClientWizard.InitPlayerSpellInfoListClient(skills);
        //Debug.Log("1. Wizard.InitializePlayerClientRPC");
    }

    public ICharacter GetCharacterData()
    {
        return this;
    }

    protected override void GameInput_OnAttack1Started(object sender, EventArgs e)
    {
        spellManagerClientWizard.CastingNormalSpell(0);
    }

    protected override void GameInput_OnAttack1Ended(object sender, EventArgs e)
    {
        spellManagerClientWizard.ShootNormalSpell(0);
    }

    protected override void GameInput_OnAttack2Started(object sender, EventArgs e)
    {
        spellManagerClientWizard.CastingNormalSpell(1);
    }

    protected override void GameInput_OnAttack2Ended(object sender, EventArgs e)
    {
        spellManagerClientWizard.ShootNormalSpell(1);
    }

    protected override void GameInput_OnAttack3Started(object sender, EventArgs e)
    {
        spellManagerClientWizard.CastingBlizzard();
    }

    protected override void GameInput_OnAttack3Ended(object sender, EventArgs e)
    {
        spellManagerClientWizard.SetBlizzard();
    }

    protected override void GameInput_OnDefenceStarted(object sender, EventArgs e)
    {
        spellManagerClientWizard.ActivateDefenceSpellOnClient();
    }

    protected override void GameInput_OnDefenceEnded(object sender, EventArgs e)
    {

    }
}
