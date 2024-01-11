using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
/// <summary>
/// 1���� ���ͺ� ��ũ��Ʈ�Դϴ�.
/// ���� ���
///   1. �� �ɷ�ġ ����
///   2. CollisionEnter �浹 ó��
///   3. ���� ����
/// </summary>
public class WaterBallLv1 : WaterSpell
{
    /// <summary>
    /// 1. �� �ɷ�ġ ����
    /// </summary>
    public override void InitSpellInfoDetail()
    {
        spellInfo = SpellSpecifications.Instance.GetSpellSpec(SpellName.WaterBallLv1);
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
    public override void CastSpell(SpellInfo spellInfo, NetworkObject player)
    {
        base.CastSpell(spellInfo, player);
    }
}
