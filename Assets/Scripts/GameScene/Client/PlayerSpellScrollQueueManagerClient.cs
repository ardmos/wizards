using System.Collections.Generic;
using UnityEngine;


public class PlayerSpellScrollQueueManagerClient : MonoBehaviour
{
    //public Queue<byte> playerScrollSpellSlotQueueOnClient = new Queue<byte>();

    // On Client
    public void UpdatePlayerScrollSpellSlotUI(int scrollCount)
    {
        Debug.Log($"UpdatePlayerScrollSpellSlotUI. count:{scrollCount}");

        // Update UI
        if (scrollCount == 0)
            // Spell Scroll Count UI 비활성화
            GameSceneUIManager.Instance.buttonReadSpellScrollUIController.DeactivateUI();
        else
        {
            // Spell Scroll Count UI 활성화 
            GameSceneUIManager.Instance.buttonReadSpellScrollUIController.ActivateUI();
            // Spell Scroll Count UI 업데이트
            GameSceneUIManager.Instance.buttonReadSpellScrollUIController.UpdateUI(scrollCount);
        }       
    }

/*    public byte PeekPlayerScrollSpellSlotQueueOnClient()
    {
        return playerScrollSpellSlotQueueOnClient.Peek();
    }

    public int GetPlayerScrollSpellSlotCount()
    {
        return playerScrollSpellSlotQueueOnClient.Count;
    }*/

/*    public void DequeuePlayerScrollSpellSlotQueueOnClient()
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
    }*/
}