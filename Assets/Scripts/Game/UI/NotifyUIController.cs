using UnityEngine;
/// <summary>
/// 게임오버 메세지(킬로그) UI 컨트롤러 입니다.
/// 1. PlayerGameOverList가 업데이트 될 때마다 메세지로 출력
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

    // 게임오버가 발생했을 때, 가장 최근에 게임오버된 플레이어를 띄워줍니다. 지금은 ClientID를 띄워줍니다. playerName기능이 구현되면 playerName으로 변경할것입니다.
    private void OnPlayerGameOverListChanged(object sender, System.EventArgs e)
    {       
        GameObject messageObject = Instantiate(notifyMessageTemplatePrefab);
        messageObject.GetComponent<NotifyMessageTemplateUI>().UpdatePlayerName(GameManager.Instance.GetLastGameOverPlayer().ToString());
        messageObject.transform.SetParent(notifyMessageGroup.transform, false);        
    }
}
