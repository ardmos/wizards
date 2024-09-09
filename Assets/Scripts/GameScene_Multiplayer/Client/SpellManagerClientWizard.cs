using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
/// <summary>
/// 기본 Wizard_Male 플레이어 캐릭터 오브젝트에 붙이는 스크립트
/// 현재 기능
///   1. 현재 캐릭터 마법 보유 현황 관리
///   2. 캐릭터 보유 마법 발동 
///   3. 현재 보유 마법에 대한 정보를 공유
///   4. 현재 캐스팅중인 마법 오브젝트 관리
///   
/// </summary>
public class SpellManagerClientWizard : SkillSpellManagerClient
{
    public void CastingBlizzard()
    {
        if (skillInfoListOnClient[2].spellState != SpellState.Ready) return;

        // 서버에 마법 캐스팅 요청
        skillSpellManagerServer.GetComponent<SpellManagerServerWizard>().CastingBlizzardServerRPC();
    }

    public void SetBlizzard()
    {
        if (skillInfoListOnClient[2].spellState != SpellState.Aiming) return;

        // 서버에 마법 발사 요청
        skillSpellManagerServer.GetComponent<SpellManagerServerWizard>().SetBlizzardServerRPC();
    }

    #region 공격 마법 캐스팅&발사
    /// <summary>
    /// 마법 캐스팅 시작을 서버에 요청합니다. 클라이언트에서 동작하는 메소드입니다.
    /// </summary>
    /// <param name="spellIndex"></param>
    public void CastingNormalSpell(ushort spellIndex)
    {
        Debug.Log($"3. skillInfoListOnClient.Count:{skillInfoListOnClient.Count}, spellIndex:{spellIndex}");
        if (skillInfoListOnClient[spellIndex].spellState != SpellState.Ready) return;

        // 서버에 마법 캐스팅 요청
        skillSpellManagerServer.GetComponent<SpellManagerServerWizard>().CastingSpellServerRPC(spellIndex);
    }

    /// <summary>
    /// 캐스팅중인 마법 발사. 클라이언트에서 동작하는 메소드
    /// </summary>
    public void ShootNormalSpell(ushort spellIndex)
    {
        if (skillInfoListOnClient[spellIndex].spellState != SpellState.Aiming) return;

        // 서버에 마법 발사 요청
        skillSpellManagerServer.GetComponent<SpellManagerServerWizard>().ShootSpellServerRPC(spellIndex) ;
    }
    #endregion

    #region 방어 마법 시전
    public void ActivateDefenceSpellOnClient()
    {
        if (skillInfoListOnClient[SkillSpellManagerServer.DEFENCE_SPELL_INDEX_DEFAULT].spellState != SpellState.Ready) return;

        // 서버에 마법 시전 요청
        skillSpellManagerServer.GetComponent<SpellManagerServerWizard>().StartActivateDefenceSpellServerRPC();
    }
    #endregion
}
