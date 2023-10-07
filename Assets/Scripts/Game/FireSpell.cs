using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 불의 주문들의 공통적인 기능을 관리하는 클래스 
/// !!! 현재 기능
/// 1. 상세 속성값은 각 마법스펠 본인들에서 정의하도록 함
/// </summary>
public abstract class FireSpell : Spell
{
    /// <summary>
    /// 상세 속성값은 각 마법스펠 본인들에서 정의
    /// </summary>
    public abstract void InitSpellDataDetail();


}
