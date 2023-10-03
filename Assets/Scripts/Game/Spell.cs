using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 마법의 공통적인 속성들을 가진 클래스
/// </summary>
public abstract class Spell : MonoBehaviour
{
    public SpellData data;
    bool castAble = true;
    float restTime = 0;

    /// <summary>
    /// 각 스펠의 디테일값은 스펠 본인들이 상속받아서 정의
    /// </summary>
    public abstract void InitSpellDetail();

    /// <summary>
    /// 각 플레이어의 스펠 캐스트 컨트롤러의 Update 메소드에서 입력처리 결과에 따라 호출될 메소드.
    /// </summary>
    /// <param name="muzzle"> 마법이 발사될 위치 </param>
    public virtual void CastSpell(Transform muzzle)
    {
        if (castAble)  // inputSystem을 통한 입력 체크는 MagicSpellCastController에서 합니다. 스킬 버튼이 여럿이기 때문.<--- 여기부터 하면 됨   각 스펠들 스크립트, 웨폰컨트롤러.
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

    /// <summary>
    /// 충돌 처리. 여기서 각 속성에 따른 결과값을 반환한다.     // <---  반환값 void인데 바꿔줘야함.
    /// </summary>
    public virtual void CollisionHandling()
    {
        
    }
}
