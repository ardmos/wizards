using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 발사 마법의 공통적인 속성들을 가진 클래스
/// </summary>
public abstract class ShootingSpell : MonoBehaviour
{
    public SpellData data;
    bool castBtnClicked = false;
    bool castAble = true;
    float restTime = 0;

    public abstract void InitSpellDetail();

    public virtual void CastSpell(Transform muzzle)
    {
        if (castBtnClicked && castAble)  ///  castBtnClicked 대신에 InputSystem 사용해야함.  지금부터 잠시 InputSystem 만들고 오자
        {
            GameObject spellObject = Instantiate(data.spellObject);
            spellObject.transform.position = muzzle.position;

            castAble = false;
        }
        if (castAble == false)
        {
            restTime += Time.deltaTime;
            if (restTime >= data.coolTime)
            {
                castAble = true;
                restTime = 0f;
            }
        }
    }
}
