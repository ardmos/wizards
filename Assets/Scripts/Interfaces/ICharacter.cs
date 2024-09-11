/// <summary>
/// 캐릭터의 공통 속성을 정의하는 인터페이스입니다.
/// </summary>
public interface ICharacter 
{
    /// <summary>
    /// 캐릭터의 클래스를 나타냅니다.
    /// </summary>
    Character characterClass { get; set; }

    /// <summary>
    /// 현재 체력을 나타냅니다.
    /// </summary>
    sbyte hp { get; set; }

    /// <summary>
    /// 최대 체력을 나타냅니다.
    /// </summary>
    sbyte maxHp { get; set; }

    /// <summary>
    /// 이동 속도를 나타냅니다.
    /// </summary>
    float moveSpeed { get; set; }

    /// <summary>
    /// 캐릭터가 사용할 수 있는 스킬 목록입니다.
    /// </summary>
    SpellName[] skills { get; set; }

    /// <summary>
    /// 캐릭터 데이터를 반환하는 메서드입니다.
    /// </summary>
    /// <returns>ICharacter 인터페이스를 구현한 객체</returns>
    ICharacter GetCharacterData();
}