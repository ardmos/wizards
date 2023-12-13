using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
/// <summary>
/// 1. 혹시 이미 게임을 진행한 후에 타이틀씬으로 돌아왔을 경우. NetworkManager & GameMultiplayer가 살아있게된다.
///    그럴 경우 LobbyScene으로 갔을 때 충돌이 일어나지 않게 하기 위해 NetworkManager & GameMultiplayer를 없애준다.
///    <<< - 였지만, 최초 타이틀씬 진입시 네트워크 매니저가 없어져서 문제가 발생중... 잠시 사용하지 않도록 한다.
/// </summary>
public class TitleSceneCleanUp : MonoBehaviour
{
    private void Awake()
    {
/*        if(NetworkManager.Singleton != null)
        {
            Destroy(NetworkManager.Singleton.gameObject);
        }
        if (GameMultiplayer.Instance != null)
        {
            Destroy(GameMultiplayer.Instance.gameObject);
        }*/
    }
}
