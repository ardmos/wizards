using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Spell;
using static UnityEngine.ParticleSystem;
/// <summary>
/// 
/// 1레벨 파이어볼 스크립트입니다.
/// 
/// !!! 현재 기능
/// 1. 상세 능력치 설정
/// 2. CollisionEnter 충돌 처리
/// 3. 마법 시전
/// </summary>
public class IceBallLv1 : IceSpell
{
    [SerializeField] private Sprite iconImage;
    [SerializeField] private GameObject muzzlePrefab;
    [SerializeField] private GameObject hitPrefab;
    [SerializeField] private List<GameObject> trails;

    [SerializeField] private bool collided, isSpellCollided;
    [SerializeField] private SpellLvlType collisionHandlingResult;

    /// <summary>
    /// 1. 상세 능력치 설정
    /// </summary>
    public override void InitSpellInfoDetail()
    {
        spellInfo = new SpellInfo()
        {
            spellType = SpellType.Ice,
            coolTime = 2.0f,
            lifeTime = 10.0f,
            moveSpeed = 10.0f,
            price = 30,
            level = 1,
            spellName = "IceBall Lv.1",
            castAble = true,
            iconImage = iconImage
        };
    }

    /// <summary>
    /// 2. CollisionEnter 충돌 처리
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter(Collision collision)
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

            if (hitPrefab != null)
            {
                var hitVFX = Instantiate(hitPrefab, pos, rot) as GameObject;

                var ps = hitVFX.GetComponent<ParticleSystem>();
                if (ps == null)
                {
                    var psChild = hitVFX.transform.GetChild(0).GetComponent<ParticleSystem>();
                    Destroy(hitVFX, psChild.main.duration);
                }
                else
                    Destroy(hitVFX, ps.main.duration);
            }

            StartCoroutine(DestroyParticle(0f));
        }
    }
    public IEnumerator DestroyParticle(float waitTime)
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
                CastSpell(collisionHandlingResult, gameObject.transform);
            }
        }
    }

    /// <summary>
    /// 3. 마법 시전
    /// </summary>
    public override void CastSpell(SpellLvlType spellLvlType, Transform muzzle)
    {
        base.CastSpell(spellLvlType, muzzle);
        MuzzleVFX(muzzlePrefab, muzzle);
    }

    public override void MuzzleVFX(GameObject muzzlePrefab, Transform muzzle)
    {
        base.MuzzleVFX(muzzlePrefab, muzzle);
    }
}
