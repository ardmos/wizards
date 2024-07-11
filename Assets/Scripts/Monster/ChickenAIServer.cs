using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ChickenAIServer : MonoBehaviour
{
    public ChickenAIMovementServer chickenAIMovementServer;

    public void GameOver()
    {
        // ������ ���
        DropScrollItem();
        // ���� ���߱�
        chickenAIMovementServer.StopMove();
        // ������Ʈ �ı�
        Destroy(gameObject);
    }

    private void DropScrollItem()
    {
        // ���ʷ���Ʈ ������
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
