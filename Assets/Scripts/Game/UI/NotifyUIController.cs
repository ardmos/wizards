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
        GameManager.Instance.OnGameOverListChanged += OnPlayerGameOverListChanged;
    }

    // ���ӿ����� �߻����� ��, ���� �ֱٿ� ���ӿ����� �÷��̾ ����ݴϴ�. ������ ClientID�� ����ݴϴ�. playerName����� �����Ǹ� playerName���� �����Ұ��Դϴ�.
    private void OnPlayerGameOverListChanged(object sender, System.EventArgs e)
    {       
        GameObject messageObject = Instantiate(notifyMessageTemplatePrefab);
        messageObject.GetComponent<NotifyMessageTemplateUI>().UpdatePlayerName(GameManager.Instance.GetLastGameOverPlayer().ToString());
        messageObject.transform.SetParent(notifyMessageGroup.transform, false);        
    }
}
