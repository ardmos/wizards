using System;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 1. Knight ���� ����
/// </summary>
public class Knight : PlayerClient, ICharacter
{
    public SkillManagerClientKnight skillManagerClientKnight;
    public Character characterClass { get; set; } = Character.Knight;
    public sbyte hp { get; set; } = 10;
    public sbyte maxHp { get; set; } = 10;
    public float moveSpeed { get; set; } = 10f;
    public SkillName[] skills { get; set; } = new SkillName[]{
                SkillName.ElectricSlashAttackVertical_Lv1,
                SkillName.ElectricSlashAttackWhirlwind_Lv1,
                SkillName.ElectricSlashAttackVertical_Lv1,
                SkillName.Dash_Lv1
                };

    [ClientRpc]
    public override void InitializePlayerClientRPC(SkillName[] skills)
    {
        base.InitializePlayerClientRPC(skills);
        // ���� skill ������ Ŭ���̾�Ʈ�� ĳ������ ��ų ��Ʈ�ѷ��� ����.
        skillManagerClientKnight.InitPlayerSpellInfoListClient(skills);
    }

    public ICharacter GetCharacterData()
    {
        return this;
    }

    protected override void GameInput_OnAttack1Started(object sender, EventArgs e)
    {
        Debug.Log("GameInput_OnAttack1Started");
        skillManagerClientKnight.AimingSkill(0);
    }

    protected override void GameInput_OnAttack1Ended(object sender, EventArgs e)
    {
        Debug.Log("GameInput_OnAttack1Ended");
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
        skillManagerClientKnight.AimingSkill(SkillSpellManagerServer.DEFENCE_SPELL_INDEX_DEFAULT);
    }

    protected override void GameInput_OnDefenceEnded(object sender, EventArgs e)
    {
        Debug.Log("GameInput_OnDefenceEnded");
        skillManagerClientKnight.ActivateDefenceSkillOnClient();
    }
}
