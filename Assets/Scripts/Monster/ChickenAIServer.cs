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
        // ���ʷ���Ʈ ������
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
