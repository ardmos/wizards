using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 마법의 공통적인 기능들을 관리하는 클래스
/// </summary>
public abstract class Spell : MonoBehaviour
{
    public SpellData spellData;

    /// <summary>
    /// 각 스펠의 디테일값(spellData)은 스펠 본인들이 상속받아서 정의
    /// </summary>
    public virtual void InitSpellDetail() {
        spellData = new SpellData();
    }

    /// <summary>
    /// 각 플레이어의 스펠 캐스트 컨트롤러의 Update 메소드에서 입력처리 결과에 따라 호출될 메소드.
    /// </summary>
    /// <param name="muzzle"> 마법이 발사될 위치 </param>
    public virtual void CastSpell(Transform muzzle)
    {
        // 해야할 것
        // 1. 생성 위치
        // 2. 생성 방향(회전값)
        // 3. 생성 개수

        if (!spellData.castAble)
        {
            spellData.restTime += Time.deltaTime;
            if (spellData.restTime >= spellData.coolTime)
            {
                spellData.castAble = true;
                spellData.restTime = 0f;
            }
        }
        if (spellData.castAble)  // inputSystem을 통한 입력 체크는 MagicSpellCastController에서 합니다. 스킬 버튼이 여럿이기 때문.<---  각 스펠들 스크립트, 웨폰컨트롤러.
        {
            GameObject spellObject = Instantiate(spellData.spellObjectPref);
            spellObject.transform.position = muzzle.position;

            spellData.castAble = false;
        }
    }
}
