using Unity.Netcode;
using UnityEngine;
/// <summary>
/// Player HP를  Server Auth 방식으로 관리할수 있도록 도와주는 스크립트 입니다.
/// 서버에서 동작합니다.
/// </summary>
public class PlayerHPManager : NetworkBehaviour
{
    public static PlayerHPManager Instance {  get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// 서버에서 호출해야하는 메서드. 서버에서 동작합니다.
    /// </summary>
    public void UpdatePlayerHP(ulong clientId, sbyte currentHP, sbyte maxHP)
    {
        if (!NetworkManager.ConnectedClients.ContainsKey(clientId))
        {
            Debug.Log($"{nameof(UpdatePlayerHP)} 존재하지 않는 ClientId값입니다.");
            return;
        }


        // 변경된 HP값 서버에 저장
        PlayerInGameData playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(clientId);
        playerData.hp = currentHP;
        playerData.maxHp = maxHP;
        GameMultiplayer.Instance.SetPlayerDataFromClientId(clientId, playerData);

        // 각 Client 플레이어의 HP바 UI 업데이트
        NetworkClient networkClient = NetworkManager.ConnectedClients[clientId];
        networkClient.PlayerObject.GetComponent<PlayerClient>().SetHPClientRPC(playerData.hp, playerData.maxHp);
        

        // 게임오버 처리. 서버권한 방식.
        if (currentHP <= 0)
        {
            // 게임오버 플레이어 사실을 서버에 기록.
            GameManager.Instance.UpdatePlayerGameOverOnServer(clientId);

            // GameOver 플레이어 AnimState GameOver 상태로 서버에 등록. 게임오버 애니메이션 실행. 
            GameMultiplayer.Instance.UpdatePlayerMoveAnimStateOnServer(clientId, PlayerMoveAnimState.GameOver);

            // 해당 플레이어 조작 불가 처리 및 게임오버 팝업 띄우기.
            networkClient.PlayerObject.GetComponent<PlayerClient>().SetPlayerGameOverClientRPC();
        }

    }
}