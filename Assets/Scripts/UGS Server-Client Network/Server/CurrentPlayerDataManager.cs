using System;
using System.Diagnostics;
using Unity.Netcode;

/// <summary>
/// ���� ���� ���� �÷��̾���� �����͸� �����ϴ� Ŭ�����Դϴ�.
/// </summary>
public class CurrentPlayerDataManager : NetworkBehaviour, ICleanable
{
    #region Singleton
    // CurrentPlayerDataManager�� �̱��� �ν��Ͻ��Դϴ�.
    public static CurrentPlayerDataManager Instance { get; private set; }
    #endregion

    #region Events
    // ������ �÷��̾� ����Ʈ�� ����� �� �߻��ϴ� �̺�Ʈ�Դϴ�
    public event EventHandler OnCurrentPlayerListOnServerChanged;
    #endregion

    #region Fields
    // ������ �������� �÷��̾���� �����Ͱ� ��� ��Ʈ��ũ ����Ʈ
    private NetworkList<PlayerInGameData> currentPlayers = new NetworkList<PlayerInGameData>();
    #endregion

    #region Unity Lifecycle
    /// <summary>
    /// �̱��� �ν��Ͻ��� �ʱ�ȭ�ϰ� �� ���� �Ŵ����� ����մϴ�.
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
    /// ��Ʈ��ũ ��ü�� ������ �� ȣ��˴ϴ�. �÷��̾� ����Ʈ�� �ʱ�ȭ�ϰ� �̺�Ʈ�� ����մϴ�.
    /// </summary>
    public override void OnNetworkSpawn()
    {
        currentPlayers.OnListChanged += OnCurrentPlayerListChanged; // ����Ʈ ���� �̺�Ʈ ���
    }

    /// <summary>
    /// ��Ʈ��ũ ��ü�� ������ �� ȣ��˴ϴ�. �̺�Ʈ �����ʸ� �����մϴ�.
    /// </summary>
    public override void OnNetworkDespawn()
    {
        currentPlayers.OnListChanged -= OnCurrentPlayerListChanged; // ����Ʈ ���� �̺�Ʈ ����
        currentPlayers = null;
    }

    /// <summary>
    /// ��ü�� �ı��� �� ȣ��˴ϴ�. �� ���� �Ŵ������� ����� �����մϴ�.
    /// </summary>
    public override void OnDestroy()
    {
        SceneCleanupManager.UnregisterCleanableObject(this);
    }
    #endregion

