using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerHPManagerClient : NetworkBehaviour
{
    #region Components
    public PlayerClient playerClient;
    #endregion

    [ClientRpc]
    public void UpdatePlayerHPClientRPC(sbyte hp, sbyte maxHP)
    {
        UpdateHPBarUI(hp, maxHP);
    }

    /// <summary>
    /// 플레이어의 HP바 UI를 업데이트합니다.
    /// </summary>
    private void UpdateHPBarUI(sbyte hp, sbyte maxHP)
    {
        if (playerClient.hPBarUIController == null) return;

        playerClient.hPBarUIController.SetHP(hp, maxHP);
    }

    /// <summary>
    /// 플레이어의 피격 이펙트를 실행해주는 메서드입니다.
    /// </summary>
    /// <param name="damage">플레이어 캐릭터의 머리 위에 띄워줄 대미지값 입니다.</param>
    [ClientRpc]
    public void HandlePlayerHitEffectsClientRPC(sbyte damage)
    {
        if(playerClient == null) return;

        // 피격 카메라 테두리 효과 실행
        playerClient.ActivateHitByAttackCameraEffect();
        // 피격 카메라 쉐이크 효과 실행
        playerClient.ActivateHitByAttackCameraShake();
        // 피격 대미지 숫자 표시 실행
        playerClient.ShowDamageTextPopup(damage);
        // 각 Client의 쉐이더 피격 이펙트 실행 ClientRPC
        playerClient.ActivateHitByAttackEffect();
    }
}
