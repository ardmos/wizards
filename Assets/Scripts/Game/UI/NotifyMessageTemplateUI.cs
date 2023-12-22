using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
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
        Destroy(gameObject, 3f);
    }

    public void UpdatePlayerName(string playerName)
    {
        txtPlayerName.text = playerName;
    }
}