    #region Player List Management
    /// <summary>
    /// ���� RPC�� ���� ���ο� �÷��̾ �߰��մϴ�.
    /// </summary>
    /// <param name="playerData">�߰��� �÷��̾��� ������</param>
    [ServerRpc(RequireOwnership = false)]
    public void AddPlayerServerRPC(PlayerInGameData playerData, ServerRpcParams serverRpcParams = default)
    {
        Logger.Log($"AddPlayerServerRPC IsClient:{IsClient}, IsServer:{IsServer}, IsHost:{IsHost}");
        if (IsClientIdExists(serverRpcParams.Receive.SenderClientId))
        {
            Logger.LogError($"�̹� �����ϴ� �÷��̾��Դϴ�. �߰��� �� �����ϴ�. Ŭ���̾�ƮID: {serverRpcParams.Receive.SenderClientId}");
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
    /// ���ο� �÷��̾ �߰��մϴ�.
    /// </summary>
    /// <param name="playerData">�߰��� �÷��̾��� ������</param>
    public void AddPlayer(PlayerInGameData playerData) => currentPlayers.Add(playerData);

    /// <summary>
    /// Ư�� Ŭ���̾�Ʈ ID�� �ش��ϴ� �÷��̾ �����մϴ�.
    /// </summary>
    /// <param name="clientId">������ �÷��̾��� Ŭ���̾�Ʈ ID</param>
    public void RemovePlayer(ulong clientId)
    {
        if (!IsClientIdExists(clientId)) return;

        currentPlayers.RemoveAt(GetPlayerDataListIndexByClientId(clientId));
    }

    /// <summary>
    /// ��� �÷��̾ �����մϴ�.
    /// </summary>
    public void RemoveAllPlayers() => currentPlayers.Clear();
    #endregion

    #region Player Data Management
    /// <summary>
    /// Ŭ���̾�Ʈ ID�� �÷��̾� �����͸� ã���ϴ�.
    /// </summary>
    /// <param name="clientId">ã���� �ϴ� �÷��̾��� Ŭ���̾�Ʈ ID</param>
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
    /// �÷��̾� �ε����� �÷��̾� �����͸� ã���ϴ�.
    /// </summary>
    /// <param name="playerIndex">ã���� �ϴ� �÷��̾��� �ε���</param>
    /// <returns>������ true�� playerData, ���н� false�� playerData�� ��ȯ</returns>
    public bool TryGetPlayerDataByPlayerIndex(int playerIndex, out PlayerInGameData playerData)
    {
        if (playerIndex < 0 || playerIndex >= currentPlayers.Count)
        {
            Logger.LogError($"�÷��̾� �ε����� ��ȿ���� �ʽ��ϴ�. �÷��̾� �ε���:{playerIndex}");
            playerData = default;
            return false;
        }
        playerData = currentPlayers[playerIndex];
        return true;
    }

    /// <summary>
    /// Ŭ���̾�Ʈ ID�� �÷��̾� �ε����� ã���ϴ�.
    /// </summary>
    /// <param name="clientId">ã���� �ϴ� Ŭ���̾�ƮID</param>
    /// <returns>������ �÷��̾� �ε��� ��ȯ, ���н� -1 ��ȯ</returns>
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
    /// ���� ���� ���� ��� �÷��̾��� ������ ����Ʈ�� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>�÷��̾� �������� NetworkList</returns>
    public NetworkList<PlayerInGameData> GetCurrentPlayers() => currentPlayers;

    /// <summary>
    /// ���� ���� ���� �÷��̾��� ���� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>�÷��̾� �� (byte ����)</returns>
    public byte GetCurrentPlayerCount() => (byte)currentPlayers.Count;

    /// <summary>
    /// Ư�� Ŭ���̾�Ʈ ID�� �ش��ϴ� �÷��̾��� �����͸� �����մϴ�.
    /// </summary>
    /// <param name="clientId">������ �÷��̾��� Ŭ���̾�Ʈ ID</param>
    /// <param name="newPlayerData">���ο� �÷��̾� ������</param>
    public void SetPlayerDataByClientId(ulong clientId, PlayerInGameData newPlayerData)
    {
        if (!IsClientIdExists(clientId)) return;

        currentPlayers[GetPlayerDataListIndexByClientId(clientId)] = newPlayerData;
    }

    /// <summary>
    /// �־��� Ŭ���̾�Ʈ ID�� ���� �÷��̾ �����ϴ��� Ȯ���մϴ�.
    /// </summary>
    /// <param name="clientId">Ȯ���� Ŭ���̾�Ʈ ID</param>
    /// <returns>�÷��̾ �����ϸ� true, �׷��� ������ false</returns>
    private bool IsClientIdExists(ulong clientId)
    {
        bool clientIdExists = GetPlayerDataListIndexByClientId(clientId) != -1;
        if (!clientIdExists) Logger.Log($"�ش� Ŭ���̾�ƮID�� ���� �÷��̾ �����ϴ�: {clientId}");

        return clientIdExists;
    }
    #endregion

    #region Player Score Management
    /// <summary>
    /// �÷��̾� ���ھ �߰��մϴ�.
    /// </summary>
    /// <param name="clientId">�÷��̾��� Ŭ���̾�Ʈ ID</param>
    /// <param name="score">�߰��� ���ھ�</param>
    public void AddPlayerScore(ulong clientId, int score)
    {
        PlayerInGameData playerData = GetPlayerDataByClientId(clientId);
        playerData.score += score;
        SetPlayerDataByClientId(clientId, playerData);
    }

    /// <summary>
    /// �÷��̾� ���ھ ����ϴ�.
    /// </summary>
    /// <param name="clientId">�÷��̾��� Ŭ���̾�Ʈ ID</param>
    public int GetPlayerScore(ulong clientId) => GetPlayerDataByClientId(clientId).score;
    #endregion

    #region Event Handlers
    /// <summary>
    /// �÷��̾� ����Ʈ�� ����� �� ȣ��Ǵ� �̺�Ʈ �ڵ鷯�Դϴ�.
    /// </summary>
    /// <param name="changeEvent">���� �̺�Ʈ ����</param>
    private void OnCurrentPlayerListChanged(NetworkListEvent<PlayerInGameData> changeEvent)
    {
        OnCurrentPlayerListOnServerChanged?.Invoke(this, EventArgs.Empty);
    }
    #endregion

    #region ICleanable ����
    /// <summary>
    /// ICleanable �������̽� ����. ��ü�� �����մϴ�.
    /// �� �޼���� �� ��ȯ �� ȣ��Ǿ� ���� ��ü�� �ı��մϴ�.
    /// </summary>
    public void Cleanup()
    {
        Destroy(gameObject);
    }
    #endregion
}