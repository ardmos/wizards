using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ChickenAIServer : NetworkBehaviour
{
    public ChickenAIMovementServer chickenAIMovementServer;

    public void GameOver()
    {
        // ������ ���
        DropScrollItem();
        // ���� ���߱�
        chickenAIMovementServer.StopMove();

        Debug.Log($"{gameObject} ���� �ı��մϴ�!");

        // ������Ʈ �ı�
        NetworkObject.Despawn(gameObject);
    }

    private void DropScrollItem()
    {
        // ���ʷ���Ʈ ������ (���ʹ� �������� ���)
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
