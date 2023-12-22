using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// ���ӿ��� �޼���(ų�α�) UI ��Ʈ�ѷ� �Դϴ�.
/// 1. PlayerGameOverList�� ������Ʈ �� ������ �޼����� ���
/// </summary>
public class NotifyUIController : MonoBehaviour
{
    public GameObject notifyMessageGroup;
    public GameObject notifyMessageTemplatePrefab;
    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.playerGameOverListServer.OnListChanged += OnPlayerGameOverListChanged;
    }

    private void OnPlayerGameOverListChanged(Unity.Netcode.NetworkListEvent<ulong> changeEvent)
    {
        Debug.Log($"NotifyUIController OnPlayerGameOverListChanged"); //<<<<<< OnListChaged�� ȣ���� �ȵǴ°��� Ȯ���Ϸ���. 
        GameObject messageObject = Instantiate(notifyMessageTemplatePrefab);
        messageObject.GetComponent<NotifyMessageTemplateUI>().UpdatePlayerName($"Player{changeEvent.Value} is game over");
        messageObject.transform.SetParent(notifyMessageGroup.transform, false);
    }
}
