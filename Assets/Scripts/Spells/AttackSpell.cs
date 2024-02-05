using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// ���� ������ �������� ��ɵ��� �����ϴ� Ŭ����
/// </summary>
public abstract class AttackSpell : NetworkBehaviour
{
    [SerializeField] protected SpellInfo spellInfo;

    [SerializeField] protected GameObject muzzleVFXPrefab;
    [SerializeField] protected GameObject hitVFXPrefab;
    [SerializeField] protected List<GameObject> trails;

    //[SerializeField] private bool isCollided;

    // ���� �浹�� �Ӽ� ���
    public abstract SpellInfo CollisionHandling(SpellInfo thisSpell, SpellInfo opponentsSpell);

    /// <summary>
    /// 2. CollisionEnter �浹 ó�� (���� ���� ���)
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter(Collision collision)
    {
        // ���������� ó��.
        if (!IsServer) return;
        SpellManager.Instance.SpellHitOnServer(collision, this);
    }

    // ���� �󼼰� ����
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
