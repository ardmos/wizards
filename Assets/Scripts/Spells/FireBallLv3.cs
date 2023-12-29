using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 
/// 3���� ���̾ ��ũ��Ʈ�Դϴ�.
/// 
/// !!! ���� ���
/// 1. �� �ɷ�ġ ����
/// 2. ���� ���忡���� Spell Object�� �̵� ó��
/// 3. CollisionEnter �浹 ó��
/// </summary>
public class FireBallLv3 : FireSpell
{
    /// <summary>
    /// 1. �� �ɷ�ġ ����
    /// </summary>
    public override void InitSpellInfoDetail()
    {
        spellInfo = new SpellInfo()
        {
            spellType = SpellType.Fire,
            coolTime = 5.0f,
            // #Test Code 10/16 : 
            //restTime = 0.0f,
            lifeTime = 10.0f,
            moveSpeed = 10.0f,
            price = 50,
            level = 3,
            spellName = SpellName.FireBallLv1,
            castAble = true
        };
    }
}
