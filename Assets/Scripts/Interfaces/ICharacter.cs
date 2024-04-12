using System.Collections.Generic;
using Unity.Collections;

// 캐릭터의 공통 속성을 정의하는 인터페이스
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