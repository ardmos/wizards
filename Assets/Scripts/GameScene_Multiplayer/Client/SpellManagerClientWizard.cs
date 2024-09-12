using UnityEngine;

/// <summary>
/// Wizard 캐릭터의 클라이언트 측 스펠 매니저 클래스입니다.
/// </summary>
public class SpellManagerClientWizard : SpellManagerClient
{
    #region Normal Attack Spells
    /// <summary>
    /// 일반 공격 마법 캐스팅을 시작합니다.
    /// </summary>
    /// <param name="spellIndex">스펠 인덱스</param>
    public void CastingNormalSpell(ushort spellIndex)
    {
        if (skillInfoListOnClient[spellIndex].spellState != SpellState.Ready) return;

        // 서버에 마법 캐스팅 요청
        spellManagerServer.GetComponent<SpellManagerServerWizard>().CastingNormalSpellServerRPC(spellIndex);
    }

    /// <summary>
    /// 캐스팅 중인 일반 공격 마법을 발사합니다.
    /// </summary>
    /// <param name="spellIndex">스펠 인덱스</param>
    public void ReleaseNormalSpell(ushort spellIndex)
    {
        if (skillInfoListOnClient[spellIndex].spellState != SpellState.Casting) return;

        // 서버에 마법 발사 요청
        spellManagerServer.GetComponent<SpellManagerServerWizard>().ReleaseNormalSpellServerRPC(spellIndex) ;
    }
    #endregion

    #region Blizzard Spell
    /// <summary>
    /// Blizzard 스펠 캐스팅을 시작합니다.
    /// </summary>
    public void CastingBlizzard()
    {
        if (skillInfoListOnClient[2].spellState != SpellState.Ready) return;

        // 서버에 마법 캐스팅 요청
        spellManagerServer.GetComponent<SpellManagerServerWizard>().CastingBlizzardServerRPC();
    }

    /// <summary>
    /// Blizzard 스펠을 필드에 설치합니다.
    /// </summary>
    public void ReleaseBlizzard()
    {
        if (skillInfoListOnClient[2].spellState != SpellState.Casting) return;

        // 서버에 마법 발사 요청
        spellManagerServer.GetComponent<SpellManagerServerWizard>().ReleaseBlizzardServerRPC();
    }
    #endregion

    #region Defence Spell
    /// <summary>
    /// 방어막 스펠을 발동합니다.
    /// </summary>
    public void ActivateShield()
    {
        if (skillInfoListOnClient[SpellManagerServer.DEFENCE_SPELL_INDEX_DEFAULT].spellState != SpellState.Ready) return;

        // 서버에 마법 시전 요청
        spellManagerServer.GetComponent<SpellManagerServerWizard>().ActivateShieldServerRPC();
    }
    #endregion
}
