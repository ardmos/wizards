using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 
/// 1���� ���̾ ��ũ��Ʈ�Դϴ�.
/// 
/// !!! ���� ���
/// 1. �� �ɷ�ġ ����
/// 2. ���� ���忡���� Spell Object�� �̵� ó��
/// 3. CollisionEnter �浹 ó��
/// 4. ���� ����
/// </summary>
public class FireBallLv1 : FireSpell
{
    public GameObject muzzlePrefab;
    public GameObject hitPrefab;

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
            price = 30,
            level = 1,
            spellName = "FireBall Lv.1",
            castAble = true
        };
    }

    /// <summary>
    /// 4. ���� ����
    /// </summary>
    public override void CastSpell(GameObject spellPrefab, Transform muzzle)
    {
        base.CastSpell(spellPrefab, muzzle);
        MuzzleVFX(muzzlePrefab, muzzle);
    }

    public override void MuzzleVFX(GameObject muzzlePrefab, Transform muzzle)
    {
        base.MuzzleVFX(muzzlePrefab, muzzle);
    }
}
