using System.Collections.Generic;
using Unity.Collections;

// 캐릭터의 공통 속성을 정의하는 인터페이스
public interface ICharacter 
{
    Character characterClass { get; set; }
    sbyte hp { get; set; }
    sbyte maxHp { get; set; }
    float moveSpeed { get; set; }
    public SkillName[] skills { get; set; }

    ICharacter GetCharacterData();
}