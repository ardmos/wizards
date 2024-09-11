/// <summary>
/// ĳ������ ���� �Ӽ��� �����ϴ� �������̽��Դϴ�.
/// </summary>
public interface ICharacter 
{
    /// <summary>
    /// ĳ������ Ŭ������ ��Ÿ���ϴ�.
    /// </summary>
    Character characterClass { get; set; }

    /// <summary>
    /// ���� ü���� ��Ÿ���ϴ�.
    /// </summary>
    sbyte hp { get; set; }

    /// <summary>
    /// �ִ� ü���� ��Ÿ���ϴ�.
    /// </summary>
    sbyte maxHp { get; set; }

    /// <summary>
    /// �̵� �ӵ��� ��Ÿ���ϴ�.
    /// </summary>
    float moveSpeed { get; set; }

    /// <summary>
    /// ĳ���Ͱ� ����� �� �ִ� ��ų ����Դϴ�.
    /// </summary>
    SpellName[] skills { get; set; }

    /// <summary>
    /// ĳ���� �����͸� ��ȯ�ϴ� �޼����Դϴ�.
    /// </summary>
    /// <returns>ICharacter �������̽��� ������ ��ü</returns>
    ICharacter GetCharacterData();
}