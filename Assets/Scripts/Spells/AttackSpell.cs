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

    [Header("AI가 피격됐을 시 타겟으로 설정될 마법을 소유한 플레이어 오브젝트.")]
    public GameObject spellOwnerObject;

    // 마법 충돌시 속성 계산
    public abstract SpellInfo CollisionHandling(SpellInfo thisSpell, SpellInfo opponentsSpell);

    // 충돌한게 Spell일 경우, 스펠간 충돌 결과 처리를 따로 진행합니다
    // 처리결과를 collisionHandlingResult 변수에 저장해뒀다가 DestroyParticle시에 castSpell 시킵니다.
    protected void SpellHitHandlerOnServer(Collider collider)
    {
        // 충돌 처리결과 저장
        SpellInfo collisionHandlingResult = null;

        // 일단 마법 정지
        gameObject.GetComponent<Rigidbody>().isKinematic = true;

        SpellInfo thisSpell = GetSpellInfo();
        SpellInfo opponentsSpell = collider.GetComponent<AttackSpell>().GetSpellInfo();

        collisionHandlingResult = CollisionHandling(thisSpell, opponentsSpell);

        //Debug.Log($"Spell끼리 충돌! ourSpell: name{thisSpell.spellName}, lvl{thisSpell.level}, owner{thisSpell.ownerPlayerClientId}  // " +
            //$"opponentsSpell : name{opponentsSpell.spellName}, lvl{opponentsSpell.level}, owner{opponentsSpell.ownerPlayerClientId}");

        // 스펠끼리 충돌해서 우리 스펠이 이겼을 때 계산 결과에 따라 충돌 위치에 새로운 마법 생성. 
        if (collisionHandlingResult.damage > 0)
        {
            //Debug.Log($"our spell is win! generate spell.name:{collisionHandlingResult.spellName}, spell.level :{collisionHandlingResult.level}, spell.owner: {collisionHandlingResult.ownerPlayerClientId} ");
            SpawnSpellObjectOnServer(collisionHandlingResult, transform);
        }
    }

    // 적중효과 VFX
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
    /// 마법 생성하기.
    /// Spell끼리 충돌 이후 호출되는 메소드 입니다.
    /// 플레이어가 캐스팅한 마법을 발사하는 메소드는 아래 ServerRPC 메소드 입니다.
    /// </summary>
    /// 필요한것.
    ///     1. 포구 Transform
    ///     2. 쏠 발사체 SpellName
    public void SpawnSpellObjectOnServer(SpellInfo spellInfo, Transform spawnPosition)
    {
        // 포구에 마법 발사체 위치시키기
        GameObject spellObject = Instantiate(GameAssetsManager.Instance.GetSpellPrefab(spellInfo.spellName), spawnPosition.position, Quaternion.identity);
        spellObject.GetComponent<NetworkObject>().Spawn();
        spellObject.GetComponent<AttackSpell>().InitSpellInfoDetail(spellInfo, gameObject);
        //Debug.Log($"SpawnSpellObjectOnServer!! skillInfo.ownerClientId : {spellInfo.ownerPlayerClientId}, name:{spellInfo.spellName}, lvl:{spellInfo.level}");

        spellObject.transform.SetParent(GameManager.Instance.transform);

        // 마법 발사체 방향 조정하기
        spellObject.transform.forward = spawnPosition.forward;

        // 마법 발사
        float moveSpeed = spellInfo.moveSpeed;
        spellObject.GetComponent<Rigidbody>().AddForce(spellObject.transform.forward * moveSpeed, ForceMode.Impulse);
    }

    // 마법 상세값 설정
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
