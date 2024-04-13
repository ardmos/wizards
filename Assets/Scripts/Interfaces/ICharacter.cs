using System.Collections.Generic;
using Unity.Collections;

// ĳ������ ���� �Ӽ��� �����ϴ� �������̽�
public interface ICharacter 
{
    CharacterClass characterClass { get; set; }
    sbyte hp { get; set; }
    sbyte maxHp { get; set; }
    float moveSpeed { get; set; }
    public SkillName[] skills { get; set; }

    ICharacter GetCharacterData();
}