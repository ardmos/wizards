/// <summary>
/// 기본 쉴드 스크립트 입니다. 전방위 방어가 가능한 쉴드 입니다.
/// </summary>
public class MagicShieldLv1 : DefenceSpell
{
    /// <summary>
    /// 1. 상세 능력치 설정(마법 사용시에 Server에서 부여해주는 능력치 입니다.)
    /// </summary>
    public override void InitSpellInfoDetail(SpellInfo spellInfoFromServer)
    {
        base.InitSpellInfoDetail(spellInfoFromServer);
    }

    public override void Activate()
    {
        base.Activate();
    }
}
