using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// ������ �������� ��ɵ��� �����ϴ� Ŭ����
/// </summary>
public abstract class Spell : NetworkBehaviour
{
    [SerializeField] protected SpellInfo spellInfo;

    [SerializeField] protected GameObject muzzleVFXPrefab;
    [SerializeField] protected GameObject hitVFXPrefab;
    [SerializeField] protected List<GameObject> trails;

    [SerializeField] private bool isCollided;
    
    // ���� �󼼰� ����
    public abstract void InitSpellInfoDetail(SpellInfo spellInfoFromServer);

    // ���� �浹�� �Ӽ� ���
    public abstract SpellInfo CollisionHandling(SpellInfo thisSpell, SpellInfo opponentsSpell);

    public void SetSpellIsCollided(bool isCollided)
    {
        this.isCollided = isCollided;
    }

    public bool IsCollided()
    {
        return this.isCollided;
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
}
