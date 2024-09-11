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
            // ScrollCountDisplayUI ��Ȱ��ȭ
            GameSceneUIManager.Instance.buttonReadSpellScrollUIController.DeactivateUI();
        else
        {
            // ScrollCountDisplayUI Ȱ��ȭ 
            GameSceneUIManager.Instance.buttonReadSpellScrollUIController.ActivateUI();
            // ScrollCountDisplayUI ������Ʈ
            GameSceneUIManager.Instance.buttonReadSpellScrollUIController.UpdateUI(scrollCount);
        }       
    }
}