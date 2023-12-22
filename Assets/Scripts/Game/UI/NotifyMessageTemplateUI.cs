using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
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
        Destroy(gameObject, 3f);
    }

    public void UpdatePlayerName(string playerName)
    {
        //Debug.Log($"NotifyMessageTemplateUI UpdatePlayerName {playerName}");
        txtPlayerName.text = $"Player{playerName} is game over";
    }
}
