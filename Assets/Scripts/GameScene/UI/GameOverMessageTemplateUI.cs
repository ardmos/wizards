using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using WebSocketSharp;
/// <summary>
/// ���ӿ��� �˸�â(ų�α�) �����ۿ� ���ø�
/// 1. �÷��̾� ĳ���͸� ������Ʈ
/// 2. ���� Destroy
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
