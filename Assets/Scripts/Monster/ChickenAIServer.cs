using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ChickenAIServer : MonoBehaviour
{
    public ChickenAIMovementServer chickenAIMovementServer;

    public void GameOver()
    {
        // 아이템 드랍
        DropScrollItem();
        // 추적 멈추기
        chickenAIMovementServer.StopMove();
        // 오브젝트 파괴
        Destroy(gameObject);
    }

    private void DropScrollItem()
    {
        // 제너레이트 아이템
        GameObject scrollObject = Instantiate(GameAssetsManager.Instance.GetItemScrollObject());

        if (!scrollObject) return;

        if (scrollObject.TryGetComponent<NetworkObject>(out NetworkObject networkObject))
        {
            networkObject.Spawn();
            if (GameManager.Instance)
            {
                scrollObject.transform.parent = GameManager.Instance.transform;
                scrollObject.transform.position = transform.position;
            }
        }
    }
}
