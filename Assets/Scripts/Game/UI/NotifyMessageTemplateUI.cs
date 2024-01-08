using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using WebSocketSharp;
/// <summary>
/// 게임오버 알림창(킬로그) 아이템용 템플릿
/// 1. 플레이어 캐릭터명 업데이트
/// 2. 셀프 Destroy
/// </summary>
public class NotifyMessageTemplateUI : MonoBehaviour
{
    public TextMeshProUGUI txtPlayerName;
    // Start is called before the first frame update
    void Start()
    {
        //Debug.Log($"NotifyMessageTemplateUI");
        Destroy(gameObject, 5f);
    }

    public void UpdatePlayerName(string playerName)
    {
        if(playerName.IsNullOrEmpty())
        {
            playerName = "---";
            Debug.LogError($"NotifyMessageTemplateUI. playerName is null or Empty. Please check.");
        }
        Debug.Log($"NotifyMessageTemplateUI UpdatePlayerName {playerName}");
        txtPlayerName.text = $"{playerName} is game over";
    }
}
