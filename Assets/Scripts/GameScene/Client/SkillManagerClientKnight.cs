using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SkillManagerClientKnight : SkillSpellManagerClient
{

    // 1.  공격 스킬 시전
    public void ActivateAttackSkillOnClient()
    {

    }

    // 2. 방어 스킬 시전 (대쉬)
    public void ActivateDefenceSkillOnClient()
    {
/*        if (spellInfoListOnClient[defenceSpellIndex].spellState != SpellState.Ready) return;*/

        // 서버에 마법 시전 요청
        //skillSpellManagerServer.GetComponent<SkillManagerServerKnight>().StartActivateDefenceSpellServerRPC(spellInfoListOnClient[defenceSpellIndex].spellName);
/*        // 해당 SpellState 업데이트
        skillSpellManagerServer.UpdatePlayerSpellStateServerRPC(defenceSpellIndex, SpellState.Cooltime);
        // 서버에 애니메이션 실행 요청
        GameMultiplayer.Instance.UpdatePlayerAttackAnimStateOnServerRPC(OwnerClientId, PlayerAttackAnimState.CastingDefensiveMagic);*/
    }
}
