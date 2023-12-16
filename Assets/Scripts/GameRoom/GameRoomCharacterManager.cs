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
    public static event EventHandler OnInstanceCreated; // GameMultiplayer���� GameRoom������ �ν��ϰ� GameRoomCharacterManager�� �۾� ������ �����.

    public event EventHandler OnPlayerCharacterUpdated;

    // Start is called before the first frame update
    void Start()
    {
        OnInstanceCreated?.Invoke(this, EventArgs.Empty);
    }

    // OninstanceCreated�̺�Ʈ�ڵ鷯�� GameMultiplayer�κ��� �� �޼��� ȣ�� �޴°ͱ��� ����. �Ʒ� ServerRpc�� ClientRpc �������ָ� ��. 
    // �������� ���� ĳ���� ���־� ������ �� Ŭ���̾�Ʈ�鿡�� ����, �� Ŭ���̾�Ʈ�鿡���� ���޹��� ������� ����CharacterPos �ؿ��ٰ� ������Ʈ ���ָ� ��.
    public void SetPlayerCharacter()
    {
        SetPlayerCharacterServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerCharacterServerRpc(ServerRpcParams serverRpcParams = default)
    {
        // Client�鿡�� ��ε�ĳ��Ʈ. 

    }

    [ClientRpc]
    private void SetPlayerCharacterClientRpc(ulong clientId)
    {
        OnPlayerCharacterUpdated?.Invoke(this, EventArgs.Empty);
    }
    




}
