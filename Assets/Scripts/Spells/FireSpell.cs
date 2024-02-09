using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 불의 주문들의 공통적인 기능을 관리하는 클래스 
/// !!! 현재 기능
/// 1. 속성별 충돌 계산
/// </summary>
public abstract class FireSpell : AttackSpell
{
    /// <summary>
    /// 불속성 마법과 다른 마법의 충돌 계산결과를 알려주는 메소드.
    /// </summary>
    public override SpellInfo CollisionHandling(SpellInfo thisSpell, SpellInfo opponentsSpell)
    {
        SpellInfo result = new SpellInfo(thisSpell);

        // Lvl 비교
        int resultLevel = thisSpell.level - opponentsSpell.level;

        // resultLevel 값이 0보다 같거나 작으면 더 계산할 필요 없음. 
        //      0이면 비긴거니까 만들 필요 없고
        //      마이너스면 진거니까 만들 필요 없음.
        //      현 메소드를 호출하는 각 마법 스크립트에서는 resultLevel값에 따라 후속 마법 오브젝트 생성여부를 판단하면 됨. 
        if (resultLevel <= 0)
        {
            result.level = 0;
            return result;
        }

        // resultLevel값이 0보다 큰 경우는 내가 이긴 경우. 상대 마법 속성별 경우의 수 만을 따져서 결과값을 반환한다.
        result.level = (byte)resultLevel;

        switch (opponentsSpell.spellType)
        {
            case SpellType.Fire:
            case SpellType.Water:
            case SpellType.Ice:
            case SpellType.Arcane:
            default:
                result.spellType = SpellType.Fire;         
                break;
            case SpellType.Lightning:
                result.spellType = SpellType.Arcane;
                break;
        }

        result.spellName = Spell.GetSpellName(result.level, result.spellType);
        return result;
    }
}
