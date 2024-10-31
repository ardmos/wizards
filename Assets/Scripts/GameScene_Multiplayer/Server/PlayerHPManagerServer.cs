using System.Collections;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Player HP��  Server Auth ������� �����Ҽ� �ֵ��� �����ִ� ��ũ��Ʈ �Դϴ�.
/// �������� �����մϴ�.
/// �� �÷��̾�� ĳ���� ������Ʈ�� ������Ʈ�� �ٿ��� ����մϴ�
/// </summary>
public class PlayerHPManagerServer : NetworkBehaviour
{
    #region Constants
    private const float DOT_DAMAGE_INTERVAL = 1f;
    #endregion

    #region Fields & Components
    public PlayerClient playerClient;
    public PlayerServer playerServer;
    public PlayerAnimator playerAnimator;
    public PlayerHPManagerClient playerHPManagerClient;
    private PlayerInGameData playerData;
    #endregion

    #region Initialization
    /// <summary>
    /// �÷��̾��� HP�� �ʱ�ȭ�մϴ�.
    /// </summary>
    /// <param name="character">ĳ���� ����</param>
    public void InitPlayerHP(ICharacter character)
    {
        if (character == null) return;
        if (playerHPManagerClient == null) return;

        if (IsHost)
        {
            InitializeForSingleplayer(character);
        }
        else
        {
            InitializeForMultiplayer(character);
        }

        playerHPManagerClient.UpdatePlayerHPClientRPC(playerData.hp, playerData.maxHp);
    }

    /// <summary>
    /// �̱��÷��̾� ��带 ���� �ʱ�ȭ �޼��� �Դϴ�.
    /// </summary>
    /// <param name="character"></param>
    private void InitializeForSingleplayer(ICharacter character)
    {
        if (character == null) return;
        if (GameSingleplayer.Instance == null) return;

        playerData = GameSingleplayer.Instance.GetPlayerDataFromClientId(OwnerClientId);
        playerData.hp = character.hp;
        playerData.maxHp = character.maxHp;
        GameSingleplayer.Instance.SetPlayerDataFromClientId(OwnerClientId, playerData);
    }

    /// <summary>
    /// ��Ƽ �÷��̾� ��带 ���� �ʱ�ȭ �޼��� �Դϴ�.
    /// </summary>
    /// <param name="character"></param>
    private void InitializeForMultiplayer(ICharacter character)
    {
        if (character == null) return;
        if (ServerNetworkConnectionManager.Instance == null) return;

        playerData = CurrentPlayerDataManager.Instance.GetPlayerDataByClientId(OwnerClientId);
        playerData.hp = character.hp;
        playerData.maxHp = character.maxHp;
        CurrentPlayerDataManager.Instance.SetPlayerDataByClientId(OwnerClientId, playerData);
    }
    #endregion

    #region HP Management
    /// <summary>
    /// ���� �������ִ� �޼����Դϴ�.
    /// </summary>
    /// <param name="healingValue">ȸ����</param>
    public void ApplyHeal(sbyte healingValue)
    {
        sbyte newHP = (sbyte)Mathf.Min(playerData.hp + healingValue, playerData.maxHp);

        UpdatePlayerHP(newHP);
    }

    /// <summary>
    /// ������� �������ִ� �޼����Դϴ�.
    /// </summary>
    /// <param name="damage">��������</param>
    /// <param name="attackerClientId">�������� Ŭ���̾�Ʈ ID</param>
    public void TakeDamage(sbyte damage, ulong attackerClientId)
    {
        if (!IsGamePlaying()) return;
        if (playerData.playerGameState != PlayerGameState.Playing) return;

        sbyte newPlayerHP = (sbyte)Mathf.Max(playerData.hp - damage, 0);
        if (newPlayerHP == 0)
        {
            HandlePlayerGameOver(attackerClientId);
        }
        else
        {
            PlayPlayerHitAnimation();
        }

        UpdatePlayerHP(newPlayerHP);
        HandlePlayerHitEffects(damage);
    }

