using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CurrentPlayerDataManager : NetworkBehaviour
{
    public static CurrentPlayerDataManager Instance { get; private set; }

    [Header("서버에 접속중인 플레이어들의 데이터가 담긴 네트워크 리스트")]
    private NetworkList<PlayerInGameData> currentPlayers;

    public event EventHandler OnCurrentPlayerListOnServerChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    public override void OnNetworkSpawn()
    {
        InitalizePlayerList();
    }

    public override void OnNetworkDespawn()
    {
        currentPlayers.OnListChanged -= OnCurrentPlayerListChanged;
    }

    private void InitalizePlayerList()
    {
        currentPlayers = new NetworkList<PlayerInGameData>();
        currentPlayers.OnListChanged += OnCurrentPlayerListChanged;
    }

    private void OnCurrentPlayerListChanged(NetworkListEvent<PlayerInGameData> changeEvent)
    {
        OnCurrentPlayerListOnServerChanged?.Invoke(this, EventArgs.Empty);
    }

    public void AddPlayer(PlayerInGameData playerData)
    {
        currentPlayers.Add(playerData);
    }

    public void RemovePlayer(ulong clientId)
    {
        int index = GetPlayerDataListIndexByClientId(clientId);
        if (index != -1)
            currentPlayers.RemoveAt(index);
    }

    public void RemoveAllPlayers()
    {
        currentPlayers?.Clear();
    }

    // 플레이어 client를 단서로 PlayerData(ClientId 포함 여러 플레이어 데이터)를 찾는 메소드
    public PlayerInGameData GetPlayerDataByClientId(ulong clientId)
    {
        foreach (PlayerInGameData playerData in currentPlayers)
        {
            if (playerData.clientId == clientId)
            {
                return playerData;
            }
        }
        return default;
    }

    // 플레이어 Index를 단서로 PlayerData(ClientId 포함 여러 플레이어 데이터)를 찾는 메소드
    public PlayerInGameData GetPlayerDataByPlayerIndex(int playerIndex)
    {
        if (playerIndex >= currentPlayers.Count)
        {
            Debug.Log($"playerIndex is wrong. playerIndex:{playerIndex}, listCount: {currentPlayers.Count}");
        }
        return currentPlayers[playerIndex];
    }

    // 플레이어 clientID를 단서로 player Index를 찾는 메소드
    public int GetPlayerDataListIndexByClientId(ulong clientId)
    {
        for (int i = 0; i < currentPlayers.Count; i++)
        {
            if (currentPlayers[i].clientId == clientId)
            {
                return i;
            }
        }
        return -1;
    }

    public NetworkList<PlayerInGameData> GetCurrentPlayers()
    {
        return currentPlayers;
    }

    public byte GetCurrentPlayerCount()
    {
        return (byte)currentPlayers.Count;
    }

    public void SetPlayerDataByClientId(ulong clientId, PlayerInGameData newPlayerData)
    {
        currentPlayers[GetPlayerDataListIndexByClientId(clientId)] = newPlayerData;
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddPlayerServerRPC(PlayerInGameData playerData, ServerRpcParams serverRpcParams = default)
    {
        // 해당 플레이어 데이터 존재 여부 확인 
        if (GetPlayerDataListIndexByClientId(serverRpcParams.Receive.SenderClientId) != -1) return;

        PlayerInGameData newPlayer = new PlayerInGameData
        {
            clientId = serverRpcParams.Receive.SenderClientId,
            // 접속시간 기록
            connectionTime = DateTime.Now,
            characterClass = playerData.characterClass,
            playerGameState = PlayerGameState.Playing,
            playerName = playerData.playerName,
            isAI = false
            // HP는 게임이 시작되면 OnNetworkSpawn때 각자 직업에 따라 SetPlayerHP로 보고함.
        };
        AddPlayer(newPlayer);
    }

    #region Player Score Management
    /// <summary>
    /// 플레이어 스코어 추가.
    /// </summary>
    /// <param name="clientID"></param>
    /// <param name="score"></param>
    public void AddPlayerScore(ulong clientID, int score)
    {
        PlayerInGameData playerData = GetPlayerDataByClientId(clientID);

        playerData.score += score;

        SetPlayerDataByClientId(clientID, playerData);       
    }

    /// <summary>
    /// 플레이어 스코어 얻기.
    /// </summary>
    /// <param name="clientID"></param>
    /// <returns></returns>
    public int GetPlayerScore(ulong clientID)
    {
        Debug.Log($"GetPlayerScore player{clientID} requested. {GetPlayerDataByClientId(clientID).score}");
        return GetPlayerDataByClientId(clientID).score;
    }
    #endregion
}
