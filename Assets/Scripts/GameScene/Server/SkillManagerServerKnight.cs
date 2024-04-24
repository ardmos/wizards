using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SkillManagerServerKnight : SkillSpellManagerServer
{
    public float dashDistance = 5f; // �뽬 �Ÿ�
    public float dashDuration = 0.5f; // �뽬 ���� �ð�

    /// <summary>
    /// ��� ���� ����
    /// </summary>
    /// <param name="player"></param>
    [ServerRpc(RequireOwnership = false)]
    public void StartActivateDefenceSpellServerRPC(SkillName spellName)
    {
        // ���� ����
        if(spellName == SkillName.Dash_Lv1)
        {
            Dash();
        }

        // State ������Ʈ
        UpdatePlayerSpellState(defenceSpellIndex, SpellState.Casting);

        // Anim ����
        playerAnimator.UpdateKnightMaleAnimationOnServer(KnightMaleAnimState.Dash);
    }

    public void Dash()
    {
        // �뽬 ���� ���� (���÷� ������ �뽬)
        Vector3 dashDirection = transform.forward;

        // �뽬 ����
        StartCoroutine(PerformDash(dashDirection));
    }

    private IEnumerator PerformDash(Vector3 dashDirection)
    {
        float startTime = Time.time;
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + dashDirection * dashDistance;

        // �뽬 ����
        while (Time.time < startTime + dashDuration)
        {
            float t = (Time.time - startTime) / dashDuration;
            transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }
    }
}
