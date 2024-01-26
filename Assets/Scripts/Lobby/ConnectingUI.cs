using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 게임룸 대신 UI를 사용하기로 하게되면서
/// 이제 안쓰게된 스크립트입니다. 
/// </summary>
public class ConnectingUI : MonoBehaviour
{
/*    private void Start()
    {
        GameMultiplayer.Instance.OnTryingToJoinGame += GameMultiplayer_OnTryingToJoinGame;
        GameMultiplayer.Instance.OnFailedToJoinGame += GameMultiplayer_OnFailedToJoinGame;
        Hide();
    }

    private void GameMultiplayer_OnFailedToJoinGame(object sender, System.EventArgs e)
    {
        Hide();
    }

    private void GameMultiplayer_OnTryingToJoinGame(object sender, System.EventArgs e)
    {
        Show();
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        // GameMultiplayer와 현 스크립트의 오브젝트는 라이프사이클이 다르기 때문에 손수 이벤트 구독을 해제해준다
        GameMultiplayer.Instance.OnTryingToJoinGame -= GameMultiplayer_OnTryingToJoinGame;
        GameMultiplayer.Instance.OnFailedToJoinGame -= GameMultiplayer_OnFailedToJoinGame;
    }*/
}
