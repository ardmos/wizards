using UnityEngine;

public class ItemAcquireUIController : MonoBehaviour
{
    public GameObject ItemAcquiredMessageContainer;
    public GameObject ItemAcquiredMessagePrefab;

    public void ShowItemAcquireUI()
    {
        GameObject messageObject = Instantiate(ItemAcquiredMessagePrefab);
        messageObject.transform.SetParent(ItemAcquiredMessageContainer.transform, false);
    }
}
