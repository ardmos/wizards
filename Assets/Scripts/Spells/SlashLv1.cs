using UnityEngine;
/// <summary>
/// 1���� �Ѽհ� ���� ��ų ��ũ��Ʈ�Դϴ�.  
/// �Ӽ� : �븻
/// 
/// �׽�Ʈ��.
/// ���� Knight ���� �պ� �� �ٵ��� �ʿ� ����.
/// </summary>
public class SlashLv1 : Spell
{
    /// <summary>
    /// 1. �� �ɷ�ġ ����(���� ���ÿ� Server���� �ο����ִ� �ɷ�ġ �Դϴ�.)
    /// </summary>
    public override void InitSpellInfoDetail(SpellInfo spellInfoFromServer)
    {
        Debug.Log($"InitSpellInfoDetail() {nameof(SlashLv1)}");
        spellInfo = spellInfoFromServer;

        if (spellInfo == null)
        {
            Debug.Log("SlashLv1 Spell Info is null");
        }
        else
        {
            Debug.Log("SlashLv1 Spell Info is not null");
            Debug.Log($"SlashLv1 spell Type : {spellInfo.spellType}, level : {spellInfo.level}");
        }
    }

    // �Ӽ��� �浹 ����� ������ִ� �޼ҵ� <--- Slash �迭 ��ũ��Ʈ�� ���� ���� ���������� �𸣰ڴ�. �ٸ� fire,water,ice ó��
    public override SpellInfo CollisionHandling(SpellInfo thisSpell, SpellInfo opponentsSpell)
    {
        SpellInfo result = new SpellInfo();

        // Lvl ��
        sbyte resultLevel = (sbyte)(thisSpell.level - opponentsSpell.level);
        result.level = resultLevel;
        // resultLevel ���� 0���� ���ų� ������ �� ����� �ʿ� ����. 
        //      0�̸� ���Ŵϱ� ���� �ʿ� ����
        //      ���̳ʽ��� ���Ŵϱ� ���� �ʿ� ����.
        //      �� �޼ҵ带 ȣ���ϴ� �� ���� ��ũ��Ʈ������ resultLevel���� ���� �ļ� ���� ������Ʈ �������θ� �Ǵ��ϸ� ��. 
        if (resultLevel <= 0)
        {
            return result;
        }
        // resultLevel���� 0���� ū ���� ���� �̱� ���. �븻Ÿ���� �븻�� ��ȯ�Ѵ�.
        result.spellType = SpellType.Normal;
        result.SetSpellName(result.level, result.spellType);
        return result;
    }


    /// <summary>
    /// 2. CollisionEnter �浹 ó��
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter(Collision collision)
    {
        SpellManager.Instance.SpellHitOnServer(collision, this);     
    }
}
