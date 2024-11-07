using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


public class PlayerScrollCounter : NetworkBehaviour
{
    // On Client
    public void UpdateScrollCount(int scrollCount)
    {
        Debug.Log($"UpdateScrollCount. count:{scrollCount}");

        // Update UI
        if (scrollCount == 0)
            // ScrollCountDisplayUI 비활성화
            GameSceneUIManager.Instance.buttonReadSpellScrollUIController.DeactivateUI();
        else
        {
            // ScrollCountDisplayUI 활성화 
            GameSceneUIManager.Instance.buttonReadSpellScrollUIController.ActivateUI();
            // ScrollCountDisplayUI 업데이트
            GameSceneUIManager.Instance.buttonReadSpellScrollUIController.UpdateUI(scrollCount);
        }       
    }
}