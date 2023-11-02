using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
/// <summary>
/// 1. 혹시 이미 게임을 진행한 후에 타이틀씬으로 돌아왔을 경우. NetworkManager & GameMultiplayer가 살아있게된다.
///    그럴 경우 LobbyScene으로 갔을 때 충돌이 일어나지 않게 하기 위해 NetworkManager & GameMultiplayer를 없애준다.
/// </summary>
public class TitleSceneCleanUp : MonoBehaviour
{
    private void Awake()
    {
        if(NetworkManager.Singleton != null)
        {
            Destroy(NetworkManager.Singleton.gameObject);
        }
        if (GameMultiplayer.Instance != null)
        {
            Destroy(GameMultiplayer.Instance.gameObject);
        }
    }
}
