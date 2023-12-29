using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
/// <summary>
/// 1레벨 워터볼 스크립트입니다.
/// 현재 기능
///   1. 상세 능력치 설정
///   2. CollisionEnter 충돌 처리
///   3. 마법 시전
/// </summary>
public class WaterBallLv1 : WaterSpell
{
    /// <summary>
    /// 1. 상세 능력치 설정
    /// </summary>
    public override void InitSpellInfoDetail()
    {
        spellInfo = new SpellInfo()
        {
            spellType = SpellType.Water,
            coolTime = 2.0f,
            lifeTime = 10.0f,
            moveSpeed = 10.0f,
            price = 30,
            level = 1,
            spellName = SpellName.WaterBallLv1,
            castAble = true,
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
    public override void CastSpell(SpellInfo spellInfo, NetworkObject player)
    {
        base.CastSpell(spellInfo, player);
        MuzzleVFX(muzzleVFXPrefab, player);
    }

    public override void MuzzleVFX(GameObject muzzlePrefab, NetworkObject player)
    {
        base.MuzzleVFX(muzzlePrefab, player);
    }
}
