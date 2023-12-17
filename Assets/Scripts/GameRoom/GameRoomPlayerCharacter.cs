using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// /// GameRoom의 캐릭터 상태 관리
/// GameRoom Scene에서 현 캐릭터오브젝트의 표시 여부를 조절하는 스크립트
/// </summary>
public class GameRoomPlayerCharacter : MonoBehaviour
{
    [SerializeField] private int playerIndex;
    [SerializeField] private GameObject readyGameObject;    

    /// <summary>
    /// GameRoom씬에서 사용되는 Player 캐릭터 프리팹의 스크립트로
    /// 각 Player들이 선택한 영웅의 3D Visual 프리팹을 자식오브젝트로 가져와 보여주는 역할을 한다. 
    /// 1. Player들이 선택한 영웅의 Prefab 가져와 Prefab을 자식 오브젝트로 인스턴시에이트 하기
    /// </summary>
    // 캐릭터 보여주기
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

    // 화면에 캐릭터 표시 여부 결정. 내부 기능이 좀 반복되는면이 있어보인다. Ready와 오브젝트 Show를 분리할 수 있어보임. 
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
        // 화면에 레디상태 표시 여부 결정
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

    // 캐릭터 보여주기
    /// <summary>
    /// Player가 선택한 캐릭터의 비주얼을 가져와 보여줍니다.
    /// 총 과정 
    /// 1. 로비씬에서 PlayerProfileData에 선택중인 클래스를 저장
    /// 2. 서버가 열리면 GameMultiplayer의 NetworkList인 playerDataList에 저장.
    /// 3. 필요할 때 playerIndex나 clientID로 꺼내서 사용.
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
