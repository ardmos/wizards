using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor.Experimental.GraphView;
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

        // 충돌을 중복 처리하는것을 방지하기 위한 처리
        GetComponent<Collider>().enabled = false;

        Collider collider = collision.collider;

        // 충돌한게 공격마법일 경우, 어떤 마법이 살아남을지 계산에 들어감
        if (collider.CompareTag("AttackSpell"))
        {
            SpellHitHandlerOnServer(collider);
        }
        // 충돌한게 플레이어일 경우, 플레이어의 피격 사실을 해당 플레이어의 SpellManager 알립니다. 
        else if (collider.CompareTag("Player"))
        {
            if (GetSpellInfo() == null)
            {
                Debug.Log("AttackSpell Info is null");
            }

            PlayerClient player = collider.GetComponent<PlayerClient>();
            if (player != null)
            {
                byte damage = (byte)GetSpellInfo().level;
                // 플레이어 피격을 서버에서 처리
                // 여기부터 하면 됩니다.  Player Server에서 하면 좋을듯 싶은데! PlayerGotHitOnServer(damage, player);
            }
            else Debug.LogError("Player is null!");
        }
        // 기타 오브젝트 충돌
        else
        {
            Debug.Log($"{collider.name} Hit!");
        }

        // 마법 충돌 사운드 재생
        PlaySFX(sfxHit);

        // 적중 효과 VFX
        HitVFX(GetHitVFXPrefab(), collision);

        Destroy(gameObject, 0.2f);
    }

    // 충돌한게 Spell일 경우, 스펠간 충돌 결과 처리를 따로 진행합니다
    // 처리결과를 collisionHandlingResult 변수에 저장해뒀다가 DestroyParticle시에 castSpell 시킵니다.
    private void SpellHitHandlerOnServer(Collider collider)
    {
        // 충돌 처리결과 저장
        SpellInfo collisionHandlingResult = null;

        // 일단 마법 정지
        gameObject.GetComponent<Rigidbody>().isKinematic = true;

        SpellInfo thisSpell = GetSpellInfo();
        SpellInfo opponentsSpell = collider.GetComponent<AttackSpell>().GetSpellInfo();

        collisionHandlingResult = CollisionHandling(thisSpell, opponentsSpell);

        Debug.Log($"Spell끼리 충돌! ourSpell: name{thisSpell.spellName}, lvl{thisSpell.level}, owner{thisSpell.ownerPlayerClientId}  // " +
            $"opponentsSpell : name{opponentsSpell.spellName}, lvl{opponentsSpell.level}, owner{opponentsSpell.ownerPlayerClientId}");

        // 스펠끼리 충돌해서 우리 스펠이 이겼을 때 계산 결과에 따라 충돌 위치에 새로운 마법 생성. 
        if (collisionHandlingResult.level > 0)
        {
            Debug.Log($"our spell is win! generate spell.name:{collisionHandlingResult.spellName}, spell.level :{collisionHandlingResult.level}, spell.owner: {collisionHandlingResult.ownerPlayerClientId} ");
            SpawnSpellObjectOnServer(collisionHandlingResult, transform);
        }
    }

    // 적중효과 VFX
    public void HitVFX(GameObject hitVFXPrefab, Collision collision)
    {
        ContactPoint contact = collision.contacts[0];
        Quaternion rot = Quaternion.FromToRotation(Vector3.up, contact.normal);
        Vector3 pos = contact.point;

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
        GameObject spellObject = Instantiate(GameAssets.instantiate.GetSpellPrefab(spellInfo.spellName), spawnPosition.position, Quaternion.identity);
        spellObject.GetComponent<NetworkObject>().Spawn();
        spellObject.GetComponent<AttackSpell>().InitSpellInfoDetail(spellInfo);
        Debug.Log($"SpawnSpellObjectOnServer!! spellInfo.ownerClientId : {spellInfo.ownerPlayerClientId}, name:{spellInfo.spellName}, lvl:{spellInfo.level}");

        // 마법 발사체 방향 조정하기
        spellObject.transform.forward = spawnPosition.forward;

        // 마법 발사
        float moveSpeed = spellInfo.moveSpeed;
        spellObject.GetComponent<Rigidbody>().AddForce(spellObject.transform.forward * moveSpeed, ForceMode.Impulse);
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
        if (SoundManager.Instance == null) return;

        SoundManager.Instance.PlayMagicSFXClientRPC(spellInfo.spellName, state);
    }
}
