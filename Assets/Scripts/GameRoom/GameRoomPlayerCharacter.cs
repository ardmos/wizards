using UnityEngine;
/// <summary>
///     /// 이제 사용 안함. 삭제 가능
///     
/// /// GameRoom의 캐릭터 상태 관리
/// 1. player index에 맞춰 Player Visual Prefab을 업데이트 해줍니다.
/// 2. 캐릭터 머리 위에 'READY' UI를 업데이트 해줍니다.
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
    /// Player Visual Prefab 업데이트
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
    // 캐릭터 보여주기
    /// <summary>
    /// Player가 선택한 캐릭터의 비주얼을 가져와 보여줍니다.
    /// 총 과정 
    /// 1. 로비씬에서 PlayerProfileData에 선택중인 클래스를 저장
    /// 2. 서버가 열리면 GameMultiplayer의 NetworkList인 playerDataList에 저장.
    /// 3. 필요할 때 playerIndex나 clientID로 꺼내서 사용.
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
    /// Ready UI 업데이트 
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
