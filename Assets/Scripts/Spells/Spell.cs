using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// ������ �������� ��ɵ��� �����ϴ� Ŭ����
/// </summary>
public abstract class Spell : MonoBehaviour
{
    public class SpellInfo
    {
        public SpellType spellType;

        public float coolTime;
        public float lifeTime;
        public float moveSpeed;
        public int price;
        public int level;
        public string spellName;
        public bool castAble;
        public Sprite iconImage;
    }

    public SpellInfo spellInfo;

    [SerializeField] protected Sprite iconImage;
    [SerializeField] protected GameObject muzzleVFXPrefab;
    [SerializeField] protected GameObject hitVFXPrefab;
    [SerializeField] protected List<GameObject> trails;

    [SerializeField] protected bool collided, isSpellCollided;
    [SerializeField] protected SpellLvlType collisionHandlingResult;

    // ���� �󼼰� ����
    public abstract void InitSpellInfoDetail();

    // ���� �浹�� �Ӽ� ���
    public abstract SpellLvlType CollisionHandling(SpellLvlType thisSpell, SpellLvlType opponentsSpell);

    // ���� �߻�
    public virtual void CastSpell(SpellLvlType spellLvlType, NetworkObject muzzle)
    {
        SpellManager.instance.SpawnSpellObject(spellLvlType, muzzle);
    }
    // ���� �߻�� VFX. ������ ����� �Ⱥ���. ����͵� ���̰� �Ϸ��� �߻�üó�� Server���� NetworkObject�� Spawn �������.
    public virtual void MuzzleVFX(GameObject muzzlePrefab, NetworkObject muzzle)
    {
        if (muzzlePrefab != null)
        {
            GameObject muzzleVFX = Instantiate(muzzlePrefab, muzzle.GetComponent<Transform>().position, Quaternion.identity);
            muzzleVFX.transform.forward = gameObject.transform.forward;
            var ps = muzzleVFX.GetComponent<ParticleSystem>();
            if (ps != null)
                Destroy(muzzleVFX, ps.main.duration);
            else
            {
                var psChild = muzzleVFX.transform.GetChild(0).GetComponent<ParticleSystem>();
                Destroy(muzzleVFX, psChild.main.duration);
            }
        }
    }

    /// <summary>
    /// 2. CollisionEnter �浹 ó�� (���� ���� ���)
    /// </summary>
    /// <param name="collision"></param>
    public virtual void SpellHit(Collision collision)
    {
        if (!collided)
        {
            if (collision.gameObject.tag == "Spell")
            {
                isSpellCollided = true;
                SpellLvlType thisSpell = new SpellLvlType { level = spellInfo.level, spellType = spellInfo.spellType };
                SpellInfo opponentsSpellInfo = collision.gameObject.GetComponent<Spell>().spellInfo;
                SpellLvlType opponentsSpell = new SpellLvlType { level = opponentsSpellInfo.level, spellType = opponentsSpellInfo.spellType };

                collisionHandlingResult = CollisionHandling(thisSpell, opponentsSpell);
            }
            // �浹�Ѱ� �÷��̾��� ���! 
            // 1. player���� GetHit() ��Ŵ
            // 2. player�� GameMultiplayer(ServerRPC)���� ����, player(ClientRPC)������Ʈ.
            else if (collision.gameObject.tag == "Player")
            {
                isSpellCollided = false;

                if (spellInfo == null)
                {
                    Debug.Log("Spell Info is null");
                }
                Debug.Log($"Hit!! spell level: {spellInfo.level}");

                Player player = collision.gameObject.GetComponent<Player>();
                if (player != null)
                {
                    sbyte damage = (sbyte)spellInfo.level;
                    Debug.Log("Spell.GotHit()");
                    GameMultiplayer.Instance.PlayerGotHit(damage, player.GetComponent<NetworkObject>().OwnerClientId);
                }
                else Debug.LogError("Player is null!");
            }
            else { isSpellCollided = false; }
            collided = true;

            if (trails.Count > 0)
            {
                for (int i = 0; i < trails.Count; i++)
                {
                    trails[i].transform.parent = null;
                    var ps = trails[i].GetComponent<ParticleSystem>();
                    if (ps != null)
                    {
                        ps.Stop();
                        Destroy(ps.gameObject, ps.main.duration + ps.main.startLifetime.constantMax);
                    }
                }
            }

            GetComponent<Rigidbody>().isKinematic = true;

            ContactPoint contact = collision.contacts[0];
            Quaternion rot = Quaternion.FromToRotation(Vector3.up, contact.normal);
            Vector3 pos = contact.point;

            if (hitVFXPrefab != null)
            {
                var hitVFX = Instantiate(hitVFXPrefab, pos, rot) as GameObject;

                var ps = hitVFX.GetComponent<ParticleSystem>();
                if (ps == null)
                {
                    var psChild = hitVFX.transform.GetChild(0).GetComponent<ParticleSystem>();
                    Destroy(hitVFX, psChild.main.duration);
                }
                else
                    Destroy(hitVFX, ps.main.duration);
            }

            StartCoroutine(DestroyParticle(1f));
        }
    }
    protected IEnumerator DestroyParticle(float waitTime)
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
    }
}
