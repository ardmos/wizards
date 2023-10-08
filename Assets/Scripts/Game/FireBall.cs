using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// �̰��� ���̾ ��ũ��Ʈ�Դϴ�.
/// 
/// 
/// �Ʒ� ���뺸��, ������ ���̾ ��ũ��Ʈ�� �����°� ���ƺ���. �װ����� ��
/// 
/// 
/// !!! ���� ���
/// 1. Lvl�� ���� �� �ɷ�ġ ����
/// 2. ���� ���忡���� Spell Object�� �̵� ó��
/// 3. CollisionEnter �浹 ó��
/// </summary>
public class FireBall : FireSpell
{
    SpellInfo fireBallLv1;
    SpellInfo fireBallLv2;
    SpellInfo fireBallLv3;
    SpellInfo currentSpell;

    // Start is called before the first frame update
    void Start()
    {
        InitSpellInfoDetail();
        currentSpell = new SpellInfo();
        currentSpell = fireBallLv1;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// 1. Lvl�� ���� �� �ɷ�ġ ����
    /// </summary>
    public override void InitSpellInfoDetail()
    {
        fireBallLv1 = new SpellInfo()
        {
            spellType = SpellType.Fire,
            coolTime = 5.0f,
            restTime = 0.0f,
            lifeTime = 10.0f,
            moveSpeed = 10.0f,
            price = 30,
            level = 1,
            spellName = "FireBall Lv.1",
            castAble = true
        };
        fireBallLv2 = new SpellInfo()
        {
            spellType = SpellType.Fire,
            coolTime = 5.0f,
            restTime = 0.0f,
            lifeTime = 10.0f,
            moveSpeed = 10.0f,
            price = 40,
            level = 2,
            spellName = "FireBall Lv.2",
            castAble = true
        };
        fireBallLv3 = new SpellInfo()
        {
            spellType = SpellType.Fire,
            coolTime = 5.0f,
            restTime = 0.0f,
            lifeTime = 10.0f,
            moveSpeed = 10.0f,
            price = 50,
            level = 3,
            spellName = "FireBall Lv.3",
            castAble = true
        };
    }

    public void SetSpellLv1()
    {
        currentSpell = fireBallLv1;
    }
    public void SetSpellLv2()
    {
        currentSpell = fireBallLv2;
    }
    public void SetSpellLv3()
    {
        currentSpell = fireBallLv3;
    }
}
