using System.Collections.Generic;
using UnityEngine;


public class PlayerSpellScrollQueueControllerClient : MonoBehaviour
{
    public Queue<byte> playerScrollSpellSlotQueueOnClient = new Queue<byte>();

    // On Client
    public void UpdatePlayerScrollSpellSlotQueueOnClient(Queue<byte> scrollQueue)
    {
        // Update Queue
        playerScrollSpellSlotQueueOnClient = new Queue<byte>(scrollQueue);

        Debug.Log($"UpdatePlayerScrollSpellSlotQueueOnClient. count:{playerScrollSpellSlotQueueOnClient.Count}");

        // Update UI
        if (playerScrollSpellSlotQueueOnClient.Count == 0)
            // Spell Scroll Count UI 비활성화
            GameSceneUIManager.Instance.buttonReadSpellScrollUIController.DeactivateUI();
        else
            // Spell Scroll Count UI 활성화 
            GameSceneUIManager.Instance.buttonReadSpellScrollUIController.ActivateAndUpdateUI();
    }

    public byte PeekPlayerScrollSpellSlotQueueOnClient()
    {
        return playerScrollSpellSlotQueueOnClient.Peek();
    }

    public int GetPlayerScrollSpellSlotCount()
    {
        return playerScrollSpellSlotQueueOnClient.Count;
    }

    public void DequeuePlayerScrollSpellSlotQueueOnClient()
    {
        // Dequeue
        playerScrollSpellSlotQueueOnClient.Dequeue();

        Debug.Log($"DequeuePlayerScrollSpellSlotQueueOnClient. count:{playerScrollSpellSlotQueueOnClient.Count}");

        // Update UI
        if (playerScrollSpellSlotQueueOnClient.Count == 0)
            // Spell Scroll Count UI 비활성화
            GameSceneUIManager.Instance.buttonReadSpellScrollUIController.DeactivateUI();
        else
            // Spell Scroll Count UI 활성화 
            GameSceneUIManager.Instance.buttonReadSpellScrollUIController.ActivateAndUpdateUI();
    }
}