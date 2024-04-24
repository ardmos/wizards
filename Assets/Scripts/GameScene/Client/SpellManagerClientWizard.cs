using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
/// <summary>
/// �⺻ Wizard_Male �÷��̾� ĳ���� ������Ʈ�� ���̴� ��ũ��Ʈ
/// ���� ���
///   1. ���� ĳ���� ���� ���� ��Ȳ ����
///   2. ĳ���� ���� ���� �ߵ� 
///   3. ���� ���� ������ ���� ������ ����
///   4. ���� ĳ�������� ���� ������Ʈ ����
///   
/// </summary>
public class SpellManagerClientWizard : SkillSpellManagerClient
{
    #region ���� ���� ĳ����&�߻�
    /// <summary>
    /// ���� ĳ���� ������ ������ ��û�մϴ�. Ŭ���̾�Ʈ���� �����ϴ� �޼ҵ��Դϴ�.
    /// </summary>
    /// <param name="spellIndex"></param>
    public void CastingSpell(ushort spellIndex)
    {
        if (spellInfoListOnClient[spellIndex].spellState != SpellState.Ready) return;

        // ������ ���� ĳ���� ��û
        skillSpellManagerServer.GetComponent<SpellManagerServerWizard>().CastingSpellServerRPC(spellIndex);
    }

    /// <summary>
    /// ĳ�������� ���� �߻�. Ŭ���̾�Ʈ���� �����ϴ� �޼ҵ�
    /// </summary>
    public void ShootSpell(ushort spellIndex)
    {
        if (spellInfoListOnClient[spellIndex].spellState != SpellState.Casting) return;   // ���⶧���ΰ� Ȯ���غ���

        // ������ ���� �߻� ��û
        skillSpellManagerServer.GetComponent<SpellManagerServerWizard>().ShootSpellServerRPC(spellIndex) ;
    }
    #endregion

    #region ��� ���� ����
    public void ActivateDefenceSpellOnClient()
    {
        if (spellInfoListOnClient[SkillSpellManagerServer.DEFENCE_SPELL_INDEX_DEFAULT].spellState != SpellState.Ready) return;

        // ������ ���� ���� ��û
        skillSpellManagerServer.GetComponent<SpellManagerServerWizard>().StartActivateDefenceSpellServerRPC();
    }
    #endregion
}
