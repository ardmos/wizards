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
        // 제너레이트 아이템 (몬스터는 랜덤으로 드랍)
        GameObject randomPrefab = null;

        if(Random.Range(0, 2) == 0)
        {
            randomPrefab = GameAssetsManager.Instance.GetItemHPPotionObject();
        }
        else
        {
            randomPrefab = GameAssetsManager.Instance.GetItemScrollObject();
        }

        GameObject dropItemObject = Instantiate(randomPrefab, transform.position, transform.rotation);

        if (!dropItemObject) return;

        if (dropItemObject.TryGetComponent<NetworkObject>(out NetworkObject networkObject))
        {
            networkObject.Spawn();
            if (MultiplayerGameManager.Instance)
            {
                dropItemObject.transform.parent = MultiplayerGameManager.Instance.transform;
                dropItemObject.transform.position = transform.position;
            }
        }
    }
}
