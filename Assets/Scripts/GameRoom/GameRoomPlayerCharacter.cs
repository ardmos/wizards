using UnityEngine;
/// <summary>
///     /// ���� ��� ����. ���� ����
///     
/// /// GameRoom�� ĳ���� ���� ����
/// 1. player index�� ���� Player Visual Prefab�� ������Ʈ ���ݴϴ�.
/// 2. ĳ���� �Ӹ� ���� 'READY' UI�� ������Ʈ ���ݴϴ�.
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
        GameMatchReadyManager.Instance.OnClintPlayerReadyDictionaryChanged += OnReadyChanged;
        GameMultiplayer.Instance.OnPlayerListOnServerChanged += OnServerPlayerListChanged;
    }

    private void Start()
    {
        readyGameObject.SetActive(false);
    }

    private void OnServerPlayerListChanged(object sender, System.EventArgs e)
    {
        //Debug.Log($"OnServerPlayerListChanged playerIndex: {playerIndex}, playerDataNetworkList.Count: {GameMultiplayer.Instance.GetPlayerDataNetworkList().Count}");
        UpdatePlayerCharacter();
    }

    private void OnReadyChanged(object sender, System.EventArgs e)
    {
        UpdatePlayerReadyUI();
    }

    private void OnDestroy()
    {
        GameMatchReadyManager.Instance.OnClintPlayerReadyDictionaryChanged -= OnReadyChanged;
        GameMultiplayer.Instance.OnPlayerListOnServerChanged -= OnServerPlayerListChanged;
    }

    /// <summary>
    /// Player Visual Prefab ������Ʈ
    /// </summary>
    private void UpdatePlayerCharacter()
    {      
        if (GameMultiplayer.Instance.IsPlayerIndexConnected(playerIndex))
        {
            Debug.Log(nameof(UpdatePlayerCharacter) + $"Player{playerIndex} Connected");
            ShowPlayerCharacter();
        }
        else
            HidePlayerCharacter(); 
    }
    private void ShowPlayerCharacter()
    {
        gameObject.SetActive(true);
        ShowCharacter3DVisual();
    }
    private void HidePlayerCharacter()
    {
        gameObject.SetActive(false);       
        Destroy(playerObject);
        playerObject = null;
    }
    // ĳ���� �����ֱ�
    /// <summary>
    /// Player�� ������ ĳ������ ���־��� ������ �����ݴϴ�.
    /// �� ���� 
    /// 1. �κ������ PlayerProfileData�� �������� Ŭ������ ����
    /// 2. ������ ������ GameMultiplayer�� NetworkList�� playerDataList�� ����.
    /// 3. �ʿ��� �� playerIndex�� clientID�� ������ ���.
    /// </summary>
    public void ShowCharacter3DVisual()
    {
        if (playerObject != null)
        {
            //Debug.Log($"player{playerIndex}'s visual character is already exist.");
            return;
        }
        //Debug.Log($"ShowCharacter3DVisual playerIndex:{playerIndex}, class:{GameMultiplayer.Instance.GetPlayerDataFromPlayerIndex(playerIndex).playerClass}");
        playerObject = Instantiate(GetCurrentPlayerCharacterPrefab(GameMultiplayer.Instance.GetPlayerDataFromPlayerIndex(playerIndex).characterClass));
        //Debug.Log($"playerObject: {playerObject.name}");
        playerObject.transform.SetParent(transform, false);
        playerObject.transform.localPosition = Vector3.zero;
    }

    private GameObject GetCurrentPlayerCharacterPrefab(CharacterClass playerClass)
    {
        GameObject playerPrefab = null;
        switch (playerClass)
        {
            case CharacterClass.Wizard:
                playerPrefab = GameAssets.instantiate.wizard_Male_ForLobby;
                break;
            case CharacterClass.Knight:
                playerPrefab = GameAssets.instantiate.knight_Male_ForLobby;
                break;
            default:
                break;
        }
        return playerPrefab;
    }

    /// <summary>
    /// Ready UI ������Ʈ 
    /// </summary>
    private void UpdatePlayerReadyUI()
    {
        if(GameMultiplayer.Instance.IsPlayerIndexConnected(playerIndex))
        {
            PlayerInGameData playerData = GameMultiplayer.Instance.GetPlayerDataFromPlayerIndex(playerIndex);
            readyGameObject.SetActive(GameMatchReadyManager.Instance.IsPlayerReady(playerData.clientId));
        }
        else
        {
            readyGameObject.SetActive(false);
        }
    }
}
