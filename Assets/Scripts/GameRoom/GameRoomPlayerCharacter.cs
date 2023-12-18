using UnityEngine;
/// <summary>
/// /// GameRoom�� ĳ���� ���� ����
/// GameRoom Scene���� �� ĳ���Ϳ�����Ʈ�� ǥ�� ���θ� �����ϴ� ��ũ��Ʈ
/// </summary>
public class GameRoomPlayerCharacter : MonoBehaviour
{
    [SerializeField] private int playerIndex;
    [SerializeField] private GameObject readyGameObject;    

    /// <summary>
    /// GameRoom������ ���Ǵ� Player ĳ���� �������� ��ũ��Ʈ��
    /// �� Player���� ������ ������ 3D Visual �������� �ڽĿ�����Ʈ�� ������ �����ִ� ������ �Ѵ�. 
    /// 1. Player���� ������ ������ Prefab ������ Prefab�� �ڽ� ������Ʈ�� �ν��Ͻÿ���Ʈ �ϱ�
    /// </summary>
    // ĳ���� �����ֱ�
    [SerializeField] private GameObject playerObject;

    private void Awake()
    {
        GameRoomReadyManager.Instance.OnClintPlayerReadyDictionaryChanged += OnReadyChanged;
        GameMultiplayer.Instance.OnServerPlayerListChanged += OnServerPlayerListChanged;
    }

    private void OnServerPlayerListChanged(object sender, System.EventArgs e)
    {
        Debug.Log($"OnServerPlayerListChanged playerIndex: {playerIndex}, playerDataNetworkList.Count: {GameMultiplayer.Instance.GetPlayerDataNetworkList().Count}");
        UpdatePlayer();
    }

    private void OnReadyChanged(object sender, System.EventArgs e)
    {
        UpdatePlayerReadyUI();
    }

    private void OnDestroy()
    {
        GameRoomReadyManager.Instance.OnClintPlayerReadyDictionaryChanged -= OnReadyChanged;
        GameMultiplayer.Instance.OnServerPlayerListChanged -= OnServerPlayerListChanged;
    }

    // ȭ�鿡 ĳ���� ǥ�� ���� ����. ���� ����� �� �ݺ��Ǵ¸��� �־�δ�. Ready�� ������Ʈ Show�� �и��� �� �־��. 
    private void UpdatePlayer()
    {
        Debug.Log(nameof(UpdatePlayer) + $"IsPlayer{playerIndex} Connected?: {GameMultiplayer.Instance.IsPlayerIndexConnected(playerIndex)}");
        if (GameMultiplayer.Instance.IsPlayerIndexConnected(playerIndex))
        {
            Show();
        }
        else
            Hide(); 
    }

    private void UpdatePlayerReadyUI()
    {
        // ȭ�鿡 ������� ǥ�� ���� ����
        PlayerData playerData = GameMultiplayer.Instance.GetPlayerDataFromPlayerIndex(playerIndex);
        readyGameObject.SetActive(GameRoomReadyManager.Instance.IsPlayerReady(playerData.clientId));
    }

    private void Show()
    {
        gameObject.SetActive(true);
        ShowCharacter3DVisual(playerIndex);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
        Destroy(playerObject);
    }

    // ĳ���� �����ֱ�
    /// <summary>
    /// Player�� ������ ĳ������ ���־��� ������ �����ݴϴ�.
    /// �� ���� 
    /// 1. �κ������ PlayerProfileData�� �������� Ŭ������ ����
    /// 2. ������ ������ GameMultiplayer�� NetworkList�� playerDataList�� ����.
    /// 3. �ʿ��� �� playerIndex�� clientID�� ������ ���.
    /// </summary>
    public void ShowCharacter3DVisual(int playerIndex)
    {
        Debug.Log($"ShowCharacter3DVisual playerIndex:{playerIndex}, class:{GameMultiplayer.Instance.GetPlayerDataFromPlayerIndex(playerIndex).playerClass}");
        
        playerObject = Instantiate(GetCurrentPlayerCharacterPrefab(GameMultiplayer.Instance.GetPlayerDataFromPlayerIndex(playerIndex).playerClass));
        Debug.Log($"playerObject: {playerObject.name}");
        playerObject.transform.SetParent(transform, false);
        playerObject.transform.localPosition = Vector3.zero;
    }

    private GameObject GetCurrentPlayerCharacterPrefab(CharacterClasses.Class playerClass)
    {
        GameObject playerPrefab = null;
        switch (playerClass)
        {
            case CharacterClasses.Class.Wizard:
                playerPrefab = GameAssets.instantiate.wizard_Male_ForLobby;
                break;
            case CharacterClasses.Class.Knight:
                playerPrefab = GameAssets.instantiate.knight_Male_ForLobby;
                break;
            default:
                break;
        }
        return playerPrefab;
    }
}
