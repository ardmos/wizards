using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
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
        //Debug.Log($"NotifyMessageTemplateUI UpdatePlayerName {playerName}");
        txtPlayerName.text = $"Player{playerName} is game over";
    }
}
