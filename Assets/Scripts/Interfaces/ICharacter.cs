using System.Collections.Generic;
using Unity.Collections;

// ĳ������ ���� �Ӽ��� �����ϴ� �������̽�
public interface ICharacter
{
    FixedString64Bytes playerName { get; set; }
    sbyte hp { get; set; }
    sbyte maxHp { get; set; }
    sbyte score { get; set; }
    float moveSpeed { get; set; }
    CharacterClass characterClass { get; set; }
    public SkillName[] skills { get; set; }

    void SetCharacterData(ICharacter characterData);
    ICharacter GetCharacterData();
}