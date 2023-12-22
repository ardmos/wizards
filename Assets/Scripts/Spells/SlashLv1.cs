using Unity.Netcode;
using UnityEngine;
/// <summary>
/// 1���� �Ѽհ� ���� ��ų ��ũ��Ʈ�Դϴ�.  
/// �Ӽ� : �븻
/// </summary>
public class SlashLv1 : Spell
{
    private void Start()
    {
        // ���������̴ϱ� ���� ª��
        Destroy(gameObject,1f);
    }

    /// <summary>
    /// 1. �� �ɷ�ġ ����
    /// </summary>
    public override void InitSpellInfoDetail()
    {
        Debug.Log($"InitSpellInfoDetail() {nameof(SlashLv1)}");
        spellInfo = new SpellInfo()
        {
            spellType = SpellType.Normal,
            coolTime = 2.0f,
            lifeTime = 10.0f,
            moveSpeed = 10.0f,
            price = 30,
            level = 1,
            spellName = "Slash Lv1",
            castAble = true,
            iconImage = iconImage
        };

        if (spellInfo == null)
        {
            Debug.Log("Spell Info is null");
        }
        else
        {
            Debug.Log("Spell Info is not null");
            Debug.Log($"spell Type : {spellInfo.spellType}, level : {spellInfo.level}");
        }
    }

    // �Ӽ��� �浹 ����� ������ִ� �޼ҵ� <--- Slash �迭 ��ũ��Ʈ�� ���� ���� ���������� �𸣰ڴ�. �ٸ� fire,water,ice ó��
    public override SpellLvlType CollisionHandling(SpellLvlType thisSpell, SpellLvlType opponentsSpell)
    {
        SpellLvlType result = new SpellLvlType();

        // Lvl ��
        int resultLevel = thisSpell.level - opponentsSpell.level;
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
        return result;
    }


    /// <summary>
    /// 2. CollisionEnter �浹 ó��
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter(Collision collision)
    {
        SpellHit(collision);
        
    }

    /// <summary>
    /// 3. ���� ����
    /// </summary>
    public override void CastSpell(SpellLvlType spellLvlType, NetworkObject player)
    {
        base.CastSpell(spellLvlType, player);
    }
}
