using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using WebSocketSharp;
/// <summary>
/// 게임오버 알림창(킬로그) 아이템용 템플릿
/// 1. 플레이어 캐릭터명 업데이트
/// 2. 셀프 Destroy
/// </summary>
public class GameOverMessageTemplateUI : MonoBehaviour
{
    public TextMeshProUGUI txtPlayerName;

    void Start()
    {
        //Debug.Log($"NotifyMessageTemplateUI");
        Destroy(gameObject, 5f);
    }

    public void SetPlayerName(string playerName)
    {
        if(playerName.IsNullOrEmpty())
        {
            playerName = "---";
            Debug.LogError($"{nameof(SetPlayerName)}. playerName is null or Empty. Please check.");
        }
        Debug.Log($"{nameof(SetPlayerName)}. SetPlayerName {playerName}");
        txtPlayerName.text = $"{playerName} has been defeated!";
    }
}
