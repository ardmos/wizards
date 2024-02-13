using Unity.Netcode;
using UnityEngine;
/// <summary>
/// ���ӿ��� �޼���(ų�α�) UI ��Ʈ�ѷ� �Դϴ�.
/// 1. PlayerGameOverList�� ������Ʈ �� ������ �޼����� ���
/// </summary>
public class NotifyGameOverUIController : NetworkBehaviour
{
    public GameObject notifyMessageGroup;
    public GameObject notifyMessageTemplatePrefab;

    // ���ӿ����� �߻����� ��, ���� �ֱٿ� ���ӿ����� �÷��̾ ����ݴϴ�. ������ ClientID�� ����ݴϴ�. playerName����� �����Ǹ� playerName���� �����Ұ��Դϴ�.
    [ClientRpc]
    public void NotifyGameOverPlayerClientRPC(string gameOverPlayerName)
    {
        Debug.Log($"Player {gameOverPlayerName} �� ���ӿ����ƽ��ϴ�! �˸� �ö󰩴ϴ�!");
        GameObject messageObject = Instantiate(notifyMessageTemplatePrefab);
        messageObject.GetComponent<NotifyMessageTemplateUI>().UpdatePlayerName(gameOverPlayerName);
        messageObject.transform.SetParent(notifyMessageGroup.transform, false);        
    }
}
