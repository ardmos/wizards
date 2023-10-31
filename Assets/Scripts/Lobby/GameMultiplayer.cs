using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Spawn network objects
/// </summary>

public class GameMultiplayer : NetworkBehaviour
{
    public static GameMultiplayer Instance { get; private set; }

    private void Awake()
    {
        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    public void StartHost()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManager_ConnectionApprovalCallback;
        NetworkManager.Singleton.StartHost();
    }

    private void NetworkManager_ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        response.Approved = true;
    }

    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
    }


}
