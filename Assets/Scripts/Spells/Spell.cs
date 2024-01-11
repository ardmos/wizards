using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 마법의 공통적인 기능들을 관리하는 클래스
/// </summary>
public abstract class Spell : NetworkBehaviour
{
    public SpellInfo spellInfo;

    [SerializeField] protected GameObject muzzleVFXPrefab;
    [SerializeField] protected GameObject hitVFXPrefab;
    [SerializeField] protected List<GameObject> trails;

    [SerializeField] protected bool collided, isSpellCollided;
    [SerializeField] protected SpellInfo collisionHandlingResult;
    
    // 마법 상세값 설정
    public abstract void InitSpellInfoDetail();

    // 마법 충돌시 속성 계산
    public abstract SpellInfo CollisionHandling(SpellInfo thisSpell, SpellInfo opponentsSpell);

    // 마법 캐스팅 시작
    public virtual void CastSpell(SpellInfo spellInfo, NetworkObject player)
    {
        SpellManager.Instance.SpawnSpellObject(spellInfo, player);
    }

    public GameObject GetMuzzleVFXPrefab()
    {
        return muzzleVFXPrefab;
    }
    public GameObject GetHitVFXPrefab()
    {
        return hitVFXPrefab;
    }
    /// <summary>
    /// 2. CollisionEnter 충돌 처리 (서버 권한 방식)
    /// </summary>
    /// <param name="collision"></param>
    public virtual void SpellHit(Collision collision)
    {
        if (IsClient) return;   // 서버에서만 처리할것.
        if (collided) return;

        // 일단 마법 정지
        GetComponent<Rigidbody>().isKinematic = true;

        Debug.Log($"collision.gameObject.tag: {collision.gameObject.tag}");

        // 충돌한게 Spell일 경우, 스펠간 충돌 결과 처리를 따로 진행합니다
        // 처리결과를 collisionHandlingResult 변수에 저장해뒀다가 DestroyParticle시에 castSpell 시킵니다.
        if (collision.gameObject.tag == "Spell")
        {
            isSpellCollided = true;
            Debug.Log($"Spell Hit!");
            SpellInfo thisSpell = spellInfo;
            SpellInfo opponentsSpell = collision.gameObject.GetComponent<Spell>().spellInfo;

            collisionHandlingResult = CollisionHandling(thisSpell, opponentsSpell);
        }
        // 충돌한게 플레이어일 경우! 
        // 1. player에게 GetHit() 시킴
        // 2. player가 GameMultiplayer(ServerRPC)에게 보고, player(ClientRPC)업데이트.
        else if (collision.gameObject.tag == "Player")
        {
            isSpellCollided = false;
            Debug.Log($"Player Hit!");

            if (spellInfo == null)
            {
                Debug.Log("Spell Info is null");
            }
            //Debug.Log($"Hit!! spell level: {spellInfo.level}");

            Player player = collision.gameObject.GetComponent<Player>();
            if (player != null)
            {
                sbyte damage = (sbyte)spellInfo.level;
                //Debug.Log("Spell.GotHit()");
                SpellManager.Instance.PlayerGotHitOnServer(damage, player.GetComponent<NetworkObject>().OwnerClientId);
            }
            else Debug.LogError("Player is null!");
        }
        else
        {
            isSpellCollided = false;
            Debug.Log($"Object Hit!");
        }
        collided = true;

        if (trails.Count > 0)
        {
            for (int i = 0; i < trails.Count; i++)
            {
                /// 이부분 에러. 이 에러때문에 플레이어 충돌시 오브젝트가 사라지지 않는다. 
                trails[i].transform.parent = null;
                ParticleSystem particleSystem = trails[i].GetComponent<ParticleSystem>();
                if (particleSystem != null)
                {
                    particleSystem.Stop();
                    Destroy(particleSystem.gameObject, particleSystem.main.duration + particleSystem.main.startLifetime.constantMax);
                }
            }
        }

        // 적중 효과 VFX
        SpellManager.Instance.HitVFX(hitVFXPrefab, collision);

        Debug.Log($"Collided Obejct name: {collision.gameObject.name}");
        //StartCoroutine(DestroyParticle(1f));
        Destroy(gameObject, 0.2f);

        if (isSpellCollided)
        {
            Debug.Log("collisionHandlingResult.level : " + collisionHandlingResult.level);
            if (collisionHandlingResult.level > 0)
            {
                CastSpell(collisionHandlingResult, gameObject.GetComponent<NetworkObject>());
            }
        }

        // 스크롤을 통한 스펠 추가 능력치. 게임씬에서 나가면 초기화되어야함. <<<--- 여기서 하는게 맞나?



    }
    // 아래 효과 안씀
    /*protected IEnumerator DestroyParticle(float waitTime)
    {

        if (transform.childCount > 0 && waitTime != 0)
        {
            List<Transform> tList = new List<Transform>();

            foreach (Transform t in transform.GetChild(0).transform)
            {
                tList.Add(t);
            }

            while (transform.GetChild(0).localScale.x > 0)
            {
                yield return new WaitForSeconds(0.01f);
                transform.GetChild(0).localScale -= new Vector3(0.1f, 0.1f, 0.1f);
                for (int i = 0; i < tList.Count; i++)
                {
                    tList[i].localScale -= new Vector3(0.1f, 0.1f, 0.1f);
                }
            }
        }

        yield return new WaitForSeconds(waitTime);
        Destroy(gameObject);

        if (isSpellCollided)
        {
            Debug.Log("collisionHandlingResult.level : " + collisionHandlingResult.level);
            if (collisionHandlingResult.level > 0)
            {
                CastSpell(collisionHandlingResult, gameObject.GetComponent<NetworkObject>());
            }
        }
    }*/
}
