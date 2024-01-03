using Unity.Netcode;
using UnityEngine;
/// <summary>
/// 1레벨 한손검 베기 스킬 스크립트입니다.  
/// 속성 : 노말
/// </summary>
public class SlashLv1 : Spell
{
    private void Start()
    {
        // 근접공격이니까 수명 짧게
        Destroy(gameObject,1f);
    }

    /// <summary>
    /// 1. 상세 능력치 설정
    /// </summary>
    public override void InitSpellInfoDetail()
    {
        Debug.Log($"InitSpellInfoDetail() {nameof(SlashLv1)}");
        spellInfo = new SpellInfo()
        {
            spellType = SpellType.Normal,
            coolTime = 2.0f,
            lifeTime = 10.0f,
            moveSpeed = 10.0f,
            price = 30,
            level = 1,
            spellName = SpellName.SlashLv1,
            spellState = SpellState.Ready,
        };

        if (spellInfo == null)
        {
            Debug.Log("SlashLv1 Spell Info is null");
        }
        else
        {
            Debug.Log("SlashLv1 Spell Info is not null");
            Debug.Log($"SlashLv1 spell Type : {spellInfo.spellType}, level : {spellInfo.level}");
        }
    }

    // 속성별 충돌 결과를 계산해주는 메소드 <--- Slash 계열 스크립트를 따로 만들어서 빼야할지도 모르겠다. 다른 fire,water,ice 처럼
    public override SpellInfo CollisionHandling(SpellInfo thisSpell, SpellInfo opponentsSpell)
    {
        SpellInfo result = new SpellInfo();

        // Lvl 비교
        int resultLevel = thisSpell.level - opponentsSpell.level;
        result.level = resultLevel;
        // resultLevel 값이 0보다 같거나 작으면 더 계산할 필요 없음. 
        //      0이면 비긴거니까 만들 필요 없고
        //      마이너스면 진거니까 만들 필요 없음.
        //      현 메소드를 호출하는 각 마법 스크립트에서는 resultLevel값에 따라 후속 마법 오브젝트 생성여부를 판단하면 됨. 
        if (resultLevel <= 0)
        {
            return result;
        }
        // resultLevel값이 0보다 큰 경우는 내가 이긴 경우. 노말타입은 노말을 반환한다.
        result.spellType = SpellType.Normal;
        return result;
    }


    /// <summary>
    /// 2. CollisionEnter 충돌 처리
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter(Collision collision)
    {
        SpellHit(collision);
        
    }

    /// <summary>
    /// 3. 마법 시전
    /// </summary>
    public override void CastSpell(SpellInfo spellInfo, NetworkObject player)
    {
        base.CastSpell(spellInfo, player);
    }
}
