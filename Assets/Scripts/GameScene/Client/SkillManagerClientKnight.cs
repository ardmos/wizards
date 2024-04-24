using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SkillManagerClientKnight : SkillSpellManagerClient
{
    // 0. 스킬 조준
    public void AimingSkill(ushort skillIndex)
    {
        if (skillInfoListOnClient[skillIndex].spellState != SpellState.Ready) return;
        skillSpellManagerServer.GetComponent<SkillManagerServerKnight>().SetPlayerSpellStateToCastingServerRPC(skillIndex);
    }

    // 1.  공격 스킬 시전
    public void ActivateAttackSkillOnClient(ushort skillIndex)
    {
        Debug.Log($"ActivateAttackSkillOnClient skillIndex:{skillIndex}, skillstate:{skillInfoListOnClient[skillIndex].spellState}");
        if (skillInfoListOnClient[skillIndex].spellState != SpellState.Casting) return;
        // 서버에 공격 스킬 시전 요청
        skillSpellManagerServer.GetComponent<SkillManagerServerKnight>().StartAttackSkillServerRPC(skillIndex);
    }

    // 2. 방어 스킬 시전 (대쉬)
    public void ActivateDefenceSkillOnClient()
    {
        if (skillInfoListOnClient[SkillSpellManagerServer.DEFENCE_SPELL_INDEX_DEFAULT].spellState != SpellState.Casting) return;

        // 서버에 방어 스킬 시전 요청
        skillSpellManagerServer.GetComponent<SkillManagerServerKnight>().StartDefenceSkillServerRPC();
    }

    
}
