using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
/// <summary>
/// 1. Ȥ�� �̹� ������ ������ �Ŀ� Ÿ��Ʋ������ ���ƿ��� ���. NetworkManager & GameMultiplayer�� ����ְԵȴ�.
///    �׷� ��� LobbyScene���� ���� �� �浹�� �Ͼ�� �ʰ� �ϱ� ���� NetworkManager & GameMultiplayer�� �����ش�.
///    <<< - ������, ���� Ÿ��Ʋ�� ���Խ� ��Ʈ��ũ �Ŵ����� �������� ������ �߻���... ��� ������� �ʵ��� �Ѵ�.
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
