using Unity.Netcode;

public class PlayerHPManagerClient : NetworkBehaviour
{
    #region Fields & Components
    public PlayerClient playerClient;
    #endregion

    /// <summary>
    /// �÷��̾��� HP�� UI�� ������Ʈ�մϴ�.
    /// </summary>
    [ClientRpc]
    public void UpdatePlayerHPClientRPC(sbyte hp, sbyte maxHP)
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
