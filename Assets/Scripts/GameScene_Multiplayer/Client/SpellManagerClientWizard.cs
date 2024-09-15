using UnityEngine;

/// <summary>
/// Wizard ĳ������ Ŭ���̾�Ʈ �� ���� �Ŵ��� Ŭ�����Դϴ�.
/// </summary>
public class SpellManagerClientWizard : SpellManagerClient
{
    #region Normal Attack Spells
    /// <summary>
    /// �Ϲ� ���� ���� ĳ������ �����մϴ�.
    /// </summary>
    /// <param name="spellIndex">���� �ε���</param>
    public void CastingNormalSpell(ushort spellIndex)
    {
        if (skillInfoListOnClient[spellIndex].spellState != SpellState.Ready) return;

        // ������ ���� ĳ���� ��û
        spellManagerServer.GetComponent<SpellManagerServerWizard>().CastingNormalSpellServerRPC(spellIndex);
    }

    /// <summary>
    /// ĳ���� ���� �Ϲ� ���� ������ �߻��մϴ�.
    /// </summary>
    /// <param name="spellIndex">���� �ε���</param>
    public void ReleaseNormalSpell(ushort spellIndex)
    {
        if (skillInfoListOnClient[spellIndex].spellState != SpellState.Casting) return;

        // ������ ���� �߻� ��û
        spellManagerServer.GetComponent<SpellManagerServerWizard>().ReleaseNormalSpellServerRPC(spellIndex) ;
    }
    #endregion

    #region Blizzard Spell
    /// <summary>
    /// Blizzard ���� ĳ������ �����մϴ�.
    /// </summary>
    public void CastingBlizzard()
    {
        if (skillInfoListOnClient[2].spellState != SpellState.Ready) return;

        // ������ ���� ĳ���� ��û
        spellManagerServer.GetComponent<SpellManagerServerWizard>().CastingBlizzardServerRPC();
    }

    /// <summary>
    /// Blizzard ������ �ʵ忡 ��ġ�մϴ�.
    /// </summary>
    public void ReleaseBlizzard()
    {
        if (skillInfoListOnClient[2].spellState != SpellState.Casting) return;

        // ������ ���� �߻� ��û
        spellManagerServer.GetComponent<SpellManagerServerWizard>().ReleaseBlizzardServerRPC();
    }
    #endregion

    #region Defence Spell
    /// <summary>
    /// �� ������ �ߵ��մϴ�.
    /// </summary>
    public void ActivateShield()
    {
        if (skillInfoListOnClient[SpellManagerServer.DEFENCE_SPELL_INDEX_DEFAULT].spellState != SpellState.Ready) return;

        // ������ ���� ���� ��û
        spellManagerServer.GetComponent<SpellManagerServerWizard>().ActivateShieldServerRPC();
    }
    #endregion
}
