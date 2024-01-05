using Unity.Netcode;
/// <summary>
/// 마법을 Server Auth 방식으로 시전할 수 있도록 도와주는 스크립트 입니다.
/// </summary>
public class PlayerHPManager : NetworkBehaviour
{
    public static PlayerHPManager Instance {  get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void SetPlayerHPOnServer(sbyte playerHP, ulong clientId)
    {    
        if (NetworkManager.ConnectedClients.ContainsKey(clientId))
        {
            UpdatePlayerHP(clientId, playerHP);
        }
    }

    /// <summary>
    /// 서버에서 호출해야하는 메서드. 서버에서 동작합니다.
    /// </summary>
    /// <param name="clientId"></param>
    /// <param name="playerTotalHP"></param>
    public void UpdatePlayerHP(ulong clientId, sbyte playerTotalHP)
    {
        // 변경된 HP값 서버에 저장
        PlayerData playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(clientId);
        playerData.playerHP = playerTotalHP;
        GameMultiplayer.Instance.SetPlayerDataFromClientId(clientId, playerData);

        // 각 Client 플레이어의 HP바 UI 업데이트
        NetworkClient networkClient = NetworkManager.ConnectedClients[clientId];
        networkClient.PlayerObject.GetComponent<Player>().SetHPClientRPC(clientId, playerTotalHP);
        

        // 게임오버 처리. 서버권한 방식.
        if (playerTotalHP <= 0)
        {
            // 게임오버 플레이어 게임오버자 명단 서버에 등록
            GameManager.Instance.UpdatePlayerGameOverListOnServer(clientId);

            // GameOver 플레이어 AnimState 서버에 등록. 
            GameMultiplayer.Instance.UpdatePlayerAnimStateOnServer(clientId, PlayerMoveAnimState.GameOver);

            // 해당 플레이어 조작 불가 처리
            networkClient.PlayerObject.GetComponent<Player>().SetPlayerGameOverClientRPC();
        }

    }
}