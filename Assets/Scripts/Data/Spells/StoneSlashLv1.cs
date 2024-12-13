using UnityEngine;
/// <summary>
/// 1���� �Ѽհ� ���� ��ų ��ũ��Ʈ�Դϴ�.  
/// �Ӽ� : Stone
/// 
/// �׽�Ʈ��.
/// ���� Knight ���� �պ� �� �ٵ��� �ʿ� ����.
/// </summary>
public class StoneSlashLv1 : AttackSpell  // �ӽ÷� ���. Knight�� ��ų Ŭ������ ���� ���������Ѵ�!
{
    /// <summary>
    /// 1. �� �ɷ�ġ ����(���� ���ÿ� Server���� �ο����ִ� �ɷ�ġ �Դϴ�.)
    /// </summary>
/*    public override void InitSpellInfoDetail(SpellInfo spellInfoFromServer)
    {
        Debug.Log($"InitSpellInfoDetail() {nameof(StoneSlashLv1)}");
        base.InitSpellInfoDetail( spellInfoFromServer );

        if (spellInfo == null)
        {
            Debug.Log("StoneSlashLv1 AttackSpell Info is null");
        }
        else
        {
            Debug.Log("StoneSlashLv1 AttackSpell Info is not null");
            Debug.Log($"StoneSlashLv1 spell Type : {spellInfo.spellType}, level : {spellInfo.level}");
        }
    }*/

    // �Ӽ��� �浹 ����� ������ִ� �޼ҵ� <--- Slash �迭 ��ũ��Ʈ�� ���� ���� ���������� �𸣰ڴ�. �ٸ� fire,water,ice ó��
    public override SpellInfo CollisionHandling(SpellInfo thisSpell, SpellInfo opponentsSpell)
    {
        SpellInfo result = new SpellInfo();

        // Lvl ��
        byte resultLevel = (byte)(thisSpell.damage - opponentsSpell.damage);
        result.damage = resultLevel;
        // resultLevel ���� 0���� ���ų� ������ �� ����� �ʿ� ����. 
        //      0�̸� ���Ŵϱ� ���� �ʿ� ����
        //      ���̳ʽ��� ���Ŵϱ� ���� �ʿ� ����.
        //      �� �޼ҵ带 ȣ���ϴ� �� ���� ��ũ��Ʈ������ resultLevel���� ���� �ļ� ���� ������Ʈ �������θ� �Ǵ��ϸ� ��. 
        if (resultLevel <= 0)
        {
            return result;
        }
        // resultLevel���� 0���� ū ���� ���� �̱� ���. �븻Ÿ���� �븻�� ��ȯ�Ѵ�.
        result.spellType = SpellType.Stone;
        result.spellName = Spell.GetSpellName(result.damage, result.spellType);
        return result;
    }
}
