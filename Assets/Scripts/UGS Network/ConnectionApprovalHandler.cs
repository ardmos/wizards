using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
/// <summary>
/// Approval progress for new connectiong clients.
/// Currently we are just making sure the amount of players does not exceed the MaxPlayers count.
/// Here you can do all sorts of pre-processing such as different prefabs for the player to spawn with depending on some condition.
/// https://docs-multiplayer.unity3d.com/netcode/current/basics/connection-approval/
/// </summary>
public class ConnectionApprovalHandler : MonoBehaviour
{
    public static int MaxPlayers = 2;

    private void Start()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;    
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton == null) return;
        NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {

        response.Approved = true;
        response.CreatePlayerObject = true;
        response.PlayerPrefabHash = null;
/*        if (SceneManager.GetActiveScene().name != LoadingSceneManager.Scene.GameRoomScene.ToString())
        {
            response.Approved = false;
            response.Reason = "Game has already started";
        }*/
        if (NetworkManager.Singleton.ConnectedClients.Count >= MaxPlayers ) 
        {
            response.Approved = false;
            response.Reason = "Server is Full";
        }

        response.Pending = false;

        if(!response.Approved)
            Debug.Log($"{nameof(ApprovalCheck)} Reason: {response.Reason}");
        else
            Debug.Log($"{nameof(ApprovalCheck)} Approved: true");
    }
}
