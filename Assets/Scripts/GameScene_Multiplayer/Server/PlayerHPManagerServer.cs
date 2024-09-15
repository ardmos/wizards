using System.Collections;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Player HP를  Server Auth 방식으로 관리할수 있도록 도와주는 스크립트 입니다.
/// 서버에서 동작합니다.
/// 각 플레이어블 캐릭터 오브젝트의 컴포넌트로 붙여서 사용합니다
/// </summary>
public class PlayerHPManagerServer : NetworkBehaviour
{
    #region Constants
    private const float DOT_DAMAGE_INTERVAL = 1f;
    #endregion

    #region Fields
    private PlayerInGameData playerData;
    public PlayerClient playerClient;
    public PlayerServer playerServer;
    public PlayerAnimator playerAnimator;
    #endregion

    #region Initialization
    /// <summary>
    /// 플레이어의 HP를 초기화합니다.
    /// </summary>
    /// <param name="character">캐릭터 정보</param>
    public void InitPlayerHP(ICharacter character)
    {
        if (character == null)
        {
            Debug.LogError("Character information is null");
            return;
        }

        if (IsHost)
        {
            InitializeForSingleplayer(character);
        }
        else
        {
            InitializeForMultiplayer(character);
        }

        playerClient.UpdateHPBarClientRPC(playerData.hp, playerData.maxHp);
    }

    /// <summary>
    /// 싱글플레이어 모드를 위한 초기화 메서드 입니다.
    /// </summary>
    /// <param name="character"></param>
    private void InitializeForSingleplayer(ICharacter character)
    {
        playerData = GameSingleplayer.Instance.GetPlayerDataFromClientId(OwnerClientId);
        playerData.hp = character.hp;
        playerData.maxHp = character.maxHp;
        GameSingleplayer.Instance.SetPlayerDataFromClientId(OwnerClientId, playerData);
    }

    /// <summary>
    /// 멀티 플레이어 모드를 위한 초기화 메서드 입니다.
    /// </summary>
    /// <param name="character"></param>
    private void InitializeForMultiplayer(ICharacter character)
    {
        playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(OwnerClientId);
        playerData.hp = character.hp;
        playerData.maxHp = character.maxHp;
        GameMultiplayer.Instance.SetPlayerDataFromClientId(OwnerClientId, playerData);
    }
    #endregion

    #region HP Management
    /// <summary>
    /// 힐을 적용해주는 메서드입니다.
    /// </summary>
    /// <param name="healingValue">회복량</param>
    public void ApplyHeal(sbyte healingValue)
    {
        sbyte newHP = (sbyte)Mathf.Min(playerData.hp + healingValue, playerData.maxHp);

        UpdatePlayerHP(newHP);
    }

    /// <summary>
    /// 대미지를 적용해주는 메서드입니다.
    /// </summary>
    /// <param name="damage">데미지량</param>
    /// <param name="attackerClientId">공격자의 클라이언트 ID</param>
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
    /// 도트 대미지를 적용해주는 메서드입니다.
    /// </summary>
    /// <param name="damagePerSecond">초 당 대미지값</param>
    /// <param name="duration">총 지속 시간</param>
    /// <param name="attackerClientId">공격자 ID</param>
    public void StartToTakeDotDamage(sbyte damagePerSecond, float duration, ulong attackerClientId)
    {
        StartCoroutine(TakeDamageOverTime(damagePerSecond, duration, attackerClientId));
    }

    public IEnumerator TakeDamageOverTime(sbyte damagePerSecond, float duration, ulong attackerClientId)
    {
        float elapsed = 0;
        while (elapsed < duration)
        {
            // 1초 대기
            yield return new WaitForSeconds(1);

            TakeDamage(damagePerSecond, attackerClientId);

            elapsed += 1;
        }
    }

    /// <summary>
    /// 플레이어의 HP값을 업데이트해주는 메서드입니다.
    /// </summary>
    /// <param name="newHP">새로운 HP값</param>
    private void UpdatePlayerHP(sbyte newHP)
    {
        playerData.hp = newHP;

        if (IsHost)
        {
            GameSingleplayer.Instance.SetPlayerDataFromClientId(OwnerClientId, playerData);
        }
        else
        {
            GameMultiplayer.Instance.SetPlayerDataFromClientId(OwnerClientId, playerData);
        }
        
        playerClient.UpdateHPBarClientRPC(playerData.hp, playerData.maxHp);
    }
    #endregion

    #region Visual Effects
    /// <summary>
    /// 플레이어의 피격 이펙트를 실행해주는 메서드입니다.
    /// </summary>
    /// <param name="damage">플레이어 캐릭터의 머리 위에 띄워줄 대미지값 입니다.</param>
    private void HandlePlayerHitEffects(sbyte damage)
    {
        // 피격 카메라 테두리 효과 실행
        playerClient.ActivateHitByAttackCameraEffectClientRPC();
        // 피격 카메라 쉐이크 효과 실행
        playerClient.ActivateHitByAttackCameraShakeClientRPC();
        // 피격 대미지 숫자 표시 실행
        playerClient.ShowDamageTextPopupClientRPC(damage);
        // 각 Client의 쉐이더 피격 이펙트 실행 ClientRPC
        playerClient.ActivateHitByAttackEffectClientRPC();
    }

    /// <summary>
    /// 플레이어의 피격 애니메이션을 실행해주는 메서드입니다.
    /// </summary>
    private void PlayPlayerHitAnimation()
    {
        playerAnimator.UpdatePlayerAnimationOnServer(PlayerMoveAnimState.Hit);
    }
    #endregion

    #region Game Over Handling
    /// <summary>
    /// 플레이어의 게임오버 처리를 담당하는 메서드입니다.
    /// </summary>
    /// <param name="attackerClientId">공격자의 clientId</param>
    private void HandlePlayerGameOver(ulong attackerClientId)
    {
        playerData.playerGameState = PlayerGameState.GameOver;
        playerServer.GameOver(attackerClientId);
    }
    #endregion

    #region Player Data
    /// <summary>
    /// 게임의 플레이 여부를 반환해주는 메서드입니다.
    /// </summary>
    /// <returns>GameState가 Playing인지 정보가 담긴 bool값</returns>
    private bool IsGamePlaying()
    {
        return IsHost ? SingleplayerGameManager.Instance.IsGamePlaying()
                      : MultiplayerGameManager.Instance.IsGamePlaying();
    }

    /// <summary>
    /// 플레이어의 현재 HP값을 반환해주는 메서드입니다.
    /// </summary>
    /// <returns>현재 HP값</returns>
    public sbyte GetHP()
    {
        return playerData.hp;
    }
    #endregion
}