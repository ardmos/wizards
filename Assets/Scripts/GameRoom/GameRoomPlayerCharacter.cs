using UnityEngine;
using UnityEngine.UI;
/// <summary>
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

    private void Start()
    {
        GameRoomCharacterManager.instance.OnPlayerCharacterUpdated += GameRoomCharacterManager_OnPlayerCharacterUpdated;
        GameRoomReadyManager.Instance.OnReadyChanged += GameRoomReadyManager_OnReadyChanged;

        UpdatePlayer();
    }

    private void GameRoomReadyManager_OnReadyChanged(object sender, System.EventArgs e)
    {
        UpdatePlayerReadyUI();
    }

    private void OnDestroy()
    {
        GameRoomCharacterManager.instance.OnPlayerCharacterUpdated -= GameRoomCharacterManager_OnPlayerCharacterUpdated;
        GameRoomReadyManager.Instance.OnReadyChanged -= GameRoomReadyManager_OnReadyChanged;
    }

    private void GameRoomCharacterManager_OnPlayerCharacterUpdated(object sender, System.EventArgs e)
    {
        UpdatePlayer();
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
        gameRoomCharacterPrefab.ShowCharacter3DVisual(playerIndex);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
        gameRoomCharacterPrefab.HideCharacter3DVisual();
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
        //Debug.Log($"LoadCharacter3DVisual playerIndex:{playerIndex}, playerObject:{playerObject}");
        playerObject = Instantiate(GameMultiplayer.Instance.GetPlayerClassPrefabByPlayerIndex_NotForGameSceneObject(playerIndex));
        playerObject.transform.SetParent(transform, false);
        playerObject.transform.localPosition = Vector3.zero;
    }

    public void HideCharacter3DVisual()
    {
        Destroy(playerObject);
        playerObject = null;
    }
}
