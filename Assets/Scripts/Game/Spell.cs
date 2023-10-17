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
        // Spell이 갖는 성질 <--- 이건 나중에. 일단 바로 발사되도록 만들어보자.
        // 1. 발사 대기
        // 2. 성장 
        // 3. 발사

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

    // 수정 요망 : 머즐 위치 재설정 필요 ( 발사체와 충돌 피하게끔 )
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
