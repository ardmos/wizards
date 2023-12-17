using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UI;
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
        GameRoomReadyManager.Instance.OnReadyChanged += GameRoomReadyManager_OnReadyChanged;

        GameMultiplayer.Instance.OnPlayerDataNetworkListChanged += Instance_OnPlayerDataNetworkListChanged;
    }

    private void Instance_OnPlayerDataNetworkListChanged(object sender, System.EventArgs e)
    {
        Debug.Log($"Instance_OnPlayerDataNetworkListChanged playerIndex: {playerIndex}, playerDataNetworkList.Count: {GameMultiplayer.Instance.GetPlayerDataNetworkList().Count}");
        UpdatePlayer();
    }

    private void GameRoomReadyManager_OnReadyChanged(object sender, System.EventArgs e)
    {
        UpdatePlayerReadyUI();
    }

    private void OnDestroy()
    {       
        GameRoomReadyManager.Instance.OnReadyChanged -= GameRoomReadyManager_OnReadyChanged;
    }

    private void GameRoomCharacterManager_OnPlayerCharacterUpdated(object sender, System.EventArgs e)
    {
        //UpdatePlayer();
        //Debug.Log($"GameRoomCharacterManager_OnPlayerCharacterUpdated sender: {sender}");
    }

    // ȭ�鿡 ĳ���� ǥ�� ���� ����. ���� ����� �� �ݺ��Ǵ¸��� �־�δ�. Ready�� ������Ʈ Show�� �и��� �� �־��. 
    private void UpdatePlayer()
    {
        //Debug.Log(nameof(UpdatePlayer) + $"IsPlayer{playerIndex} Connected?: {GameMultiplayer.Instance.IsPlayerIndexConnected(playerIndex)}");
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
        Debug.Log($"playerObject: {playerObject.name}");
        playerObject.transform.SetParent(transform, false);
        playerObject.transform.localPosition = Vector3.zero;
    }
}
