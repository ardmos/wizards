using Unity.Netcode;
/// <summary>
/// Player HP를  Server Auth 방식으로 관리할수 있도록 도와주는 스크립트 입니다.
/// 서버에서 동작합니다.
/// 각 플레이어블 캐릭터 오브젝트의 컴포넌트로 붙여서 사용합니다
/// </summary>
public class PlayerHPManagerServer : NetworkBehaviour
{
    PlayerInGameData playerData;

    public void InitPlayerHP(ICharacter character)
    {
        playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(OwnerClientId);
        playerData.hp = character.hp; 
        playerData.maxHp = character.maxHp;
        GameMultiplayer.Instance.SetPlayerDataFromClientId(OwnerClientId, playerData);

        GetComponent<PlayerClient>().SetHPClientRPC(playerData.hp, playerData.maxHp);
    }

    public void ApplyHeal(sbyte healingValue)
    {
        /// 힐 적용하는중
        playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(OwnerClientId);
        sbyte newHP = (sbyte)(playerData.hp + healingValue);
        if (newHP > playerData.maxHp) newHP = playerData.maxHp;   
    }

    /// <summary>
    /// 서버에서 호출해야하는 메서드. 서버에서 동작합니다.
    /// </summary>
    public void TakingDamage(sbyte damage, ulong clientWhoAttacked)
    {
        // 요청한 플레이어 현재 HP값 가져오기 
        playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(OwnerClientId);
        sbyte newPlayerHP = playerData.hp;

        // HP보다 Damage가 클 경우(게임오버 처리는 Player에서 HP잔량 파악해서 알아서 한다.)
        if (newPlayerHP <= damage  && newPlayerHP != 0)
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

        // 각 Client 플레이어의 HP바 UI 업데이트
        /*NetworkClient networkClient = NetworkManager.ConnectedClients[OwnerClientId];
        networkClient.PlayerObject.GetComponent<PlayerClient>().SetHPClientRPC(playerData.hp, playerData.maxHp);*/
        GetComponent<PlayerClient>().SetHPClientRPC(playerData.hp, playerData.maxHp);
    }

    // 게임오버 처리. 서버권한 방식.
    private void GameOver(ulong clientWhoAttacked)
    {
        // 킬한 플레이어 스코어 업데이트
        GameMultiplayer.Instance.AddPlayerScore(clientWhoAttacked, 300);

        // 게임오버 플레이어 사실을 서버에 기록.
        GameManager.Instance.UpdatePlayerGameOverOnServer(OwnerClientId, clientWhoAttacked);

        // GameOver 플레이어 AnimState GameOver 상태로 서버에 등록. 게임오버 애니메이션 실행. 
        GameMultiplayer.Instance.UpdatePlayerMoveAnimStateOnServer(OwnerClientId, PlayerMoveAnimState.GameOver);

        // 해당 플레이어 조작 불가 처리 및 게임오버 팝업 띄우기.
        GetComponent<PlayerClient>().SetPlayerGameOverClientRPC();
    }
}