    /// <summary>
    /// ��Ʈ ������� �������ִ� �޼����Դϴ�.
    /// </summary>
    /// <param name="damagePerSecond">�� �� �������</param>
    /// <param name="duration">�� ���� �ð�</param>
    /// <param name="attackerClientId">������ ID</param>
    public void StartToTakeDotDamage(sbyte damagePerSecond, float duration, ulong attackerClientId)
    {
        StartCoroutine(TakeDamageOverTime(damagePerSecond, duration, attackerClientId));
    }

    public IEnumerator TakeDamageOverTime(sbyte damagePerSecond, float duration, ulong attackerClientId)
    {
        float elapsed = 0;
        while (elapsed < duration)
        {
            // 1�� ���
            yield return new WaitForSeconds(1);

            TakeDamage(damagePerSecond, attackerClientId);

            elapsed += 1;
        }
    }

    /// <summary>
    /// �÷��̾��� HP���� ������Ʈ���ִ� �޼����Դϴ�.
    /// </summary>
    /// <param name="newHP">���ο� HP��</param>
    private void UpdatePlayerHP(sbyte newHP)
    {
        if (playerHPManagerClient == null) return;
        if (IsHost && GameSingleplayer.Instance == null) return;
        if (!IsHost && ServerNetworkConnectionManager.Instance == null) return;

        playerData.hp = newHP;
        if (IsHost)
        {
            GameSingleplayer.Instance.SetPlayerDataFromClientId(OwnerClientId, playerData);
        }
        else
        {
            CurrentPlayerDataManager.Instance.SetPlayerDataByClientId(OwnerClientId, playerData);
        }

        playerHPManagerClient.UpdatePlayerHPClientRPC(playerData.hp, playerData.maxHp);
    }
    #endregion

    #region Visual Effects
    /// <summary>
    /// �÷��̾��� �ǰ� ����Ʈ�� �������ִ� �޼����Դϴ�.
    /// </summary>
    /// <param name="damage">�÷��̾� ĳ������ �Ӹ� ���� ����� ������� �Դϴ�.</param>
    private void HandlePlayerHitEffects(sbyte damage)
    {
        if (playerHPManagerClient == null) return;

        playerHPManagerClient.HandlePlayerHitEffectsClientRPC(damage);
    }

    /// <summary>
    /// �÷��̾��� �ǰ� �ִϸ��̼��� �������ִ� �޼����Դϴ�.
    /// </summary>
    private void PlayPlayerHitAnimation()
    {
        if(playerAnimator ==  null) return;

        playerAnimator.UpdatePlayerAnimationOnServer(PlayerMoveAnimState.Hit);
    }
    #endregion

    #region Game Over Handling
    /// <summary>
    /// �÷��̾��� ���ӿ��� ó���� ����ϴ� �޼����Դϴ�.
    /// </summary>
    /// <param name="attackerClientId">�������� clientId</param>
    private void HandlePlayerGameOver(ulong attackerClientId)
    {
        if (playerServer == null) return;

        playerData.playerGameState = PlayerGameState.GameOver;
        playerServer.GameOver(attackerClientId);
    }
    #endregion

    #region Player Data
    /// <summary>
    /// ������ �÷��� ���θ� ��ȯ���ִ� �޼����Դϴ�.
    /// </summary>
    /// <returns>GameState�� Playing���� ������ ��� bool��</returns>
    private bool IsGamePlaying()
    {
        if (IsHost && SingleplayerGameManager.Instance == null) return false;
        if (!IsHost && MultiplayerGameManager.Instance == null) return false;

        return IsHost ? SingleplayerGameManager.Instance.IsGamePlaying()
                      : MultiplayerGameManager.Instance.IsGamePlaying();
    }

    /// <summary>
    /// �÷��̾��� ���� HP���� ��ȯ���ִ� �޼����Դϴ�.
    /// </summary>
    /// <returns>���� HP��</returns>
    public sbyte GetHP()
    {
        return playerData.hp;
    }
    #endregion
}