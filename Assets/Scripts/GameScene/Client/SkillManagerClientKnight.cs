using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SkillManagerClientKnight : SkillSpellManagerClient
{
    // 0. 쿨타임 관리


    // 1.  공격 스킬 시전
    public void ActivateAttackSkillOnClient()
    {

    }

    // 2. 방어 스킬 시전 (대쉬)
    public void ActivateDefenceSkillOnClient()
    {
        if (spellInfoListOnClient[SkillSpellManagerServer.DEFENCE_SPELL_INDEX_DEFAULT].spellState != SpellState.Ready) return;

        // 서버에 마법 시전 요청
        skillSpellManagerServer.GetComponent<SkillManagerServerKnight>().StartActivateDefenceSpellServerRPC();
    }

    
}
