using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
/// <summary>
/// 
/// 1���� ���̾ ��ũ��Ʈ�Դϴ�.
/// 
/// !!! ���� ���
/// 1. �� �ɷ�ġ ����
/// 2. CollisionEnter �浹 ó��
/// 3. ���� ����
/// </summary>
public class FireBallLv1 : FireSpell
{

    /// <summary>
    /// 1. �� �ɷ�ġ ����
    /// </summary>
    public override void InitSpellInfoDetail()
    {
        //Debug.Log("InitSpellInfoDetail() FireBall Lv1");
        spellInfo = new SpellInfo()
        {
            spellType = SpellType.Fire,
            coolTime = 2.0f,
            lifeTime = 10.0f,
            //moveSpeed = 10.0f, //�׽�Ʈ��
            moveSpeed = 0.0f,
            price = 30,
            level = 1,
            spellName = SpellName.FireBallLv1,
            spellState = SpellState.Ready,
        };
    }

    /// <summary>
    /// 2. CollisionEnter �浹 ó�� (���� ���� ���)
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter(Collision collision)
    {
        SpellHit(collision);
    }

    /// <summary>
    /// 3. ���� ����
    /// </summary>
    public override void CastSpell(SpellInfo spellInfo, NetworkObject player)
    {
        base.CastSpell(spellInfo, player);
    }
}
