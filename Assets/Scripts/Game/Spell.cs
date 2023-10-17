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

    public virtual void CastSpell(SpellLvlType spellLvlType, Transform muzzle)
    {
        // ������ �߻�ü ��ġ��Ű��
        GameObject spellObject = Instantiate(GetSpellObject(spellLvlType), muzzle.position, Quaternion.identity);
        // �Ʒ� ȸ��, �߻�� �� ������ Move��ũ��Ʈ���� ó��. ���⼭�� ���� ������ ����.
        // �÷��̾ �����ִ� ����� �߻�ü�� �ٶ󺸴� ���� ��ġ��Ű��. �����̳� ���� �� �̵��� �߻�ü�� �˾Ƽ� ��.
        spellObject.transform.forward = muzzle.forward;
        // ��ȯ�ÿ� Impulse 
        float speed = 35f;
        spellObject.GetComponent<Rigidbody>().AddForce(spellObject.transform.forward * speed, ForceMode.Impulse);
    }

    private GameObject GetSpellObject(SpellLvlType spellLvlType)
    {
        GameObject resultObject = null;
        GameAssets gameAssets = GameAssets.instantiate;

        switch (spellLvlType.level)
        {
            case 1:
                switch (spellLvlType.spellType)
                {
                    case SpellType.Fire:
                        resultObject = gameAssets.fireBall_1;
                        break;
                    case SpellType.Water:
                        resultObject = gameAssets.waterBall_1;
                        break;
                    case SpellType.Ice:
                        resultObject = gameAssets.iceBall_1;
                        break;
                    case SpellType.Lightning:
                        break;
                    case SpellType.Arcane:
                        break;
                    default:
                        break;
                }
                break;
            case 2:
                switch (spellLvlType.spellType)
                {
                    case SpellType.Fire:
                        break;
                    case SpellType.Water:
                        break;
                    case SpellType.Ice:
                        break;
                    case SpellType.Lightning:
                        break;
                    case SpellType.Arcane:
                        break;
                    default:
                        break;
                }
                break;
            case 3:
                switch (spellLvlType.spellType)
                {
                    case SpellType.Fire:
                        break;
                    case SpellType.Water:
                        break;
                    case SpellType.Ice:
                        break;
                    case SpellType.Lightning:
                        break;
                    case SpellType.Arcane:
                        break;
                    default:
                        break;
                }
                break;
            default:
                break;
        }

        return resultObject;
    }

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
