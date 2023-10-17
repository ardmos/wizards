using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// ������ �������� ��ɵ��� �����ϴ� Ŭ����
/// </summary>
public abstract class Spell : MonoBehaviour
{
    public struct SpellLvlType
    {
        public Spell.SpellType spellType;
        public int level;
    }

    public enum SpellType
    {
        Fire,
        Water,
        Ice,
        Lightning,
        Arcane
    }
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
    }

    public SpellInfo spellInfo;

    public abstract void InitSpellInfoDetail();

    // �Ӽ��� �浹 ���. ���⼱ Lvl�� Type�� ��ȯ�ϰ� ������ �Ӽ����� �� �������翡�� �Է��� ����Ѵ�. (������ ������ ���ε� ������ �״�� ����ϰԵ�)
    public abstract SpellLvlType CollisionHandling(SpellLvlType thisSpell, SpellLvlType opponentsSpell);

    public virtual void CastSpell(GameObject spellPrefab, Transform muzzle)
    {
        // Spell�� ���� ���� <--- �̰� ���߿�. �ϴ� �ٷ� �߻�ǵ��� ������.
        // 1. �߻� ���
        // 2. ���� 
        // 3. �߻�

        // #Test Code 10/14 : Generate Spell Object By Prefab
        GameObject spellObject = Instantiate(spellPrefab, muzzle.position, Quaternion.identity);
        spellObject.transform.forward = muzzle.forward;
        //spellObject.transform.localRotation = muzzle.transform.localRotation;
        // ???????? Impulse 
        float speed = 35f;
        spellObject.GetComponent<Rigidbody>().AddForce(spellObject.transform.forward * speed, ForceMode.Impulse);

        // #Test Code 10/15 : Adding Cool-Time System
        //spellInfo.castAble = false;
    }

    // ���� ��� : ���� ��ġ �缳�� �ʿ� ( �߻�ü�� �浹 ���ϰԲ� )
    public virtual void MuzzleVFX(GameObject muzzlePrefab, Transform muzzle)
    {
        if (muzzlePrefab != null)
        {
            GameObject muzzleVFX = Instantiate(muzzlePrefab, muzzle.position, Quaternion.identity);
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
}
