using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 게임오버 메세지(킬로그) UI 컨트롤러 입니다.
/// 1. PlayerGameOverList가 업데이트 될 때마다 메세지로 출력
/// </summary>
public class NotifyUIController : MonoBehaviour
{
    public GameObject notifyMessageGroup;
    public GameObject notifyMessageTemplatePrefab;
    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.playerGameOverListServer.OnListChanged += OnPlayerGameOverListChanged;
    }

    private void OnPlayerGameOverListChanged(Unity.Netcode.NetworkListEvent<ulong> changeEvent)
    {
        Debug.Log($"NotifyUIController OnPlayerGameOverListChanged"); //<<<<<< OnListChaged가 호출이 안되는건지 확인하려함. 
        GameObject messageObject = Instantiate(notifyMessageTemplatePrefab);
        messageObject.GetComponent<NotifyMessageTemplateUI>().UpdatePlayerName($"Player{changeEvent.Value} is game over");
        messageObject.transform.SetParent(notifyMessageGroup.transform, false);
    }
}
