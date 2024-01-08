using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using WebSocketSharp;
/// <summary>
/// ���ӿ��� �˸�â(ų�α�) �����ۿ� ���ø�
/// 1. �÷��̾� ĳ���͸� ������Ʈ
/// 2. ���� Destroy
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
