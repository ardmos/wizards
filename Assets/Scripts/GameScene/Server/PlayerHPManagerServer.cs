using System.Collections;
using Unity.Collections.LowLevel.Unsafe;
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
    public PlayerAnimator playerAnimator;

    public void InitPlayerHP(ICharacter character)
    {
        playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(OwnerClientId);
        playerData.hp = character.hp; 
        playerData.maxHp = character.maxHp;
        GameMultiplayer.Instance.SetPlayerDataFromClientId(OwnerClientId, playerData);

        playerClient.SetHPClientRPC(playerData.hp, playerData.maxHp);
    }

    /// <summary>
    /// 서버에서 동작합니다
    /// </summary>
    /// <param name="healingValue"></param>
    public void ApplyHeal(sbyte healingValue)
    {
        // 힐 적용하는중
        playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(OwnerClientId);
        sbyte newHP = (sbyte)(playerData.hp + healingValue);
        if (newHP > playerData.maxHp) newHP = playerData.maxHp;

        // 변경된 HP값 서버에 저장
        playerData.hp = newHP;
        GameMultiplayer.Instance.SetPlayerDataFromClientId(OwnerClientId, playerData);

        // 각 Client 플레이어의 HP바 UI 업데이트 ClientRPC       
        playerClient.SetHPClientRPC(playerData.hp, playerData.maxHp);
    }

    /// <summary>
    /// 서버에서 호출해야하는 메서드. 서버에서 동작합니다.
    /// </summary>
    public void TakingDamage(sbyte damage, ulong clientWhoAttacked)
    {
        // GamePlaying중이 아니면 전부 리턴. 게임이 끝나면 무적처리 되도록.
        if (!GameManager.Instance.IsGamePlaying()) return;

        // 요청한 플레이어 현재 HP값 가져오기 
        playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(OwnerClientId);
        sbyte newPlayerHP = playerData.hp;

        if (newPlayerHP == 0) return;

        // HP보다 Damage가 클 경우(게임오버 처리는 Player에서 HP잔량 파악해서 알아서 한다.)
        if (newPlayerHP <= damage)
        {
            // HP 0
            newPlayerHP = 0;
            playerData.playerGameState = PlayerGameState.GameOver; // 아래에서 HP값을 저장할 때 새로 SetPlayerData를 하기 때문에...! 일단 여기서 한 번더 게임오버 스테이트를 저장해주고 있는데, GameOver()에서 이미 게임오버처리를 해주고 있다. 일단은 동작하지만 수정 필요.

            // 게임오버 처리
            GameOver(clientWhoAttacked);
        }
        else
        {
            // HP 감소 계산
            newPlayerHP -= (sbyte)damage;
        }

        // 변경된 HP값 서버에 저장
        playerData.hp = newPlayerHP;
        GameMultiplayer.Instance.SetPlayerDataFromClientId(OwnerClientId, playerData);

        // 각 Client 플레이어의 HP바 UI 업데이트 ClientRPC       
        playerClient.SetHPClientRPC(playerData.hp, playerData.maxHp);

        // 각 Client의 화면에서 쉐이더 피격 이펙트 실행 ClientRPC
        playerClient.ActivateHitEffectClientRPC();

        // 피격 애니메이션 실행 Server
        playerAnimator.UpdatePlayerMoveAnimationOnServer(PlayerMoveAnimState.Hit);

        // 피격 카메라 효과 실행 ClientRPC
        playerClient.ActivateHitCameraEffectClientRPC();

        // 피격 카메라 쉐이크 효과 실행 ClientRPC
        playerClient.ActivateHitCameraShakeClientRPC();

        // 피격 사운드 효과 실행 ClientRPC

    }



    // 게임오버 처리. 서버권한 방식.
    private void GameOver(ulong clientWhoAttacked)
    {
        // 킬한 플레이어 스코어 업데이트. 킬한 플레이어가 본인일 경우, 게임 내 모든 플레이어들에게 점수를 줍니다. 
        // 킬한 플레이어가 본인일 경우(ex, Water로 인한 사망)
        if(clientWhoAttacked == OwnerClientId)
        {
            foreach(PlayerInGameData playerInGameData in GameMultiplayer.Instance.GetPlayerDataNetworkList()){
                GameMultiplayer.Instance.AddPlayerScore(playerInGameData.clientId, 300);
            }
        }
        else
        {
            GameMultiplayer.Instance.AddPlayerScore(clientWhoAttacked, 300);
        }
        

        // 게임오버 플레이어 사실을 서버에 기록.
        GameManager.Instance.UpdatePlayerGameOverOnServer(OwnerClientId, clientWhoAttacked);

        // GameOver 플레이어 AnimState GameOver 상태로 서버에 등록. 게임오버 애니메이션 실행.  애니메이션 실행은 PlayerMovementServer에서 해줍니다
        //GameMultiplayer.Instance.UpdatePlayerMoveAnimStateOnServer(OwnerClientId, PlayerMoveAnimState.GameOver);
        //playerAnimator.UpdatePlayerMoveAnimationOnServer(PlayerMoveAnimState.GameOver);

        // 해당 플레이어 조작 불가 처리 및 게임오버 팝업 띄우기.
        playerClient.SetPlayerGameOverClientRPC();
    }
}