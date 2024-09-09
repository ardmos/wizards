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
    public void CastingBlizzard()
    {
        if (skillInfoListOnClient[2].spellState != SpellState.Ready) return;

        // ������ ���� ĳ���� ��û
        skillSpellManagerServer.GetComponent<SpellManagerServerWizard>().CastingBlizzardServerRPC();
    }

    public void SetBlizzard()
    {
        if (skillInfoListOnClient[2].spellState != SpellState.Aiming) return;

        // ������ ���� �߻� ��û
        skillSpellManagerServer.GetComponent<SpellManagerServerWizard>().SetBlizzardServerRPC();
    }

    #region ���� ���� ĳ����&�߻�
    /// <summary>
    /// ���� ĳ���� ������ ������ ��û�մϴ�. Ŭ���̾�Ʈ���� �����ϴ� �޼ҵ��Դϴ�.
    /// </summary>
    /// <param name="spellIndex"></param>
    public void CastingNormalSpell(ushort spellIndex)
    {
        Debug.Log($"3. skillInfoListOnClient.Count:{skillInfoListOnClient.Count}, spellIndex:{spellIndex}");
        if (skillInfoListOnClient[spellIndex].spellState != SpellState.Ready) return;

        // ������ ���� ĳ���� ��û
        skillSpellManagerServer.GetComponent<SpellManagerServerWizard>().CastingSpellServerRPC(spellIndex);
    }

    /// <summary>
    /// ĳ�������� ���� �߻�. Ŭ���̾�Ʈ���� �����ϴ� �޼ҵ�
    /// </summary>
    public void ShootNormalSpell(ushort spellIndex)
    {
        if (skillInfoListOnClient[spellIndex].spellState != SpellState.Aiming) return;

        // ������ ���� �߻� ��û
        skillSpellManagerServer.GetComponent<SpellManagerServerWizard>().ShootSpellServerRPC(spellIndex) ;
    }
    #endregion

    #region ��� ���� ����
    public void ActivateDefenceSpellOnClient()
    {
        if (skillInfoListOnClient[SkillSpellManagerServer.DEFENCE_SPELL_INDEX_DEFAULT].spellState != SpellState.Ready) return;

        // ������ ���� ���� ��û
        skillSpellManagerServer.GetComponent<SpellManagerServerWizard>().StartActivateDefenceSpellServerRPC();
    }
    #endregion
}
