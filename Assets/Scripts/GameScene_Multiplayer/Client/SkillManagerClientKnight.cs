using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SkillManagerClientKnight : SpellManagerClient
{
    public KnightBuzzMeshTrail meshTrail;

    // 0. ��ų ����
    public void AimingSkill(ushort skillIndex)
    {
        if (skillInfoListOnClient[skillIndex].spellState != SpellState.Ready) return;
        spellManagerServer.GetComponent<SkillManagerServerKnight>().ReadyAttackSkillServerRPC(skillIndex);
    }

    // 1.  ���� ��ų ����
    public void ActivateAttackSkillOnClient(ushort skillIndex)
    {
        //Debug.Log($"0.ActivateAttackSkillOnClient skillIndex:{skillIndex}, skillstate:{skillInfoListOnClient[skillIndex].spellState}");
        if (skillInfoListOnClient[skillIndex].spellState != SpellState.Casting) return;
        // ������ ���� ��ų ���� ��û
        spellManagerServer.GetComponent<SkillManagerServerKnight>().StartAttackSkillServerRPC(skillIndex);
    }

    // 2. ��� ��ų ���� (�뽬)
    public void ActivateDefenceSkillOnClient()
    {
        Debug.Log($"skillInfoListOnClient[spellManagerServer.DEFENCE_SPELL_INDEX_DEFAULT].spellState: {skillInfoListOnClient[SpellManagerServer.DEFENCE_SPELL_INDEX_DEFAULT].spellState}");

        //if (skillInfoListOnClient[SkillSpellManagerServer.DEFENCE_SPELL_INDEX_DEFAULT].spellState != SpellState.Aiming) return;
        if (skillInfoListOnClient[SpellManagerServer.DEFENCE_SPELL_INDEX_DEFAULT].spellState != SpellState.Ready) return;

        // ������ ��� ��ų ���� ��û
        spellManagerServer.GetComponent<SkillManagerServerKnight>().StartDefenceSkillServerRPC();
    }

    /// <summary>
    /// ��� Ʈ���� ȿ���� Ȱ��ȭ�մϴ�.
    /// </summary>
    [ClientRpc]
    public void ActivateDashTrailEffectClientRPC()
    {
        meshTrail.ActivateTrail();
    }
}
