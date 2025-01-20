using System;
using System.Diagnostics;
using Unity.Netcode;

/// <summary>
/// 현재 접속 중인 플레이어들의 데이터를 관리하는 클래스입니다.
/// </summary>
public class CurrentPlayerDataManager : NetworkBehaviour, ICleanable
{
    #region Singleton
    // CurrentPlayerDataManager의 싱글톤 인스턴스입니다.
    public static CurrentPlayerDataManager Instance { get; private set; }
    #endregion

    #region Events
    // 서버의 플레이어 리스트가 변경될 때 발생하는 이벤트입니다
    public event EventHandler OnCurrentPlayerListOnServerChanged;
    #endregion

    #region Fields
    // 서버에 접속중인 플레이어들의 데이터가 담긴 네트워크 리스트
    private NetworkList<PlayerInGameData> currentPlayers = new NetworkList<PlayerInGameData>();
    #endregion

    #region Unity Lifecycle
    /// <summary>
    /// 싱글톤 인스턴스를 초기화하고 씬 정리 매니저에 등록합니다.
    /// </summary>
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneCleanupManager.RegisterCleanableObject(this);
        }
        else Destroy(gameObject);
    }

    /// <summary>
    /// 네트워크 객체가 스폰될 때 호출됩니다. 플레이어 리스트를 초기화하고 이벤트를 등록합니다.
    /// </summary>
    public override void OnNetworkSpawn()
    {
        currentPlayers.OnListChanged += OnCurrentPlayerListChanged; // 리스트 변경 이벤트 등록
    }

    /// <summary>
    /// 네트워크 객체가 디스폰될 때 호출됩니다. 이벤트 리스너를 제거합니다.
    /// </summary>
    public override void OnNetworkDespawn()
    {
        currentPlayers.OnListChanged -= OnCurrentPlayerListChanged; // 리스트 변경 이벤트 해제
        currentPlayers = null;
    }

    /// <summary>
    /// 객체가 파괴될 때 호출됩니다. 씬 정리 매니저에서 등록을 해제합니다.
    /// </summary>
    public override void OnDestroy()
    {
        SceneCleanupManager.UnregisterCleanableObject(this);
    }
    #endregion

    #region Player List Management
    /// <summary>
    /// 서버 RPC를 통해 새로운 플레이어를 추가합니다.
    /// </summary>
    /// <param name="playerData">추가할 플레이어의 데이터</param>
    [ServerRpc(RequireOwnership = false)]
    public void AddPlayerServerRPC(PlayerInGameData playerData, ServerRpcParams serverRpcParams = default)
    {
        Logger.Log($"AddPlayerServerRPC IsClient:{IsClient}, IsServer:{IsServer}, IsHost:{IsHost}");
        if (IsClientIdExists(serverRpcParams.Receive.SenderClientId))
        {
            Logger.LogError($"이미 존재하는 플레이어입니다. 추가할 수 업습니다. 클라이언트ID: {serverRpcParams.Receive.SenderClientId}");
            return;
        }

        PlayerInGameData newPlayer = new PlayerInGameData
        {
            clientId = serverRpcParams.Receive.SenderClientId,
            connectionTime = DateTime.Now,
            characterClass = playerData.characterClass,
            playerGameState = PlayerGameState.Playing,
            playerName = playerData.playerName,
            isAI = false
        };
        AddPlayer(newPlayer);
    }

    /// <summary>
    /// 새로운 플레이어를 추가합니다.
    /// </summary>
    /// <param name="playerData">추가할 플레이어의 데이터</param>
    public void AddPlayer(PlayerInGameData playerData) => currentPlayers.Add(playerData);

    /// <summary>
    /// 특정 클라이언트 ID에 해당하는 플레이어를 제거합니다.
    /// </summary>
    /// <param name="clientId">제거할 플레이어의 클라이언트 ID</param>
    public void RemovePlayer(ulong clientId)
    {
        if (!IsClientIdExists(clientId)) return;

        currentPlayers.RemoveAt(GetPlayerDataListIndexByClientId(clientId));
    }

    /// <summary>
    /// 모든 플레이어를 제거합니다.
    /// </summary>
    public void RemoveAllPlayers() => currentPlayers.Clear();
    #endregion

    #region Player Data Management
    /// <summary>
    /// 클라이언트 ID로 플레이어 데이터를 찾습니다.
    /// </summary>
    /// <param name="clientId">찾고자 하는 플레이어의 클라이언트 ID</param>
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

    /// <summary>
    /// 플레이어 인덱스로 플레이어 데이터를 찾습니다.
    /// </summary>
    /// <param name="playerIndex">찾고자 하는 플레이어의 인덱스</param>
    /// <returns>성공시 true와 playerData, 실패시 false와 playerData를 반환</returns>
    public bool TryGetPlayerDataByPlayerIndex(int playerIndex, out PlayerInGameData playerData)
    {
        if (playerIndex < 0 || playerIndex >= currentPlayers.Count)
        {
            Logger.LogError($"플레이어 인덱스가 유효하지 않습니다. 플레이어 인덱스:{playerIndex}");
            playerData = default;
            return false;
        }
        playerData = currentPlayers[playerIndex];
        return true;
    }

    /// <summary>
    /// 클라이언트 ID로 플레이어 인덱스를 찾습니다.
    /// </summary>
    /// <param name="clientId">찾고자 하는 클라이언트ID</param>
    /// <returns>성공시 플레이어 인덱스 반환, 실패시 -1 반환</returns>
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

    /// <summary>
    /// 현재 접속 중인 모든 플레이어의 데이터 리스트를 반환합니다.
    /// </summary>
    /// <returns>플레이어 데이터의 NetworkList</returns>
    public NetworkList<PlayerInGameData> GetCurrentPlayers() => currentPlayers;

    /// <summary>
    /// 현재 접속 중인 플레이어의 수를 반환합니다.
    /// </summary>
    /// <returns>플레이어 수 (byte 형식)</returns>
    public byte GetCurrentPlayerCount() => (byte)currentPlayers.Count;

    /// <summary>
    /// 특정 클라이언트 ID에 해당하는 플레이어의 데이터를 설정합니다.
    /// </summary>
    /// <param name="clientId">설정할 플레이어의 클라이언트 ID</param>
    /// <param name="newPlayerData">새로운 플레이어 데이터</param>
    public void SetPlayerDataByClientId(ulong clientId, PlayerInGameData newPlayerData)
    {
        if (!IsClientIdExists(clientId)) return;

        currentPlayers[GetPlayerDataListIndexByClientId(clientId)] = newPlayerData;
    }

    /// <summary>
    /// 주어진 클라이언트 ID를 가진 플레이어가 존재하는지 확인합니다.
    /// </summary>
    /// <param name="clientId">확인할 클라이언트 ID</param>
    /// <returns>플레이어가 존재하면 true, 그렇지 않으면 false</returns>
    private bool IsClientIdExists(ulong clientId)
    {
        bool clientIdExists = GetPlayerDataListIndexByClientId(clientId) != -1;
        if (!clientIdExists) Logger.Log($"해당 클라이언트ID를 가진 플레이어가 없습니다: {clientId}");

        return clientIdExists;
    }
    #endregion

    #region Player Score Management
    /// <summary>
    /// 플레이어 스코어를 추가합니다.
    /// </summary>
    /// <param name="clientId">플레이어의 클라이언트 ID</param>
    /// <param name="score">추가할 스코어</param>
    public void AddPlayerScore(ulong clientId, int score)
    {
        PlayerInGameData playerData = GetPlayerDataByClientId(clientId);
        playerData.score += score;
        SetPlayerDataByClientId(clientId, playerData);
    }

    /// <summary>
    /// 플레이어 스코어를 얻습니다.
    /// </summary>
    /// <param name="clientId">플레이어의 클라이언트 ID</param>
    public int GetPlayerScore(ulong clientId) => GetPlayerDataByClientId(clientId).score;
    #endregion

    #region Event Handlers
    /// <summary>
    /// 플레이어 리스트가 변경될 때 호출되는 이벤트 핸들러입니다.
    /// </summary>
    /// <param name="changeEvent">변경 이벤트 정보</param>
    private void OnCurrentPlayerListChanged(NetworkListEvent<PlayerInGameData> changeEvent)
    {
        OnCurrentPlayerListOnServerChanged?.Invoke(this, EventArgs.Empty);
    }
    #endregion

    #region ICleanable 구현
    /// <summary>
    /// ICleanable 인터페이스 구현. 객체를 정리합니다.
    /// 이 메서드는 씬 전환 시 호출되어 현재 객체를 파괴합니다.
    /// </summary>
    public void Cleanup()
    {
        Destroy(gameObject);
    }
    #endregion
}