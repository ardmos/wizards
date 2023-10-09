using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Spell;
/// <summary>
/// 
/// 2���� ���̾ ��ũ��Ʈ�Դϴ�.
/// 
/// !!! ���� ���
/// 1. �� �ɷ�ġ ����
/// 2. ���� ���忡���� Spell Object�� �̵� ó��
/// 3. CollisionEnter �浹 ó��
/// </summary>
public class FireBallLv2 : FireSpell
{
    SpellInfo spellInfo;

    // Start is called before the first frame update
    void Start()
    {
        InitSpellInfoDetail();
    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// 1. �� �ɷ�ġ ����
    /// </summary>
    public override void InitSpellInfoDetail()
    {
        spellInfo = new SpellInfo()
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
    }
}
