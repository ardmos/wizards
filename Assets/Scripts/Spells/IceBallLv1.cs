using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
/// <summary>
/// 
/// 1레벨 파이어볼 스크립트입니다.
/// 
/// !!! 현재 기능
/// 1. 상세 능력치 설정
/// 2. CollisionEnter 충돌 처리
/// 3. 마법 시전
/// </summary>
public class IceBallLv1 : IceSpell
{
    /// <summary>
    /// 1. 상세 능력치 설정
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
    /// 2. CollisionEnter 충돌 처리
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter(Collision collision)
    {
        SpellHit(collision);
    }

    /// <summary>
    /// 3. 마법 시전
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
