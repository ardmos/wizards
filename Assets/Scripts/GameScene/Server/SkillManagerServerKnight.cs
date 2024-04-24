using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SkillManagerServerKnight : SkillSpellManagerServer
{
    public float dashDistance = 5f; // 대쉬 거리
    public float dashDuration = 0.5f; // 대쉬 지속 시간

    /// <summary>
    /// 방어 마법 시전
    /// </summary>
    /// <param name="player"></param>
    [ServerRpc(RequireOwnership = false)]
    public void StartActivateDefenceSpellServerRPC(SkillName spellName)
    {
        // 마법 시전
        if(spellName == SkillName.Dash_Lv1)
        {
            Dash();
        }

        // State 업데이트
        UpdatePlayerSpellState(defenceSpellIndex, SpellState.Casting);

        // Anim 실행
        playerAnimator.UpdateKnightMaleAnimationOnServer(KnightMaleAnimState.Dash);
    }

    public void Dash()
    {
        // 대쉬 방향 설정 (예시로 앞으로 대쉬)
        Vector3 dashDirection = transform.forward;

        // 대쉬 동작
        StartCoroutine(PerformDash(dashDirection));
    }

    private IEnumerator PerformDash(Vector3 dashDirection)
    {
        float startTime = Time.time;
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + dashDirection * dashDistance;

        // 대쉬 동작
        while (Time.time < startTime + dashDuration)
        {
            float t = (Time.time - startTime) / dashDuration;
            transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }
    }
}
