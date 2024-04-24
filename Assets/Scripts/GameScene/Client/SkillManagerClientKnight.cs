using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SkillManagerClientKnight : SkillSpellManagerClient
{
    // 0. ��ų ����
    public void AimingSkill(ushort skillIndex)
    {
        if (skillInfoListOnClient[skillIndex].spellState != SpellState.Ready) return;
        skillSpellManagerServer.GetComponent<SkillManagerServerKnight>().SetPlayerSpellStateToCastingServerRPC(skillIndex);
    }

    // 1.  ���� ��ų ����
    public void ActivateAttackSkillOnClient(ushort skillIndex)
    {
        Debug.Log($"ActivateAttackSkillOnClient skillIndex:{skillIndex}, skillstate:{skillInfoListOnClient[skillIndex].spellState}");
        if (skillInfoListOnClient[skillIndex].spellState != SpellState.Casting) return;
        // ������ ���� ��ų ���� ��û
        skillSpellManagerServer.GetComponent<SkillManagerServerKnight>().StartAttackSkillServerRPC(skillIndex);
    }

    // 2. ��� ��ų ���� (�뽬)
    public void ActivateDefenceSkillOnClient()
    {
        if (skillInfoListOnClient[SkillSpellManagerServer.DEFENCE_SPELL_INDEX_DEFAULT].spellState != SpellState.Casting) return;

        // ������ ��� ��ų ���� ��û
        skillSpellManagerServer.GetComponent<SkillManagerServerKnight>().StartDefenceSkillServerRPC();
    }

    
}
