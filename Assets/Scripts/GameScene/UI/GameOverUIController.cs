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
