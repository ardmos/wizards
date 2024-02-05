using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 공격 마법의 공통적인 기능들을 관리하는 클래스
/// </summary>
public abstract class AttackSpell : NetworkBehaviour
{
    [SerializeField] protected SpellInfo spellInfo;

    [SerializeField] protected GameObject muzzleVFXPrefab;
    [SerializeField] protected GameObject hitVFXPrefab;
    [SerializeField] protected List<GameObject> trails;

    //[SerializeField] private bool isCollided;

    // 마법 충돌시 속성 계산
    public abstract SpellInfo CollisionHandling(SpellInfo thisSpell, SpellInfo opponentsSpell);

    /// <summary>
    /// 2. CollisionEnter 충돌 처리 (서버 권한 방식)
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter(Collision collision)
    {
        // 서버에서만 처리.
        if (!IsServer) return;
        SpellManager.Instance.SpellHitOnServer(collision, this);
    }

    // 마법 상세값 설정
    public virtual void InitSpellInfoDetail(SpellInfo spellInfoFromServer)
    {
        if (!IsServer) return;
        spellInfo = new SpellInfo(spellInfoFromServer);
    }

    public virtual void Shoot(Vector3 force, ForceMode forceMode)
    {
        if (!IsServer) return;
        //Debug.Log($"AttackSpell class Shoot()");
        GetComponent<Rigidbody>().AddForce(force, forceMode);
    }

/*    public void SetSpellIsCollided(bool isCollided)
    {
        this.isCollided = isCollided;
    }

    public bool IsCollided()
    {
        return this.isCollided;
    }*/

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
}
