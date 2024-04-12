using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
/// <summary>
/// 1. Knight ½ºÅÈ °ü¸®
/// </summary>
public class Knight : ICharacter
{
    public string playerName { get; set; }
    public sbyte hp { get; set; } = 0;
    public float moveSpeed { get; set; } = 10f;
    public SkillName[] skills { get; set; } = new SkillName[]{
                SkillName.ElectricSlashAttack1_Lv1,
                SkillName.ElectricSlashAttack2_Lv1,
                SkillName.ElectricSlashAttack1_Lv1,
                SkillName.Dash_Lv1
                };

    public void SetCharacterData(string playerName, sbyte hp, float moveSpeed, SkillName[] skills)
    {
        this.playerName = playerName;
        this.hp = hp;
        this.moveSpeed = moveSpeed;
        this.skills = skills;
    }

    public ICharacter GetCharacterData()
    {
        return this;
    }


}
