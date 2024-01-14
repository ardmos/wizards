using UnityEngine;
/// <summary>
/// 1레벨 워터볼 스크립트입니다.
/// 
/// !!! 현재 기능
/// 1. 상세 능력치 설정
/// </summary>
public class WaterBallLv1 : WaterSpell
{
    /// <summary>
    /// 1. 상세 능력치 설정(마법 사용시에 Server에서 부여해주는 능력치 입니다.)
    /// </summary>
    public override void InitSpellInfoDetail(SpellInfo spellInfoFromServer)
    {
        spellInfo = spellInfoFromServer;
    }
}
