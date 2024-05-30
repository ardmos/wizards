using System;
using Unity.Netcode;
using UnityEngine;
/// <summary>
/// 게임오버 메세지(킬로그) UI 컨트롤러 입니다.
/// 1. PlayerGameOverList가 업데이트 될 때마다 메세지로 출력
/// </summary>
public class GameOverUIController : NetworkBehaviour
{
    public GameObject gameOverMessageContainer;
    public GameObject gameOverMessagePrefab;

    public override void OnNetworkSpawn()
    {
        GameManager.Instance.OnPlayerGameOver += GameManager_OnPlayerGameOver;
    }

    private void GameManager_OnPlayerGameOver(object sender, PlayerGameOverEventArgs e)
    {
        string playerWhoAttacked = "";
        if (e.clientIDWhoAttacked == 100) playerWhoAttacked = "\'Disconnect\'";
        else playerWhoAttacked = GameMultiplayer.Instance.GetPlayerDataFromClientId(e.clientIDWhoAttacked).playerName.ToString();
        ShowGameOverPlayerClientRPC(GameMultiplayer.Instance.GetPlayerDataFromClientId(e.clientIDWhoGameOver).playerName.ToString(), playerWhoAttacked);
    }

    public override void OnNetworkDespawn()
    {
        GameManager.Instance.OnPlayerGameOver -= GameManager_OnPlayerGameOver;
    }

    // 게임오버가 발생했을 때, 가장 최근에 게임오버된 플레이어를 띄워줍니다. 지금은 ClientID를 띄워줍니다. playerName기능이 구현되면 playerName으로 변경할것입니다.
    [ClientRpc]
    public void ShowGameOverPlayerClientRPC(string gameOverPlayerName, string attackedPlayerName)
    {
        //Debug.Log($"Player {gameOverPlayerName} 가 게임오버됐습니다! 알림 올라갑니다!");
        GameObject messageObject = Instantiate(gameOverMessagePrefab);
        messageObject.GetComponent<GameOverMessageTemplateUI>().SetMessage(gameOverPlayerName, attackedPlayerName);
        messageObject.transform.SetParent(gameOverMessageContainer.transform, false);        
    }
}
