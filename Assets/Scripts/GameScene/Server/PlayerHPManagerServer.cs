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
    public PlayerServer playerServer;
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
        //playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(OwnerClientId);
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
        //playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(OwnerClientId);
        if (playerData.playerGameState != PlayerGameState.Playing) return;

        // 피격 카메라 효과 실행 ClientRPC
        playerClient.ActivateHitByAttackCameraEffectClientRPC();

        // 피격 카메라 쉐이크 효과 실행 ClientRPC
        playerClient.ActivateHitByAttackCameraShakeClientRPC();

        // 피격 사운드 효과 실행 ClientRPC

        // 피격 대미지 숫자 표시 실행. 각 Client Damage Text Popup UI 업데이트 지시 
        playerClient.ShowDamageTextPopupClientRPC(damage);

        // 요청한 플레이어 현재 HP값 가져오기 
        sbyte newPlayerHP = playerData.hp;

        // HP보다 Damage가 클 경우(게임오버 처리는 Player에서 HP잔량 파악해서 알아서 한다.)
        if (newPlayerHP <= damage)
        {
            // HP 0
            newPlayerHP = 0;
            playerData.playerGameState = PlayerGameState.GameOver; // 아래에서 HP값을 저장할 때 새로 SetPlayerData를 하기 때문에...! 일단 여기서 한 번더 게임오버 스테이트를 저장해주고 있는데, GameOver()에서 이미 게임오버처리를 해주고 있다. 일단은 동작하지만 수정 필요.

            // 게임오버 처리, GameOver 애니메이션 실행
            GameOver(clientWhoAttacked);
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
        GameMultiplayer.Instance.SetPlayerDataFromClientId(OwnerClientId, playerData);

        // 각 Client 플레이어의 HP바 UI 업데이트 ClientRPC       
        playerClient.SetHPClientRPC(playerData.hp, playerData.maxHp);

        // 각 Client의 쉐이더 피격 이펙트 실행 ClientRPC
        playerClient.ActivateHitByAttackEffectClientRPC();
    }

    public sbyte GetHP()
    {
        return playerData.hp;
    }

    // 게임오버 처리. 서버권한 방식.
    private void GameOver(ulong clientWhoAttacked)
    {
        // 스스로 게임오버 당한 경우, 게임 내 모든 플레이어들에게 점수를 줍니다. 
        if (clientWhoAttacked == OwnerClientId)
        {
            foreach(PlayerInGameData playerInGameData in GameMultiplayer.Instance.GetPlayerDataNetworkList()){
                GameMultiplayer.Instance.AddPlayerScore(playerInGameData.clientId, 300);
            }
        }
        // 일반적인 경우 상대 플레이어 300스코어 획득
        else
        {
            GameMultiplayer.Instance.AddPlayerScore(clientWhoAttacked, 300);
        }

        // 게임오버 플레이어 사실을 서버에 기록.
        GameManager.Instance.UpdatePlayerGameOverOnServer(OwnerClientId, clientWhoAttacked);

        // 플레이어 게임오버 애니메이션 실행
        playerAnimator.UpdatePlayerMoveAnimationOnServer(PlayerMoveAnimState.GameOver);

        // 플레이어 물리 충돌 해제
        playerServer.GameOver();
        // 해당 플레이어 조작 불가 처리 및 게임오버 팝업 띄우기.
        playerClient.SetPlayerGameOverClientRPC();
        // 플레이어 이름 & HP UI off
        playerClient.OffPlayerUIClientRPC();

        // 스크롤 아이템 드랍
        DropScrollItem();
    }

    private void DropScrollItem()
    {
        // 제너레이트 아이템
        GameObject scrollObject = Instantiate(GameAssetsManager.Instance.GetItemScrollObject());

        if (!scrollObject) return;

        if (scrollObject.TryGetComponent<NetworkObject>(out NetworkObject networkObject))
        {
            networkObject.Spawn();
            if (GameManager.Instance)
            {
                scrollObject.transform.parent = GameManager.Instance.transform;
                scrollObject.transform.position = transform.position;
            }
        }
    }
}