using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEditor.U2D.Animation;
using UnityEngine;
/// <summary>
/// 1. Knight ½ºÅÈ °ü¸®
/// </summary>
public class Knight : ICharacter
{
    public FixedString64Bytes playerName { get; set; }
    public sbyte hp { get; set; } = 0;
    public sbyte maxHp { get; set; }
    public sbyte score { get; set; }
    public float moveSpeed { get; set; } = 10f;
    public CharacterClass characterClass { get; set; }
    public SkillName[] skills { get; set; } = new SkillName[]{
                SkillName.ElectricSlashAttack1_Lv1,
                SkillName.ElectricSlashAttack2_Lv1,
                SkillName.ElectricSlashAttack1_Lv1,
                SkillName.Dash_Lv1
                };

    public void SetCharacterData(ICharacter characterData)
    {
        this.playerName = characterData.playerName;
        this.hp = characterData.hp;
        this.maxHp = characterData.maxHp;
        this.score = characterData.score;
        this.moveSpeed = characterData.moveSpeed;
        this.characterClass = characterData.characterClass;
        this.skills = characterData.skills;
    }

    public ICharacter GetCharacterData()
    {
        return this;
    }


}
