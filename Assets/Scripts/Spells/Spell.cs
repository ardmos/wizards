using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// ������ �������� ��ɵ��� �����ϴ� Ŭ����
/// </summary>
public abstract class Spell : NetworkBehaviour
{
    public SpellInfo spellInfo;

    [SerializeField] protected GameObject muzzleVFXPrefab;
    [SerializeField] protected GameObject hitVFXPrefab;
    [SerializeField] protected List<GameObject> trails;

    [SerializeField] protected bool collided, isSpellCollided;
    [SerializeField] protected SpellInfo collisionHandlingResult;
    
    // ���� �󼼰� ����
    public abstract void InitSpellInfoDetail();

    // ���� �浹�� �Ӽ� ���
    public abstract SpellInfo CollisionHandling(SpellInfo thisSpell, SpellInfo opponentsSpell);

    // ���� ĳ���� ����
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
    /// 2. CollisionEnter �浹 ó�� (���� ���� ���)
    /// </summary>
    /// <param name="collision"></param>
    public virtual void SpellHit(Collision collision)
    {
        if (IsClient) return;   // ���������� ó���Ұ�.
        if (collided) return;

        // �ϴ� ���� ����
        GetComponent<Rigidbody>().isKinematic = true;

        Debug.Log($"collision.gameObject.tag: {collision.gameObject.tag}");

        // �浹�Ѱ� Spell�� ���, ���簣 �浹 ��� ó���� ���� �����մϴ�
        // ó������� collisionHandlingResult ������ �����ص״ٰ� DestroyParticle�ÿ� castSpell ��ŵ�ϴ�.
        if (collision.gameObject.tag == "Spell")
        {
            isSpellCollided = true;
            Debug.Log($"Spell Hit!");
            SpellInfo thisSpell = spellInfo;
            SpellInfo opponentsSpell = collision.gameObject.GetComponent<Spell>().spellInfo;

            collisionHandlingResult = CollisionHandling(thisSpell, opponentsSpell);
        }
        // �浹�Ѱ� �÷��̾��� ���! 
        // 1. player���� GetHit() ��Ŵ
        // 2. player�� GameMultiplayer(ServerRPC)���� ����, player(ClientRPC)������Ʈ.
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
                /// �̺κ� ����. �� ���������� �÷��̾� �浹�� ������Ʈ�� ������� �ʴ´�. 
                trails[i].transform.parent = null;
                ParticleSystem particleSystem = trails[i].GetComponent<ParticleSystem>();
                if (particleSystem != null)
                {
                    particleSystem.Stop();
                    Destroy(particleSystem.gameObject, particleSystem.main.duration + particleSystem.main.startLifetime.constantMax);
                }
            }
        }

        // ���� ȿ�� VFX
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

        // ��ũ���� ���� ���� �߰� �ɷ�ġ. ���Ӿ����� ������ �ʱ�ȭ�Ǿ����. <<<--- ���⼭ �ϴ°� �³�?



    }
    // �Ʒ� ȿ�� �Ⱦ�
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
