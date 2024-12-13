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
        if (IsHost)
        {
            SingleplayerGameManager.Instance.OnPlayerGameOver += GameManager_OnPlayerGameOver;
        }
        else
        {
            MultiplayerGameManager.Instance.OnPlayerGameOver += GameManager_OnPlayerGameOver;
        }
    }

    private void GameManager_OnPlayerGameOver(object sender, PlayerGameOverEventArgs e)
    {
        if (IsHost)
        {
            string playerWhoAttacked = "";
            if (e.clientIDWhoAttacked == 100) playerWhoAttacked = "\'Disconnect\'";
            else playerWhoAttacked = GameSingleplayer.Instance.GetPlayerDataFromClientId(e.clientIDWhoAttacked).playerName.ToString();
            ShowGameOverPlayerClientRPC(GameSingleplayer.Instance.GetPlayerDataFromClientId(e.clientIDWhoGameOver).playerName.ToString(), playerWhoAttacked);
        }
        else
        {
            string playerWhoAttacked = "";
            if (e.clientIDWhoAttacked == 100) playerWhoAttacked = "\'Disconnect\'";
            else playerWhoAttacked = CurrentPlayerDataManager.Instance.GetPlayerDataByClientId(e.clientIDWhoAttacked).playerName.ToString();
            ShowGameOverPlayerClientRPC(CurrentPlayerDataManager.Instance.GetPlayerDataByClientId(e.clientIDWhoGameOver).playerName.ToString(), playerWhoAttacked);
        }


    }

    public override void OnNetworkDespawn()
    {
        if (IsHost)
        {
            SingleplayerGameManager.Instance.OnPlayerGameOver -= GameManager_OnPlayerGameOver;
        }
        else
        {
            MultiplayerGameManager.Instance.OnPlayerGameOver -= GameManager_OnPlayerGameOver;
        }        
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
