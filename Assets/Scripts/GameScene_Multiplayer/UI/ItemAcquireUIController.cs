using UnityEngine;

public class ItemAcquireUIController : MonoBehaviour
{
    public GameObject ItemAcquiredMessageContainer;
    public GameObject ItemScrollAcquiredMessagePrefab;

    public void ShowScrollAcquiredUI()
    {
        GameObject messageObject = Instantiate(ItemScrollAcquiredMessagePrefab);
        messageObject.transform.SetParent(ItemAcquiredMessageContainer.transform, false);
    }
}
