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

    public void SetMessage(string gameOverPlayerName, string attackedPlayerName)
    {
        if(gameOverPlayerName.IsNullOrEmpty())
        {
            gameOverPlayerName = "---";
            Debug.LogError($"{nameof(SetMessage)}. playerName is null or Empty. Please check.");
        }
        //Debug.Log($"{nameof(SetMessage)}. SetMessage {gameOverPlayerName}");
        txtPlayerName.text = $"'{gameOverPlayerName}' has been taken down by '{attackedPlayerName}'!";
    }
}
