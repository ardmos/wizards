using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SkillManagerClientKnight : SpellManagerClient
{
    public KnightBuzzMeshTrail meshTrail;

    // 0. 스킬 조준
    public void AimingSkill(ushort skillIndex)
    {
        if (skillInfoListOnClient[skillIndex].spellState != SpellState.Ready) return;
        spellManagerServer.GetComponent<SkillManagerServerKnight>().ReadyAttackSkillServerRPC(skillIndex);
    }

    // 1.  공격 스킬 시전
    public void ActivateAttackSkillOnClient(ushort skillIndex)
    {
        //Debug.Log($"0.ActivateAttackSkillOnClient skillIndex:{skillIndex}, skillstate:{skillInfoListOnClient[skillIndex].spellState}");
        if (skillInfoListOnClient[skillIndex].spellState != SpellState.Casting) return;
        // 서버에 공격 스킬 시전 요청
        spellManagerServer.GetComponent<SkillManagerServerKnight>().StartAttackSkillServerRPC(skillIndex);
    }

    // 2. 방어 스킬 시전 (대쉬)
    public void ActivateDefenceSkillOnClient()
    {
        Debug.Log($"skillInfoListOnClient[spellManagerServer.DEFENCE_SPELL_INDEX_DEFAULT].spellState: {skillInfoListOnClient[SpellManagerServer.DEFENCE_SPELL_INDEX_DEFAULT].spellState}");

        //if (skillInfoListOnClient[SkillSpellManagerServer.DEFENCE_SPELL_INDEX_DEFAULT].spellState != SpellState.Aiming) return;
        if (skillInfoListOnClient[SpellManagerServer.DEFENCE_SPELL_INDEX_DEFAULT].spellState != SpellState.Ready) return;

        // 서버에 방어 스킬 시전 요청
        spellManagerServer.GetComponent<SkillManagerServerKnight>().StartDefenceSkillServerRPC();
    }

    /// <summary>
    /// 대시 트레일 효과를 활성화합니다.
    /// </summary>
    [ClientRpc]
    public void ActivateDashTrailEffectClientRPC()
    {
        meshTrail.ActivateTrail();
    }
}
