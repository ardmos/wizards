using System;
using Unity.Netcode;
using UnityEngine;
/// <summary>
/// ���ӿ��� �޼���(ų�α�) UI ��Ʈ�ѷ� �Դϴ�.
/// 1. PlayerGameOverList�� ������Ʈ �� ������ �޼����� ���
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

    // ���ӿ����� �߻����� ��, ���� �ֱٿ� ���ӿ����� �÷��̾ ����ݴϴ�. ������ ClientID�� ����ݴϴ�. playerName����� �����Ǹ� playerName���� �����Ұ��Դϴ�.
    [ClientRpc]
    public void ShowGameOverPlayerClientRPC(string gameOverPlayerName, string attackedPlayerName)
    {
        //Debug.Log($"Player {gameOverPlayerName} �� ���ӿ����ƽ��ϴ�! �˸� �ö󰩴ϴ�!");
        GameObject messageObject = Instantiate(gameOverMessagePrefab);
        messageObject.GetComponent<GameOverMessageTemplateUI>().SetMessage(gameOverPlayerName, attackedPlayerName);
        messageObject.transform.SetParent(gameOverMessageContainer.transform, false);        
    }
}
