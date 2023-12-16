using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
/// <summary>
/// GameRoom의 캐릭터 상태 관리
/// </summary>
public class GameRoomCharacterManager : NetworkBehaviour
{
    public static GameRoomCharacterManager instance {  get; private set; }
    public static event EventHandler OnInstanceCreated; // GameMultiplayer에서 GameRoom시작을 인식하고 GameRoomCharacterManager의 작업 시작을 명령함.

    public event EventHandler OnPlayerCharacterUpdated;

    // Start is called before the first frame update
    void Start()
    {
        OnInstanceCreated?.Invoke(this, EventArgs.Empty);
    }

    // OninstanceCreated이벤트핸들러로 GameMultiplayer로부터 이 메서드 호출 받는것까지 했음. 아래 ServerRpc랑 ClientRpc 구현해주면 됨. 
    // 서버에서 현재 캐릭터 비주얼 정보를 각 클라이언트들에게 전달, 각 클라이언트들에서는 전달받은 정보대로 각각CharacterPos 밑에다가 업데이트 해주면 끝.
    public void SetPlayerCharacter()
    {
        SetPlayerCharacterServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerCharacterServerRpc(ServerRpcParams serverRpcParams = default)
    {
        // Client들에게 브로드캐스트. 

    }

    [ClientRpc]
    private void SetPlayerCharacterClientRpc(ulong clientId)
    {
        OnPlayerCharacterUpdated?.Invoke(this, EventArgs.Empty);
    }
    




}
