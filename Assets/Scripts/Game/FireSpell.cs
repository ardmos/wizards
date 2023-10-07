using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 불의 주문들의 공통적인 기능을 관리하는 클래스 
/// !!! 현재 기능
/// 1. 상세 속성값은 각 마법스펠 본인들에서 정의하도록 함
/// 2. 속성별 충돌 계산
/// </summary>
public abstract class FireSpell : Spell
{
    public struct CollisionCalcResult
    {
        Spell.SpellType spellType;
        int level;
    }

    /// <summary>
    /// 상세 속성값은 각 마법스펠 본인들에서 정의
    /// </summary>
    public abstract void InitSpellDataDetail();

    // 속성별 충돌 계산 구현할 차례. 여기선 Lvl와 Type만 반환하고 나머지 속성값은 현 스크립트를 호출한 각 마법스펠에서 입력해 사용한다. (보통은 기존의 본인들 스탯을 그대로 사용하게됨)
    public CollisionCalcResult CollisionHandling(Spell thisSpell, Spell opponentsSpell)
    {
        CollisionCalcResult result = new CollisionCalcResult();

        swit


        return result;
    }
}
