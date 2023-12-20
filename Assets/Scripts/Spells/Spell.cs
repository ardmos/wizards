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
}
