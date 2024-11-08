using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class WizardRukeAIItemDropManagerServer : MonoBehaviour
{
    public void DropItem(Vector3 dropPosition)
    {
        // ���ʷ���Ʈ ������ ����
        Vector3 newItemHPPotionPos = new Vector3(dropPosition.x + 0.5f, dropPosition.y, dropPosition.z);
        GenerateItem(newItemHPPotionPos, GameAssetsManager.Instance.GetItemHPPotionObject());

        // ���ʷ���Ʈ ������ ��ũ��
        Vector3 newItemScrollPos = new Vector3(dropPosition.x - 0.5f, dropPosition.y, dropPosition.z);
        GenerateItem(newItemScrollPos, GameAssetsManager.Instance.GetItemScrollObject());
    }

    private void GenerateItem(Vector3 position, GameObject prefabObject)
    {     
        GameObject itemObject = Instantiate(prefabObject, position, transform.rotation, MultiplayerGameManager.Instance.transform);
       if (!itemObject) return;

        if (itemObject.TryGetComponent<NetworkObject>(out NetworkObject networkObject))
        {
            networkObject.Spawn();
            if (MultiplayerGameManager.Instance)
            {
                itemObject.transform.parent = MultiplayerGameManager.Instance.transform;
                itemObject.transform.position = position;
            }
        }
    }
}
