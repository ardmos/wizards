using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
/// <summary>
/// GameRoom狼 某腐磐 惑怕 包府
/// </summary>
public class GameRoomCharacterManager : NetworkBehaviour
{
    public static GameRoomCharacterManager instance {  get; private set; }
    //public static event EventHandler OnInstanceCreated; // GameMultiplayer俊辑 GameRoom矫累阑 牢侥窍绊 GameRoomCharacterManager狼 累诀 矫累阑 疙飞窃.

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

    // OninstanceCreated捞亥飘勤甸矾肺 GameMultiplayer肺何磐 捞 皋辑靛 龋免 罐绰巴鳖瘤 沁澜. 酒贰 ServerRpc尔 ClientRpc 备泅秦林搁 凳. 
    // 辑滚俊辑 泅犁 某腐磐 厚林倔 沥焊甫 阿 努扼捞攫飘甸俊霸 傈崔, 阿 努扼捞攫飘甸俊辑绰 傈崔罐篮 沥焊措肺 阿阿CharacterPos 关俊促啊 诀单捞飘 秦林搁 场.
    // GameMultiplayer에서 ClientConnected, disconnected일 경우 호출됨. 
    public void UpdatePlayerCharacter()
    {
        UpdatePlayerCharacterServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdatePlayerCharacterServerRpc(ServerRpcParams serverRpcParams = default)
    {
        // Client甸俊霸 宏肺靛某胶飘.
        // SetPlayerCharacterServerRpc를 호출한 ClientID를 담아 브로드캐스팅 해줌. 각Client함수에서는 자신의 ClientID와 같은 경우만 캐릭터 생성 로직 진행.  
        UpdatePlayerCharacterClientRpc(serverRpcParams.Receive.SenderClientId);    
    }

    [ClientRpc]
    private void UpdatePlayerCharacterClientRpc(ulong clientId)
    {
        OnPlayerCharacterUpdated?.Invoke(this, EventArgs.Empty);
    }
    




}
