using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ChickenAIServer : NetworkBehaviour
{
    public ChickenAIMovementServer chickenAIMovementServer;

    public void GameOver()
    {
        // 아이템 드랍
        DropScrollItem();
        // 추적 멈추기
        chickenAIMovementServer.StopMove();

        Debug.Log($"{gameObject} 몬스터 파괴합니다!");

        // 오브젝트 파괴
        NetworkObject.Despawn(gameObject);
    }

    private void DropScrollItem()
    {
        // 제너레이트 아이템
        GameObject scrollObject = Instantiate(GameAssetsManager.Instance.GetItemScrollObject(), transform.position, transform.rotation);

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
