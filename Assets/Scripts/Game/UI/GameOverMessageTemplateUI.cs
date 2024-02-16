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
