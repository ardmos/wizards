using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
/// <summary> 
/// 1. Wizard ���� ����
/// </summary>
public class Wizard : PlayerClient, ICharacter
{
    public SpellManagerClientWizard spellManagerClientWizard;
    public Character characterClass { get; set; } = Character.Wizard;
    public sbyte hp { get; set; } = 5;
    public sbyte maxHp { get; set; } = 5;
    public float moveSpeed { get; set; } = 5f;
    public SkillName[] skills { get; set; } = new SkillName[]{
                SkillName.FireBallLv1,
                SkillName.WaterBallLv1,
                SkillName.IceBallLv1,
                SkillName.MagicShieldLv1
                };

    [ClientRpc]
    public override void InitializePlayerClientRPC(SkillName[] skills)
    {
        base.InitializePlayerClientRPC(skills);
        // ���� skill ������ Ŭ���̾�Ʈ�� ĳ������ ��ų ��Ʈ�ѷ��� ����.
        spellManagerClientWizard.InitPlayerSpellInfoListClient(skills);
        //Debug.Log("1. Wizard.InitializePlayerClientRPC");
    }

    public ICharacter GetCharacterData()
    {
        return this;
    }

    protected override void GameInput_OnAttack1Started(object sender, EventArgs e)
    {
        spellManagerClientWizard.CastingSpell(0);
    }

    protected override void GameInput_OnAttack1Ended(object sender, EventArgs e)
    {
        spellManagerClientWizard.ShootSpell(0);
    }

    protected override void GameInput_OnAttack2Started(object sender, EventArgs e)
    {
        spellManagerClientWizard.CastingSpell(1);
    }

    protected override void GameInput_OnAttack2Ended(object sender, EventArgs e)
    {
        spellManagerClientWizard.ShootSpell(1);
    }

    protected override void GameInput_OnAttack3Started(object sender, EventArgs e)
    {
        spellManagerClientWizard.CastingSpell(2);
    }

    protected override void GameInput_OnAttack3Ended(object sender, EventArgs e)
    {
        spellManagerClientWizard.ShootSpell(2);
    }

    protected override void GameInput_OnDefenceStarted(object sender, EventArgs e)
    {
        spellManagerClientWizard.ActivateDefenceSpellOnClient();
    }

    protected override void GameInput_OnDefenceEnded(object sender, EventArgs e)
    {

    }
}
