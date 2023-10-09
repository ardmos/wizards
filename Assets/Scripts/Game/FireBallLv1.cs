using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 
/// 1레벨 파이어볼 스크립트입니다.
/// 
/// !!! 현재 기능
/// 1. 상세 능력치 설정
/// 2. 게임 월드에서의 Spell Object의 이동 처리
/// 3. CollisionEnter 충돌 처리
/// 4. 마법 시전
/// </summary>
public class FireBallLv1 : FireSpell
{
    [SerializeField]
    private SpellInfo spellInfo;

    public GameObject muzzlePrefab;

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
    /// 1. 상세 능력치 설정
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
            price = 30,
            level = 1,
            spellName = "FireBall Lv.1",
            castAble = true
        };
    }

    /// <summary>
    /// 4. 마법 시전
    /// </summary>
    public override void CastSpell(Transform muzzle)
    {
        base.CastSpell(muzzle);
        MuzzleVFX(muzzlePrefab, muzzle);
    }

    public override void MuzzleVFX(GameObject muzzlePrefab, Transform muzzle)
    {
        base.MuzzleVFX(muzzlePrefab, muzzle);
    }
}
