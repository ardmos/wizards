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
    /// �÷��̾��� HP�� UI�� ������Ʈ�մϴ�.
    /// </summary>
    private void UpdateHPBarUI(sbyte hp, sbyte maxHP)
    {
        if (playerClient.hPBarUIController == null) return;

        playerClient.hPBarUIController.SetHP(hp, maxHP);
    }

    /// <summary>
    /// �÷��̾��� �ǰ� ����Ʈ�� �������ִ� �޼����Դϴ�.
    /// </summary>
    /// <param name="damage">�÷��̾� ĳ������ �Ӹ� ���� ����� ������� �Դϴ�.</param>
    [ClientRpc]
    public void HandlePlayerHitEffectsClientRPC(sbyte damage)
    {
        if(playerClient == null) return;

        // �ǰ� ī�޶� �׵θ� ȿ�� ����
        playerClient.ActivateHitByAttackCameraEffect();
        // �ǰ� ī�޶� ����ũ ȿ�� ����
        playerClient.ActivateHitByAttackCameraShake();
        // �ǰ� ����� ���� ǥ�� ����
        playerClient.ShowDamageTextPopup(damage);
        // �� Client�� ���̴� �ǰ� ����Ʈ ���� ClientRPC
        playerClient.ActivateHitByAttackEffect();
    }
}
