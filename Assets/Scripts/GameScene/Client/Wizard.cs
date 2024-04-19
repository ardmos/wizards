using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
/// <summary> 
/// 1. Wizard ½ºÅÈ °ü¸®
/// </summary>
public class Wizard : PlayerClient, ICharacter
{
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
