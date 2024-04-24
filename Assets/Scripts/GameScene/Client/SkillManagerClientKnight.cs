using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SkillManagerClientKnight : SkillSpellManagerClient
{
    // 0. ��Ÿ�� ����


    // 1.  ���� ��ų ����
    public void ActivateAttackSkillOnClient()
    {

    }

    // 2. ��� ��ų ���� (�뽬)
    public void ActivateDefenceSkillOnClient()
    {
        if (spellInfoListOnClient[SkillSpellManagerServer.DEFENCE_SPELL_INDEX_DEFAULT].spellState != SpellState.Ready) return;

        // ������ ���� ���� ��û
        skillSpellManagerServer.GetComponent<SkillManagerServerKnight>().StartActivateDefenceSpellServerRPC();
    }

    
}
