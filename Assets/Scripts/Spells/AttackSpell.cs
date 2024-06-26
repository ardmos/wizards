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

    [Header("AI�� �ǰݵ��� �� Ÿ������ ������ ������ ������ �÷��̾� ������Ʈ.")]
    public GameObject spellOwnerObject;

    // ���� �浹�� �Ӽ� ���
    public abstract SpellInfo CollisionHandling(SpellInfo thisSpell, SpellInfo opponentsSpell);

    // �浹�Ѱ� Spell�� ���, ���簣 �浹 ��� ó���� ���� �����մϴ�
    // ó������� collisionHandlingResult ������ �����ص״ٰ� DestroyParticle�ÿ� castSpell ��ŵ�ϴ�.
    protected void SpellHitHandlerOnServer(Collider collider)
    {
        // �浹 ó����� ����
        SpellInfo collisionHandlingResult = null;

        // �ϴ� ���� ����
        gameObject.GetComponent<Rigidbody>().isKinematic = true;

        SpellInfo thisSpell = GetSpellInfo();
        SpellInfo opponentsSpell = collider.GetComponent<AttackSpell>().GetSpellInfo();

        collisionHandlingResult = CollisionHandling(thisSpell, opponentsSpell);

        //Debug.Log($"Spell���� �浹! ourSpell: name{thisSpell.spellName}, lvl{thisSpell.level}, owner{thisSpell.ownerPlayerClientId}  // " +
            //$"opponentsSpell : name{opponentsSpell.spellName}, lvl{opponentsSpell.level}, owner{opponentsSpell.ownerPlayerClientId}");

        // ���糢�� �浹�ؼ� �츮 ������ �̰��� �� ��� ����� ���� �浹 ��ġ�� ���ο� ���� ����. 
        if (collisionHandlingResult.damage > 0)
        {
            //Debug.Log($"our spell is win! generate spell.name:{collisionHandlingResult.spellName}, spell.level :{collisionHandlingResult.level}, spell.owner: {collisionHandlingResult.ownerPlayerClientId} ");
            SpawnSpellObjectOnServer(collisionHandlingResult, transform);
        }
    }

    // ����ȿ�� VFX
    public void HitVFX(GameObject hitVFXPrefab, Collision collision)
    {
        ContactPoint contact = collision.contacts[0];
        Quaternion rot = Quaternion.FromToRotation(Vector3.up, contact.normal);
        float positionYAdjustment = 1f;
        float positionZAdjustment = -1f;
        Vector3 pos = new Vector3(contact.point.x, contact.point.y + positionYAdjustment, contact.point.z + positionZAdjustment);

        if (hitVFXPrefab != null)
        {
            //Debug.Log($"hitVFXPrefab is Not null");
            var hitVFX = Instantiate(hitVFXPrefab, pos, rot) as GameObject;
            hitVFX.GetComponent<NetworkObject>().Spawn();
            var particleSystem = hitVFX.GetComponent<ParticleSystem>();
            if (particleSystem == null)
            {
                particleSystem = hitVFX.transform.GetChild(0).GetComponent<ParticleSystem>();
            }
            Destroy(hitVFX, particleSystem.main.duration);
        }
        else Debug.Log($"hitVFXPrefab is null");
    }

    public void HitVFX(GameObject hitVFXPrefab)
    {
        Quaternion rot = Quaternion.FromToRotation(Vector3.up, Vector3.forward);
        float positionYAdjustment = 1f;
        float positionZAdjustment = -1f;
        Vector3 pos = new Vector3(transform.position.x, transform.position.y + positionYAdjustment, transform.position.z + positionZAdjustment);

        if (hitVFXPrefab != null)
        {
            //Debug.Log($"hitVFXPrefab is Not null");
            var hitVFX = Instantiate(hitVFXPrefab, pos, rot) as GameObject;
            hitVFX.GetComponent<NetworkObject>().Spawn();
            var particleSystem = hitVFX.GetComponent<ParticleSystem>();
            if (particleSystem == null)
            {
                particleSystem = hitVFX.transform.GetChild(0).GetComponent<ParticleSystem>();
            }
            Destroy(hitVFX, particleSystem.main.duration);
        }
        else Debug.Log($"hitVFXPrefab is null");
    }

    /// <summary>
    /// ���� �����ϱ�.
    /// Spell���� �浹 ���� ȣ��Ǵ� �޼ҵ� �Դϴ�.
    /// �÷��̾ ĳ������ ������ �߻��ϴ� �޼ҵ�� �Ʒ� ServerRPC �޼ҵ� �Դϴ�.
    /// </summary>
    /// �ʿ��Ѱ�.
    ///     1. ���� Transform
    ///     2. �� �߻�ü SpellName
    public void SpawnSpellObjectOnServer(SpellInfo spellInfo, Transform spawnPosition)
    {
        // ������ ���� �߻�ü ��ġ��Ű��
        GameObject spellObject = Instantiate(GameAssetsManager.Instance.GetSpellPrefab(spellInfo.spellName), spawnPosition.position, Quaternion.identity);
        spellObject.GetComponent<NetworkObject>().Spawn();
        spellObject.GetComponent<AttackSpell>().InitSpellInfoDetail(spellInfo, gameObject);
        //Debug.Log($"SpawnSpellObjectOnServer!! skillInfo.ownerClientId : {spellInfo.ownerPlayerClientId}, name:{spellInfo.spellName}, lvl:{spellInfo.level}");

        spellObject.transform.SetParent(GameManager.Instance.transform);

        // ���� �߻�ü ���� �����ϱ�
        spellObject.transform.forward = spawnPosition.forward;

        // ���� �߻�
        float moveSpeed = spellInfo.moveSpeed;
        spellObject.GetComponent<Rigidbody>().AddForce(spellObject.transform.forward * moveSpeed, ForceMode.Impulse);
    }

    // ���� �󼼰� ����
    public virtual void InitSpellInfoDetail(SpellInfo spellInfoFromServer, GameObject spellOwnerObject)
    {
        if (IsClient) return;

        //spellInfo = new SpellInfo(spellInfoFromServer);
        spellInfo = spellInfoFromServer;
        //Debug.Log($"spellInfo:{spellInfo.upgradeOptions.Length}, spellInfoFromServer:{spellInfoFromServer}");
        this.spellOwnerObject = spellOwnerObject;
    }

    public virtual void Shoot(Vector3 force, ForceMode forceMode)
    {
        if (IsClient) return;

        GetComponent<Rigidbody>().AddForce(force, forceMode);
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
