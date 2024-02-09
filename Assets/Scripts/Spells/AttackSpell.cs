using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// ���� ������ �������� ��ɵ��� �����ϴ� Ŭ����
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

    // ���� �浹�� �Ӽ� ���
    public abstract SpellInfo CollisionHandling(SpellInfo thisSpell, SpellInfo opponentsSpell);

    /// <summary>
    /// 2. CollisionEnter �浹 ó�� (���� ���� ���)
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter(Collision collision)
    {
        // ���������� ó��.
        if (IsClient) return;

        SpellManager.Instance.SpellHitOnServer(collision, this);
        // ���� �浹 ���� ���
        PlaySFX(sfxHit);
    }

    // ���� ĳ���� ���۽� �󼼰� ����
    public virtual void InitSpellInfoDetail(SpellInfo spellInfoFromServer)
    {
        if (IsClient) return;

        spellInfo = new SpellInfo(spellInfoFromServer);
        // ���� ���� ���� ���
        PlaySFX(sfxCasting);
    }

    public virtual void Shoot(Vector3 force, ForceMode forceMode)
    {
        if (IsClient) return;

        GetComponent<Rigidbody>().AddForce(force, forceMode);
        // ���� �߻� ���� ���
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
