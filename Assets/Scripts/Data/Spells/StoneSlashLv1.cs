using UnityEngine;
/// <summary>
/// 1레벨 한손검 베기 스킬 스크립트입니다.  
/// 속성 : Stone
/// 
/// 테스트용.
/// 추후 Knight 직업 손볼 때 다듬을 필요 있음.
/// </summary>
public class StoneSlashLv1 : AttackSpell  // 임시로 사용. Knight용 스킬 클래스를 따로 만들어줘야한다!
{
    /// <summary>
    /// 1. 상세 능력치 설정(마법 사용시에 Server에서 부여해주는 능력치 입니다.)
    /// </summary>
/*    public override void InitSpellInfoDetail(SpellInfo spellInfoFromServer)
    {
        Debug.Log($"InitSpellInfoDetail() {nameof(StoneSlashLv1)}");
        base.InitSpellInfoDetail( spellInfoFromServer );

        if (spellInfo == null)
        {
            Debug.Log("StoneSlashLv1 AttackSpell Info is null");
        }
        else
        {
            Debug.Log("StoneSlashLv1 AttackSpell Info is not null");
            Debug.Log($"StoneSlashLv1 spell Type : {spellInfo.spellType}, level : {spellInfo.level}");
        }
    }*/

    // 속성별 충돌 결과를 계산해주는 메소드 <--- Slash 계열 스크립트를 따로 만들어서 빼야할지도 모르겠다. 다른 fire,water,ice 처럼
    public override SpellInfo CollisionHandling(SpellInfo thisSpell, SpellInfo opponentsSpell)
    {
        SpellInfo result = new SpellInfo();

        // Lvl 비교
        byte resultLevel = (byte)(thisSpell.damage - opponentsSpell.damage);
        result.damage = resultLevel;
        // resultLevel 값이 0보다 같거나 작으면 더 계산할 필요 없음. 
        //      0이면 비긴거니까 만들 필요 없고
        //      마이너스면 진거니까 만들 필요 없음.
        //      현 메소드를 호출하는 각 마법 스크립트에서는 resultLevel값에 따라 후속 마법 오브젝트 생성여부를 판단하면 됨. 
        if (resultLevel <= 0)
        {
            return result;
        }
        // resultLevel값이 0보다 큰 경우는 내가 이긴 경우. 노말타입은 노말을 반환한다.
        result.spellType = SpellType.Stone;
        result.spellName = Spell.GetSpellName(result.damage, result.spellType);
        return result;
    }
}
