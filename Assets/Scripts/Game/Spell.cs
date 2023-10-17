using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 마법의 공통적인 기능들을 관리하는 클래스
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

    // 속성별 충돌 계산. 여기선 Lvl와 Type만 반환하고 나머지 속성값은 각 마법스펠에서 입력해 사용한다. (보통은 기존의 본인들 스탯을 그대로 사용하게됨)
    public abstract SpellLvlType CollisionHandling(SpellLvlType thisSpell, SpellLvlType opponentsSpell);

    public virtual void CastSpell(GameObject spellPrefab, Transform muzzle)
    {
        // 포구에 발사체 위치시키기
        GameObject spellObject = Instantiate(spellPrefab, muzzle.position, Quaternion.identity);
        // 아래 회전, 발사는 각 마법의 Move스크립트에서 처리. 여기서는 머즐에 생성만 해줌.
        // 플레이어가 보고있는 방향과 발사체가 바라보는 방향 일치시키기. 직진이나 유도 등 이동은 발사체가 알아서 함.
        spellObject.transform.forward = muzzle.forward;
        // 소환시에 Impulse 
        float speed = 35f;
        spellObject.GetComponent<Rigidbody>().AddForce(spellObject.transform.forward * speed, ForceMode.Impulse);
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
