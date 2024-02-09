using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 공격 마법의 공통적인 기능들을 관리하는 클래스
/// </summary>
public abstract class AttackSpell : NetworkBehaviour
{
    private const byte sfxCasting = 0;
    private const byte sfxShooting = 1;
    private const byte sfxHit = 2;

    [SerializeField] protected SpellInfo spellInfo;

    [SerializeField] protected GameObject muzzleVFXPrefab;
    [SerializeField] protected GameObject hitVFXPrefab;
    [SerializeField] protected List<GameObject> trails;

    // 마법 충돌시 속성 계산
    public abstract SpellInfo CollisionHandling(SpellInfo thisSpell, SpellInfo opponentsSpell);

    /// <summary>
    /// 2. CollisionEnter 충돌 처리 (서버 권한 방식)
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter(Collision collision)
    {
        // 서버에서만 처리.
        if (IsClient) return;

        SpellManager.Instance.SpellHitOnServer(collision, this);
        // 마법 충돌 사운드 재생
        PlaySFX(sfxHit);
    }

    // 마법 캐스팅 시작시 상세값 설정
    public virtual void InitSpellInfoDetail(SpellInfo spellInfoFromServer)
    {
        if (IsClient) return;

        spellInfo = new SpellInfo(spellInfoFromServer);
        // 마법 생성 사운드 재생
        PlaySFX(sfxCasting);
    }

    public virtual void Shoot(Vector3 force, ForceMode forceMode)
    {
        if (IsClient) return;

        GetComponent<Rigidbody>().AddForce(force, forceMode);
        // 마법 발사 사운드 재생
        PlaySFX(sfxShooting);
    }

    public SpellInfo GetSpellInfo()
    {
        return spellInfo;
    }
    public GameObject GetMuzzleVFXPrefab()
    {
        return muzzleVFXPrefab;
    }
    public GameObject GetHitVFXPrefab()
    {
        return hitVFXPrefab;
    }
    public List<GameObject> GetTrails()
    {
        return trails;
    }

    private void PlaySFX(byte state)
    {
        if (SoundManager.instance == null) return;

        SoundManager.instance.PlayMagicSFXClientRPC(spellInfo.spellName, state);
    }
}
