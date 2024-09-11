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
    PlayerInGameData playerData;
    public PlayerClient playerClient;
    public PlayerServer playerServer;
    public PlayerAnimator playerAnimator;

    public void InitPlayerHP(ICharacter character)
    {
        if (IsHost)
        {
            playerData = GameSingleplayer.Instance.GetPlayerDataFromClientId(OwnerClientId);
            playerData.hp = character.hp;
            playerData.maxHp = character.maxHp;
            GameSingleplayer.Instance.SetPlayerDataFromClientId(OwnerClientId, playerData);

            playerClient.UpdateHPBarClientRPC(playerData.hp, playerData.maxHp);
        }
        else
        {
            playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(OwnerClientId);
            playerData.hp = character.hp;
            playerData.maxHp = character.maxHp;
            GameMultiplayer.Instance.SetPlayerDataFromClientId(OwnerClientId, playerData);

            playerClient.UpdateHPBarClientRPC(playerData.hp, playerData.maxHp);
        }
    }

    /// <summary>
    /// 서버에서 동작합니다
    /// </summary>
    /// <param name="healingValue"></param>
    public void ApplyHeal(sbyte healingValue)
    {
        // 힐 적용하는중
        //playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(OwnerClientId);
        sbyte newHP = (sbyte)(playerData.hp + healingValue);
        if (newHP > playerData.maxHp) newHP = playerData.maxHp;

        // 변경된 HP값 서버에 저장
        playerData.hp = newHP;
        if (IsHost)
        {
            GameSingleplayer.Instance.SetPlayerDataFromClientId(OwnerClientId, playerData);
        }
        else
        {
            GameMultiplayer.Instance.SetPlayerDataFromClientId(OwnerClientId, playerData);
        }

        // 각 Client 플레이어의 HP바 UI 업데이트 ClientRPC       
        playerClient.UpdateHPBarClientRPC(playerData.hp, playerData.maxHp);
    }

    /// <summary>
    /// 서버에서 호출해야하는 메서드. 서버에서 동작합니다.
    /// </summary>
    public void TakingDamage(sbyte damage, ulong clientWhoAttacked)
    {
        // GamePlaying중이 아니면 전부 리턴. 게임이 끝나면 무적처리 되도록.
        if(IsHost)
        {
            if (!SingleplayerGameManager.Instance.IsGamePlaying()) return;
        }
        else
        {
            if (!MultiplayerGameManager.Instance.IsGamePlaying()) return;
        }
        
        //playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(OwnerClientId);
        if (playerData.playerGameState != PlayerGameState.Playing) return;

        // 요청한 플레이어 현재 HP값 가져오기 
        sbyte newPlayerHP = playerData.hp;

        // HP보다 Damage가 클 경우(게임오버 처리는 Player에서 HP잔량 파악해서 알아서 한다.)
        if (newPlayerHP <= damage)
        {
            // HP 0
            newPlayerHP = 0;
            playerData.playerGameState = PlayerGameState.GameOver; // 아래에서 HP값을 저장할 때 새로 SetPlayerData를 하기 때문에...! 일단 여기서 한 번더 게임오버 스테이트를 저장해주고 있는데, GameOver()에서 이미 게임오버처리를 해주고 있다. 일단은 동작하지만 수정 필요.

            // 게임오버 처리
            playerServer.GameOver(clientWhoAttacked);
        }
        else
        {
            // HP 감소 계산
            newPlayerHP -= (sbyte)damage;

            // 피격 애니메이션 실행 Server
            playerAnimator.UpdatePlayerMoveAnimationOnServer(PlayerMoveAnimState.Hit);
        }

        // 변경된 HP값 서버에 저장
        playerData.hp = newPlayerHP;

        if (IsHost)
        {
            GameSingleplayer.Instance.SetPlayerDataFromClientId(OwnerClientId, playerData);
        }
        else
        {
            GameMultiplayer.Instance.SetPlayerDataFromClientId(OwnerClientId, playerData);
        }

        // 피격 카메라 효과 실행 ClientRPC
        playerClient.ActivateHitByAttackCameraEffectClientRPC();

        // 피격 카메라 쉐이크 효과 실행 ClientRPC
        playerClient.ActivateHitByAttackCameraShakeClientRPC();

        // 피격 사운드 효과 실행 ClientRPC

        // 피격 대미지 숫자 표시 실행. 각 Client Damage Text Popup UI 업데이트 지시 
        playerClient.ShowDamageTextPopupClientRPC(damage);

        // 각 Client 플레이어의 HP바 UI 업데이트 ClientRPC       
        playerClient.UpdateHPBarClientRPC(playerData.hp, playerData.maxHp);

        // 각 Client의 쉐이더 피격 이펙트 실행 ClientRPC
        playerClient.ActivateHitByAttackEffectClientRPC();
    }

    /// <summary>
    /// 스킬 충돌 처리(서버에서 동작)
    /// 플레이어 적중시 ( 다른 마법이나 구조물과의 충돌 처리는 Spell.cs에 있다. 코드 정리 필요)
    /// clientID와 HP 연계해서 처리. 
    /// 충돌 녀석이 플레이어일 경우 실행. 
    /// ClientID로 리스트 검색 후 HP 수정시키고 업데이트된 내용 브로드캐스팅.
    /// 수신측은 ClientID의 플레이어 HP 업데이트. 
    /// 서버에서 구동되는 스크립트.
    /// </summary>
    /// <param name="damage"></param>
    /// <param name="clientId"></param>
    public void TakingDamageWithCameraShake(sbyte damage, ulong clientWhoAttacked, GameObject clientObjectWhoAttacked)
    {
        // 피격 처리 총괄.
        TakingDamage(damage, clientWhoAttacked);

        // 공격자가 Player라면 카메라 쉐이크 
        if (clientObjectWhoAttacked.TryGetComponent<PlayerClient>(out PlayerClient playerClient))
        {
            playerClient.ActivateHitCameraShakeClientRPC();
        }
    }

    // 파이어볼 도트 대미지를 받는 Coroutine
    public IEnumerator TakeDamageOverTime(sbyte damagePerSecond, float duration, ulong clientWhoAttacked)
    {
        float elapsed = 0;
        while (elapsed < duration)
        {
            // 1초 대기
            yield return new WaitForSeconds(1);

            TakingDamage(damagePerSecond, clientWhoAttacked);

            elapsed += 1;
        }
    }

    public sbyte GetHP()
    {
        return playerData.hp;
    }
}