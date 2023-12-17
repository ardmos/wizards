using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
/// <summary>
/// GameRoom�� ĳ���� ���� ����
/// </summary>
public class GameRoomCharacterManager : NetworkBehaviour
{
    public static GameRoomCharacterManager instance {  get; private set; }
    //public static event EventHandler OnInstanceCreated; // GameMultiplayer���� GameRoom������ �ν��ϰ� GameRoomCharacterManager�� �۾� ������ �����.

    public event EventHandler OnPlayerCharacterUpdated;

    // Start is called before the first frame update
    void Awake()
    {
        if(instance == null)
        {
            instance = this;
            Debug.Log($"GameRoomCharacterManager instance is null? : {instance}");
        }
        //OnInstanceCreated?.Invoke(this, EventArgs.Empty);
    }

    // OninstanceCreated�̺�Ʈ�ڵ鷯�� GameMultiplayer�κ��� �� �޼��� ȣ�� �޴°ͱ��� ����. �Ʒ� ServerRpc�� ClientRpc �������ָ� ��. 
    // �������� ���� ĳ���� ���־� ������ �� Ŭ���̾�Ʈ�鿡�� ����, �� Ŭ���̾�Ʈ�鿡���� ���޹��� ������� ����CharacterPos �ؿ��ٰ� ������Ʈ ���ָ� ��.
    // GameMultiplayer�2�3�1�9 ClientConnected, disconnected�3�1 �7�4�3�7 �6�3�4�9�9�3. 
    public void UpdatePlayerCharacter()
    {
        UpdatePlayerCharacterServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdatePlayerCharacterServerRpc(ServerRpcParams serverRpcParams = default)
    {
        // Client�鿡�� ��ε�ĳ��Ʈ.
        // SetPlayerCharacterServerRpc�0�7 �6�3�4�9�6�3 ClientID�0�7 �9�5�2�3 �1�5�0�9�9�3�4�3�2�1�5�4 �6�7�3�1. �7�6Client�6�5�2�1�2�3�1�9�8�9 �3�1�2�1�3�5 ClientID�2�5 �7�0�3�1 �7�4�3�7�0�7 �4�3�0�6�5�5 �1�6�1�0 �0�9�3�2 �3�5�6�8.  
        UpdatePlayerCharacterClientRpc(serverRpcParams.Receive.SenderClientId);    
    }

    [ClientRpc]
    private void UpdatePlayerCharacterClientRpc(ulong clientId)
    {
        OnPlayerCharacterUpdated?.Invoke(this, EventArgs.Empty);
    }
    




}
