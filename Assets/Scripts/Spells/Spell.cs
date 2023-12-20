using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 마법의 공통적인 기능들을 관리하는 클래스
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

    // 마법 상세값 설정
    public abstract void InitSpellInfoDetail();

    // 마법 충돌시 속성 계산
    public abstract SpellLvlType CollisionHandling(SpellLvlType thisSpell, SpellLvlType opponentsSpell);

    // 마법 발사
    public virtual void CastSpell(SpellLvlType spellLvlType, NetworkObject muzzle)
    {
        SpellManager.instance.SpawnSpellObject(spellLvlType, muzzle);
    }
    // 마법 발사시 VFX. 지금은 상대방건 안보임. 상대방것도 보이게 하려면 발사체처럼 Server에서 NetworkObject로 Spawn 해줘야함.
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
