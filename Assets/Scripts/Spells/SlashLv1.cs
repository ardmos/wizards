using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 1���� �Ѽհ� ���� ��ų ��ũ��Ʈ�Դϴ�.  
/// �Ӽ� : �븻
/// </summary>
public class SlashLv1 : Spell
{
    [SerializeField] private Sprite iconImage;
    [SerializeField] private GameObject hitPrefab;

    [SerializeField] private bool collided, isSpellCollided;
    [SerializeField] private SpellLvlType collisionHandlingResult;

    private void Start()
    {
        // ���������̴ϱ� ���� ª��
        Destroy(gameObject,1f);
    }

    /// <summary>
    /// 1. �� �ɷ�ġ ����
    /// </summary>
    public override void InitSpellInfoDetail()
    {
        Debug.Log($"InitSpellInfoDetail() {nameof(SlashLv1)}");
        spellInfo = new SpellInfo()
        {
            spellType = SpellType.Normal,
            coolTime = 2.0f,
            lifeTime = 10.0f,
            moveSpeed = 10.0f,
            price = 30,
            level = 1,
            spellName = "Slash Lv1",
            castAble = true,
            iconImage = iconImage
        };

        if (spellInfo == null)
        {
            Debug.Log("Spell Info is null");
        }
        else
        {
            Debug.Log("Spell Info is not null");
            Debug.Log($"spell Type : {spellInfo.spellType}, level : {spellInfo.level}");
        }
    }

    // �Ӽ��� �浹 ����� ������ִ� �޼ҵ�
    public override SpellLvlType CollisionHandling(SpellLvlType thisSpell, SpellLvlType opponentsSpell)
    {
        SpellLvlType result = new SpellLvlType();

        // Lvl ��
        int resultLevel = thisSpell.level - opponentsSpell.level;
        result.level = resultLevel;
        // resultLevel ���� 0���� ���ų� ������ �� ����� �ʿ� ����. 
        //      0�̸� ���Ŵϱ� ���� �ʿ� ����
        //      ���̳ʽ��� ���Ŵϱ� ���� �ʿ� ����.
        //      �� �޼ҵ带 ȣ���ϴ� �� ���� ��ũ��Ʈ������ resultLevel���� ���� �ļ� ���� ������Ʈ �������θ� �Ǵ��ϸ� ��. 
        if (resultLevel <= 0)
        {
            return result;
        }
        // resultLevel���� 0���� ū ���� ���� �̱� ���. �븻Ÿ���� �븻�� ��ȯ�Ѵ�.
        result.spellType = SpellType.Normal;
        return result;
    }


    /// <summary>
    /// 2. CollisionEnter �浹 ó��
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
                    player.GetHit(spellInfo.level);
                }
                else Debug.LogError("Player is null!");
            }
            else { isSpellCollided = false; }

            collided = true;

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
    /// 3. ���� ����
    /// </summary>
    public override void CastSpell(SpellLvlType spellLvlType, Transform muzzle)
    {
        base.CastSpell(spellLvlType, muzzle);
    }
}
