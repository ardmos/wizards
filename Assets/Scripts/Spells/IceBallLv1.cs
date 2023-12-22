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
public class IceBallLv1 : IceSpell
{
    /// <summary>
    /// 1. �� �ɷ�ġ ����
    /// </summary>
    public override void InitSpellInfoDetail()
    {
        spellInfo = new SpellInfo()
        {
            spellType = SpellType.Ice,
            coolTime = 2.0f,
            lifeTime = 10.0f,
            moveSpeed = 10.0f,
            price = 30,
            level = 1,
            spellName = "IceBall Lv.1",
            castAble = true,
            iconImage = iconImage
        };
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
        MuzzleVFX(muzzleVFXPrefab, player);
    }

    public override void MuzzleVFX(GameObject muzzlePrefab, NetworkObject player)
    {
        base.MuzzleVFX(muzzlePrefab, player);
    }
}
