using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SkillManagerClientKnight : SkillSpellManagerClient
{

    // 1.  ���� ��ų ����
    public void ActivateAttackSkillOnClient()
    {

    }

    // 2. ��� ��ų ���� (�뽬)
    public void ActivateDefenceSkillOnClient()
    {
/*        if (spellInfoListOnClient[defenceSpellIndex].spellState != SpellState.Ready) return;*/

        // ������ ���� ���� ��û
        //skillSpellManagerServer.GetComponent<SkillManagerServerKnight>().StartActivateDefenceSpellServerRPC(spellInfoListOnClient[defenceSpellIndex].spellName);
/*        // �ش� SpellState ������Ʈ
        skillSpellManagerServer.UpdatePlayerSpellStateServerRPC(defenceSpellIndex, SpellState.Cooltime);
        // ������ �ִϸ��̼� ���� ��û
        GameMultiplayer.Instance.UpdatePlayerAttackAnimStateOnServerRPC(OwnerClientId, PlayerAttackAnimState.CastingDefensiveMagic);*/
    }
}
