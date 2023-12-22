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
        GameObject messageObject = Instantiate(notifyMessageTemplatePrefab);
        messageObject.GetComponent<NotifyMessageTemplateUI>().UpdatePlayerName(changeEvent.Value.ToString());
        messageObject.transform.SetParent(notifyMessageGroup.transform, false);
    }
}
