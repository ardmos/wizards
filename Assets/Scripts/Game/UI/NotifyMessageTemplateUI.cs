using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
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
        Destroy(gameObject, 3f);
    }

    public void UpdatePlayerName(string playerName)
    {
        txtPlayerName.text = playerName;
    }
}
